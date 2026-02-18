using FluentAssertions;
using NSubstitute;
using Portfolio.Application.DTOs;
using Portfolio.Application.Interfaces;
using Portfolio.Application.Services;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Interfaces;
using Xunit;

namespace Portfolio.Application.Tests.Services;

public class LeadServiceTests
{
    private readonly ILeadRepository _leadRepository;
    private readonly IAdminNotificationService _notificationService;
    private readonly LeadService _sut;

    public LeadServiceTests()
    {
        _leadRepository = Substitute.For<ILeadRepository>();
        _notificationService = Substitute.For<IAdminNotificationService>();

        _sut = new LeadService(_leadRepository, _notificationService);
    }

    #region SubmitLeadAsync

    [Fact]
    public async Task SubmitLeadAsync_CreatesLead_WithIsReadFalse()
    {
        // Arrange
        var dto = new LeadSubmitDto
        {
            FullName = "John Doe",
            Email = "john@example.com",
            Phone = "+1234567890",
            Company = "Acme Inc",
            Message = "I would like to discuss a project.",
            CaptchaId = "captcha-id",
            CaptchaCode = "captcha-code"
        };

        // Act
        var result = await _sut.SubmitLeadAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.IsRead.Should().BeFalse();
        result.FullName.Should().Be("John Doe");
        result.Email.Should().Be("john@example.com");
        result.Phone.Should().Be("+1234567890");
        result.Company.Should().Be("Acme Inc");
        result.Message.Should().Be("I would like to discuss a project.");
        result.Id.Should().NotBeEmpty();
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        await _leadRepository.Received(1).AddAsync(
            Arg.Is<Lead>(l =>
                l.FullName == "John Doe" &&
                l.Email == "john@example.com" &&
                !l.IsRead),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SubmitLeadAsync_SendsNotification()
    {
        // Arrange
        var dto = new LeadSubmitDto
        {
            FullName = "Jane Smith",
            Email = "jane@example.com",
            Message = "Hello!",
            CaptchaId = "captcha-id",
            CaptchaCode = "captcha-code"
        };

        // Act
        await _sut.SubmitLeadAsync(dto);

        // Assert
        _ = _notificationService.Received(1).NotifyNewLeadAsync(
            "Jane Smith",
            "jane@example.com",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SubmitLeadAsync_StillSucceeds_WhenNotificationFails()
    {
        // Arrange
        var dto = new LeadSubmitDto
        {
            FullName = "John Doe",
            Email = "john@example.com",
            Message = "Contact me",
            CaptchaId = "captcha-id",
            CaptchaCode = "captcha-code"
        };

        _notificationService.NotifyNewLeadAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("Notification failed")));

        // Act
        var result = await _sut.SubmitLeadAsync(dto);

        // Assert — lead submission should succeed even if notification throws
        result.Should().NotBeNull();
        result.FullName.Should().Be("John Doe");
        await _leadRepository.Received(1).AddAsync(Arg.Any<Lead>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SubmitLeadAsync_SetsReadAtToNull()
    {
        // Arrange
        var dto = new LeadSubmitDto
        {
            FullName = "John Doe",
            Email = "john@example.com",
            Message = "Hello",
            CaptchaId = "captcha-id",
            CaptchaCode = "captcha-code"
        };

        // Act
        var result = await _sut.SubmitLeadAsync(dto);

        // Assert
        result.ReadAt.Should().BeNull();
    }

    #endregion

    #region GetAllLeadsAsync

    [Fact]
    public async Task GetAllLeadsAsync_ReturnsOrderedLeads()
    {
        // Arrange
        var leads = new List<Lead>
        {
            new()
            {
                Id = Guid.NewGuid(),
                FullName = "Lead 1",
                Email = "lead1@example.com",
                Message = "Message 1",
                IsRead = false,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new()
            {
                Id = Guid.NewGuid(),
                FullName = "Lead 2",
                Email = "lead2@example.com",
                Message = "Message 2",
                IsRead = true,
                ReadAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            }
        };

        _leadRepository.GetAllOrderedByDateAsync(Arg.Any<CancellationToken>())
            .Returns(leads);

        // Act
        var result = await _sut.GetAllLeadsAsync();

        // Assert
        result.Should().HaveCount(2);
        result[0].FullName.Should().Be("Lead 1");
        result[1].FullName.Should().Be("Lead 2");
        await _leadRepository.Received(1).GetAllOrderedByDateAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAllLeadsAsync_ReturnsEmptyList_WhenNoLeads()
    {
        // Arrange
        _leadRepository.GetAllOrderedByDateAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Lead>());

        // Act
        var result = await _sut.GetAllLeadsAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region MarkAsReadAsync

    [Fact]
    public async Task MarkAsReadAsync_MarksLeadAsRead_WithTimestamps()
    {
        // Arrange
        var id = Guid.NewGuid();
        var lead = new Lead
        {
            Id = id,
            FullName = "John Doe",
            Email = "john@example.com",
            Message = "Hello",
            IsRead = false,
            ReadAt = null,
            CreatedAt = DateTime.UtcNow.AddHours(-2)
        };

        _leadRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(lead);

        // Act
        var result = await _sut.MarkAsReadAsync(id);

        // Assert
        result.IsRead.Should().BeTrue();
        result.ReadAt.Should().NotBeNull();
        result.ReadAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        lead.IsRead.Should().BeTrue();
        lead.ReadAt.Should().NotBeNull();
        lead.UpdatedAt.Should().NotBeNull();
        lead.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        await _leadRepository.Received(1).UpdateAsync(lead, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task MarkAsReadAsync_ThrowsKeyNotFoundException_WhenLeadDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        _leadRepository.GetByIdAsync(id, Arg.Any<CancellationToken>())
            .Returns((Lead?)null);

        // Act
        var act = () => _sut.MarkAsReadAsync(id);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{id}*");
    }

    [Fact]
    public async Task MarkAsReadAsync_SetsUpdatedAt()
    {
        // Arrange
        var id = Guid.NewGuid();
        var lead = new Lead
        {
            Id = id,
            FullName = "John Doe",
            Email = "john@example.com",
            Message = "Hello",
            IsRead = false,
            CreatedAt = DateTime.UtcNow.AddHours(-1)
        };

        _leadRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(lead);

        // Act
        await _sut.MarkAsReadAsync(id);

        // Assert
        lead.UpdatedAt.Should().NotBeNull();
        lead.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task MarkAsReadAsync_MapsAllFieldsCorrectly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var lead = new Lead
        {
            Id = id,
            FullName = "Jane Smith",
            Email = "jane@example.com",
            Phone = "+9876543210",
            Company = "Tech Corp",
            Message = "Interested in services",
            IsRead = false,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        _leadRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns(lead);

        // Act
        var result = await _sut.MarkAsReadAsync(id);

        // Assert
        result.Id.Should().Be(id);
        result.FullName.Should().Be("Jane Smith");
        result.Email.Should().Be("jane@example.com");
        result.Phone.Should().Be("+9876543210");
        result.Company.Should().Be("Tech Corp");
        result.Message.Should().Be("Interested in services");
        result.IsRead.Should().BeTrue();
    }

    #endregion
}
