#pragma warning disable CS8620 // Nullability mismatch in NSubstitute generic type inference

using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Portfolio.Application.Common;
using Portfolio.Application.DTOs;
using Portfolio.Application.Interfaces;
using Portfolio.Application.Services;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Interfaces;
using Xunit;

namespace Portfolio.Application.Tests.Services;

public class ProjectServiceTests
{
    private readonly IProjectRepository _projectRepository;
    private readonly IHtmlSanitizerService _htmlSanitizer;
    private readonly IHybridCacheService _cache;
    private readonly ProjectService _sut;

    public ProjectServiceTests()
    {
        _projectRepository = Substitute.For<IProjectRepository>();
        _htmlSanitizer = Substitute.For<IHtmlSanitizerService>();
        _cache = Substitute.For<IHybridCacheService>();

        var cachingOptions = Options.Create(new CachingOptions
        {
            ProjectMinutes = 30,
            PublishedListMinutes = 15
        });

        // Default: sanitizer returns input unchanged
        _htmlSanitizer.Sanitize(Arg.Any<string>()).Returns(callInfo => callInfo.ArgAt<string>(0));

        _sut = new ProjectService(_projectRepository, _htmlSanitizer, _cache, cachingOptions);
    }

    #region GetPublishedProjectsAsync

    [Fact]
    public async Task GetPublishedProjectsAsync_ReturnsCachedData_WhenCacheHit()
    {
        // Arrange
        var cachedProjects = new List<ProjectDto>
        {
            new() { Id = Guid.NewGuid(), Title = "Cached Project", Slug = "cached-project" }
        };

        _cache.GetOrCreateAsync(
            Arg.Is<string>(k => k == CacheKeys.ProjectsPublished),
            Arg.Any<Func<Task<IReadOnlyList<ProjectDto>>>>(),
            Arg.Any<TimeSpan?>(),
            Arg.Any<TimeSpan?>(),
            Arg.Any<CancellationToken>()
        ).Returns(Task.FromResult<IReadOnlyList<ProjectDto>?>(cachedProjects));

        // Act
        var result = await _sut.GetPublishedProjectsAsync();

        // Assert
        result.Should().BeEquivalentTo(cachedProjects);
        await _projectRepository.DidNotReceive().GetPublishedAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetPublishedProjectsAsync_CallsFactory_WhenCacheMiss()
    {
        // Arrange
        var projects = new List<Project>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Project 1",
                Slug = "project-1",
                Description = "Description",
                TechStack = "C#, Angular",
                CreatedAt = DateTime.UtcNow
            }
        };

        _projectRepository.GetPublishedAsync(Arg.Any<CancellationToken>()).Returns(projects);

        _cache.GetOrCreateAsync(
            Arg.Any<string>(),
            Arg.Any<Func<Task<IReadOnlyList<ProjectDto>>>>(),
            Arg.Any<TimeSpan?>(),
            Arg.Any<TimeSpan?>(),
            Arg.Any<CancellationToken>()
        ).Returns(callInfo => callInfo.ArgAt<Func<Task<IReadOnlyList<ProjectDto>>>>(1).Invoke());

        // Act
        var result = await _sut.GetPublishedProjectsAsync();

        // Assert
        result.Should().HaveCount(1);
        result[0].Title.Should().Be("Project 1");
        await _projectRepository.Received(1).GetPublishedAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region GetProjectBySlugAsync

    [Fact]
    public async Task GetProjectBySlugAsync_ReturnsProject_WhenSlugExists()
    {
        // Arrange
        var slug = "test-project";
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Title = "Test Project",
            Slug = slug,
            Summary = "Summary",
            Description = "Description",
            TechStack = "C#",
            IsPublished = true,
            CreatedAt = DateTime.UtcNow
        };

        _projectRepository.GetBySlugAsync(slug, Arg.Any<CancellationToken>()).Returns(project);

        _cache.GetOrCreateAsync(
            Arg.Any<string>(),
            Arg.Any<Func<Task<ProjectDto>>>(),
            Arg.Any<TimeSpan?>(),
            Arg.Any<TimeSpan?>(),
            Arg.Any<CancellationToken>()
        ).Returns(callInfo => callInfo.ArgAt<Func<Task<ProjectDto>>>(1).Invoke());

        // Act
        var result = await _sut.GetProjectBySlugAsync(slug);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("Test Project");
        result.Slug.Should().Be(slug);
    }

    [Fact]
    public async Task GetProjectBySlugAsync_ReturnsNull_WhenSlugDoesNotExist()
    {
        // Arrange
        _projectRepository.GetBySlugAsync("non-existent", Arg.Any<CancellationToken>())
            .Returns((Project?)null);

        _cache.GetOrCreateAsync(
            Arg.Any<string>(),
            Arg.Any<Func<Task<ProjectDto>>>(),
            Arg.Any<TimeSpan?>(),
            Arg.Any<TimeSpan?>(),
            Arg.Any<CancellationToken>()
        ).Returns(callInfo => callInfo.ArgAt<Func<Task<ProjectDto>>>(1).Invoke());

        // Act
        var result = await _sut.GetProjectBySlugAsync("non-existent");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region CreateProjectAsync

    [Fact]
    public async Task CreateProjectAsync_SanitizesDescription()
    {
        // Arrange
        var dto = new ProjectCreateDto
        {
            Title = "New Project",
            Slug = "new-project",
            Summary = "Summary",
            Description = "<script>alert('xss')</script><p>Hello</p>",
            TechStack = "C#",
            IsPublished = false
        };

        _htmlSanitizer.Sanitize(dto.Description).Returns("<p>Hello</p>");

        // Act
        var result = await _sut.CreateProjectAsync(dto);

        // Assert
        _htmlSanitizer.Received(1).Sanitize(dto.Description);
        result.Description.Should().Be("<p>Hello</p>");
    }

    [Fact]
    public async Task CreateProjectAsync_WhenPublished_SetsPublishedAt()
    {
        // Arrange
        var dto = new ProjectCreateDto
        {
            Title = "Published Project",
            Slug = "published-project",
            Summary = "Summary",
            Description = "<p>Description</p>",
            TechStack = "C#",
            IsPublished = true
        };

        // Act
        var result = await _sut.CreateProjectAsync(dto);

        // Assert
        result.IsPublished.Should().BeTrue();
        result.PublishedAt.Should().NotBeNull();
        result.PublishedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task CreateProjectAsync_WhenNotPublished_DoesNotSetPublishedAt()
    {
        // Arrange
        var dto = new ProjectCreateDto
        {
            Title = "Draft Project",
            Slug = "draft-project",
            Summary = "Summary",
            Description = "<p>Description</p>",
            TechStack = "C#",
            IsPublished = false
        };

        // Act
        var result = await _sut.CreateProjectAsync(dto);

        // Assert
        result.IsPublished.Should().BeFalse();
        result.PublishedAt.Should().BeNull();
    }

    [Fact]
    public async Task CreateProjectAsync_InvalidatesCache()
    {
        // Arrange
        var dto = new ProjectCreateDto
        {
            Title = "Project",
            Slug = "project",
            Summary = "Summary",
            Description = "Description",
            TechStack = "C#",
            IsPublished = false
        };

        // Act
        await _sut.CreateProjectAsync(dto);

        // Assert
        await _cache.Received(1).RemoveAsync(
            Arg.Is<string>(k => k == CacheKeys.ProjectsPublished),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateProjectAsync_CallsRepositoryAdd()
    {
        // Arrange
        var dto = new ProjectCreateDto
        {
            Title = "Project",
            Slug = "project",
            Summary = "Summary",
            Description = "Description",
            TechStack = "C#, Angular",
            SortOrder = 5,
            LiveUrl = "https://example.com",
            GitHubUrl = "https://github.com/example",
            IsPublished = false
        };

        // Act
        await _sut.CreateProjectAsync(dto);

        // Assert
        await _projectRepository.Received(1).AddAsync(
            Arg.Is<Project>(p =>
                p.Title == "Project" &&
                p.Slug == "project" &&
                p.TechStack == "C#, Angular" &&
                p.SortOrder == 5 &&
                p.LiveUrl == "https://example.com" &&
                p.GitHubUrl == "https://github.com/example"),
            Arg.Any<CancellationToken>());
    }

    #endregion

    #region UpdateProjectAsync

    [Fact]
    public async Task UpdateProjectAsync_ThrowsKeyNotFoundException_WhenProjectDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        _projectRepository.GetByIdAsync(id, Arg.Any<CancellationToken>())
            .Returns((Project?)null);

        var dto = new ProjectUpdateDto
        {
            Title = "Updated",
            Slug = "updated",
            Summary = "Summary",
            Description = "Description",
            TechStack = "C#"
        };

        // Act
        var act = () => _sut.UpdateProjectAsync(id, dto);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{id}*");
    }

    [Fact]
    public async Task UpdateProjectAsync_SanitizesDescription()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingProject = new Project
        {
            Id = id,
            Title = "Old",
            Slug = "old",
            Summary = "Old summary",
            Description = "Old description",
            TechStack = "C#",
            IsPublished = false,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        _projectRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(existingProject);

        var dto = new ProjectUpdateDto
        {
            Title = "Updated",
            Slug = "updated",
            Summary = "New summary",
            Description = "<script>bad</script><p>Good</p>",
            TechStack = "C#",
            IsPublished = false
        };

        _htmlSanitizer.Sanitize(dto.Description).Returns("<p>Good</p>");

        // Act
        var result = await _sut.UpdateProjectAsync(id, dto);

        // Assert
        _htmlSanitizer.Received(1).Sanitize(dto.Description);
        result.Description.Should().Be("<p>Good</p>");
    }

    [Fact]
    public async Task UpdateProjectAsync_SetsPublishedAt_OnPublishTransition()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingProject = new Project
        {
            Id = id,
            Title = "Draft",
            Slug = "draft",
            Summary = "Summary",
            Description = "Description",
            TechStack = "C#",
            IsPublished = false,
            PublishedAt = null,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        _projectRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(existingProject);

        var dto = new ProjectUpdateDto
        {
            Title = "Now Published",
            Slug = "now-published",
            Summary = "Summary",
            Description = "Description",
            TechStack = "C#",
            IsPublished = true
        };

        // Act
        var result = await _sut.UpdateProjectAsync(id, dto);

        // Assert
        result.IsPublished.Should().BeTrue();
        result.PublishedAt.Should().NotBeNull();
        result.PublishedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task UpdateProjectAsync_DoesNotChangePublishedAt_WhenAlreadyPublished()
    {
        // Arrange
        var id = Guid.NewGuid();
        var originalPublishedAt = DateTime.UtcNow.AddDays(-5);
        var existingProject = new Project
        {
            Id = id,
            Title = "Published",
            Slug = "published",
            Summary = "Summary",
            Description = "Description",
            TechStack = "C#",
            IsPublished = true,
            PublishedAt = originalPublishedAt,
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        };

        _projectRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(existingProject);

        var dto = new ProjectUpdateDto
        {
            Title = "Updated Published",
            Slug = "updated-published",
            Summary = "New Summary",
            Description = "New Description",
            TechStack = "C#, Angular",
            IsPublished = true
        };

        // Act
        var result = await _sut.UpdateProjectAsync(id, dto);

        // Assert
        result.PublishedAt.Should().Be(originalPublishedAt);
    }

    [Fact]
    public async Task UpdateProjectAsync_InvalidatesCache()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingProject = new Project
        {
            Id = id,
            Title = "Project",
            Slug = "project",
            Summary = "Summary",
            Description = "Description",
            TechStack = "C#",
            IsPublished = false,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        _projectRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(existingProject);

        var dto = new ProjectUpdateDto
        {
            Title = "Updated",
            Slug = "updated-slug",
            Summary = "Summary",
            Description = "Description",
            TechStack = "C#",
            IsPublished = false
        };

        // Act
        await _sut.UpdateProjectAsync(id, dto);

        // Assert
        await _cache.Received(1).RemoveAsync(CacheKeys.ProjectsPublished, Arg.Any<CancellationToken>());
        await _cache.Received(1).RemoveAsync(CacheKeys.ProjectBySlug("updated-slug"), Arg.Any<CancellationToken>());
    }

    #endregion

    #region DeleteProjectAsync

    [Fact]
    public async Task DeleteProjectAsync_ThrowsKeyNotFoundException_WhenProjectDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        _projectRepository.GetByIdAsync(id, Arg.Any<CancellationToken>())
            .Returns((Project?)null);

        // Act
        var act = () => _sut.DeleteProjectAsync(id);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{id}*");
    }

    [Fact]
    public async Task DeleteProjectAsync_InvalidatesCache()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingProject = new Project
        {
            Id = id,
            Title = "To Delete",
            Slug = "to-delete",
            Description = "Description",
            TechStack = "C#",
            IsPublished = true,
            CreatedAt = DateTime.UtcNow
        };

        _projectRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(existingProject);

        // Act
        await _sut.DeleteProjectAsync(id);

        // Assert
        await _projectRepository.Received(1).DeleteAsync(existingProject, Arg.Any<CancellationToken>());
        await _cache.Received(1).RemoveAsync(CacheKeys.ProjectsPublished, Arg.Any<CancellationToken>());
        await _cache.Received(1).RemoveAsync(CacheKeys.ProjectBySlug("to-delete"), Arg.Any<CancellationToken>());
    }

    #endregion
}
