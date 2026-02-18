# Testing

## Test Structure

The solution has three test projects matching the architecture layers:

```
tests/
├── Portfolio.Api.Tests/              Integration tests (HTTP-level)
│   ├── AuthControllerTests.cs
│   ├── HealthCheckTests.cs
│   ├── PublicEndpointTests.cs
│   └── CustomWebApplicationFactory.cs
├── Portfolio.Application.Tests/      Unit tests (business logic)
│   └── Services/
│       ├── AiContentServiceTests.cs
│       ├── BlogPostServiceTests.cs
│       ├── LeadServiceTests.cs
│       └── ProjectServiceTests.cs
└── Portfolio.Infrastructure.Tests/   Unit tests (infrastructure)
    ├── Caching/
    │   └── HybridCacheServiceTests.cs
    ├── Identity/
    │   └── CaptchaServiceTests.cs
    └── Services/
        └── HtmlSanitizerServiceTests.cs
```

## Frameworks & Libraries

| Package | Purpose |
|---------|---------|
| [xUnit](https://xunit.net/) | Test framework |
| [NSubstitute](https://nsubstitute.github.io/) | Mocking library |
| [FluentAssertions](https://fluentassertions.com/) | Readable assertions |
| [Microsoft.AspNetCore.Mvc.Testing](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests) | Integration test host |
| [Microsoft.EntityFrameworkCore.Sqlite](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.Sqlite) | In-memory SQLite for integration tests |
| [Microsoft.EntityFrameworkCore.InMemory](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.InMemory) | In-memory EF provider for unit tests |

## Running Tests

```bash
# Run all tests
dotnet test Portfolio.sln

# Run a specific project
dotnet test tests/Portfolio.Application.Tests

# Run with verbose output
dotnet test Portfolio.sln --verbosity normal

# Run a specific test class
dotnet test --filter "FullyQualifiedName~AiContentServiceTests"

# Run a specific test
dotnet test --filter "FullyQualifiedName~GenerateTextAsync_SavesRecordAndReturnsCompleted"
```

## Test Categories

### Unit Tests (Application + Infrastructure)

Unit tests mock all dependencies using NSubstitute. They test business logic in isolation.

**Example pattern:**

```csharp
public class BlogPostServiceTests
{
    private readonly IBlogPostRepository _repository;
    private readonly BlogPostService _sut;

    public BlogPostServiceTests()
    {
        _repository = Substitute.For<IBlogPostRepository>();
        // ... setup other mocks
        _sut = new BlogPostService(_repository, ...);
    }

    [Fact]
    public async Task GetBySlugAsync_ReturnsDto_WhenFound()
    {
        // Arrange
        _repository.GetBySlugAsync("my-post").Returns(new BlogPost { ... });

        // Act
        var result = await _sut.GetBySlugAsync("my-post");

        // Assert
        result.Should().NotBeNull();
        result!.Slug.Should().Be("my-post");
    }
}
```

### Integration Tests (API)

Integration tests use `WebApplicationFactory<Program>` to spin up the full HTTP pipeline, including middleware, authentication, and database access.

**Key infrastructure**: `CustomWebApplicationFactory` replaces production dependencies with test doubles:

| Production | Test Replacement | Reason |
|-----------|-----------------|--------|
| SQL Server | SQLite (in-memory) | Fast, no Docker needed |
| Redis (distributed cache) | In-memory distributed cache | No external dependency |
| Redis (output cache) | In-memory output cache | No external dependency |
| EF Migrations | `Database.EnsureCreated()` | Faster than running migrations |

**Why SQLite for integration tests?**

SQLite's in-memory mode (`DataSource=:memory:`) provides a real relational database that supports SQL queries, constraints, and relationships — but without needing a running SQL Server instance. This is the standard approach recommended by Microsoft for ASP.NET Core integration tests.

The `CustomWebApplicationFactory` keeps a persistent `SqliteConnection` open for the factory's lifetime, ensuring the in-memory database persists across requests within a test.

**Authenticated test requests:**

```csharp
[Fact]
public async Task AdminEndpoint_RequiresAuthentication()
{
    var client = _factory.CreateClient();
    var response = await client.GetAsync("/api/v1/admin/blogs");
    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
}

[Fact]
public async Task AdminEndpoint_ReturnsOk_WhenAuthenticated()
{
    var client = await _factory.CreateAuthenticatedClientAsync();
    var response = await client.GetAsync("/api/v1/admin/blogs");
    response.StatusCode.Should().Be(HttpStatusCode.OK);
}
```

The factory provides `CreateAuthenticatedClientAsync()` which logs in as the seeded admin user and sets the `Authorization: Bearer` header automatically.

## Writing New Tests

### Unit Test Checklist

1. Create test class in the matching test project (e.g., `Application.Tests/Services/`)
2. Mock dependencies with `Substitute.For<T>()`
3. Follow Arrange-Act-Assert pattern
4. Test both success and failure paths
5. Verify repository/service interactions with `Received()`

### Integration Test Checklist

1. Create test class in `Portfolio.Api.Tests`
2. Implement `IClassFixture<CustomWebApplicationFactory>`
3. Use `_factory.CreateClient()` for anonymous requests
4. Use `_factory.CreateAuthenticatedClientAsync()` for admin requests
5. Test HTTP status codes, response bodies, and headers

## Test Conventions

- **File naming**: `{ClassUnderTest}Tests.cs`
- **Method naming**: `{Method}_{Scenario}_{ExpectedResult}` (e.g., `GetBySlugAsync_ReturnsNull_WhenNotFound`)
- **One assertion concept per test** (multiple assertions are fine if they verify the same concept)
- **No test interdependence** — each test runs independently
- Use `#region` blocks to group related tests by method
