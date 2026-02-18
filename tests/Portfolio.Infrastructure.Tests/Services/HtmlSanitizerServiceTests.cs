using FluentAssertions;
using Portfolio.Infrastructure.Services;
using Xunit;

namespace Portfolio.Infrastructure.Tests.Services;

public class HtmlSanitizerServiceTests
{
    private readonly HtmlSanitizerService _sut;

    public HtmlSanitizerServiceTests()
    {
        _sut = new HtmlSanitizerService();
    }

    #region Null and empty input

    [Fact]
    public void Sanitize_ReturnsNull_WhenInputIsNull()
    {
        // Act
        var result = _sut.Sanitize(null!);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Sanitize_ReturnsEmpty_WhenInputIsEmpty()
    {
        // Act
        var result = _sut.Sanitize(string.Empty);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Sanitize_ReturnsWhitespace_WhenInputIsWhitespace()
    {
        // Act
        var result = _sut.Sanitize("   ");

        // Assert
        result.Should().Be("   ");
    }

    #endregion

    #region Stripping dangerous content

    [Fact]
    public void Sanitize_StripsScriptTags()
    {
        // Arrange
        var html = "<p>Hello</p><script>alert('xss')</script><p>World</p>";

        // Act
        var result = _sut.Sanitize(html);

        // Assert
        result.Should().NotContain("<script");
        result.Should().NotContain("alert");
        result.Should().Contain("<p>Hello</p>");
        result.Should().Contain("<p>World</p>");
    }

    [Fact]
    public void Sanitize_StripsInlineScriptTags()
    {
        // Arrange
        var html = "<script type=\"text/javascript\">document.cookie</script>";

        // Act
        var result = _sut.Sanitize(html);

        // Assert
        result.Should().NotContain("<script");
        result.Should().NotContain("document.cookie");
    }

    [Fact]
    public void Sanitize_StripsOnClickHandler()
    {
        // Arrange
        var html = "<button onclick=\"alert('xss')\">Click me</button>";

        // Act
        var result = _sut.Sanitize(html);

        // Assert
        result.Should().NotContain("onclick");
        result.Should().NotContain("alert");
    }

    [Fact]
    public void Sanitize_StripsOnErrorHandler()
    {
        // Arrange
        var html = "<img src=\"invalid.jpg\" onerror=\"alert('xss')\" alt=\"test\" />";

        // Act
        var result = _sut.Sanitize(html);

        // Assert
        result.Should().NotContain("onerror");
        result.Should().NotContain("alert");
    }

    [Fact]
    public void Sanitize_StripsOnMouseOverHandler()
    {
        // Arrange
        var html = "<div onmouseover=\"steal()\">Hover me</div>";

        // Act
        var result = _sut.Sanitize(html);

        // Assert
        result.Should().NotContain("onmouseover");
        result.Should().NotContain("steal");
    }

    [Fact]
    public void Sanitize_StripsIframeTags()
    {
        // Arrange
        var html = "<p>Content</p><iframe src=\"https://evil.com\"></iframe>";

        // Act
        var result = _sut.Sanitize(html);

        // Assert
        result.Should().NotContain("<iframe");
        result.Should().Contain("<p>Content</p>");
    }

    [Fact]
    public void Sanitize_StripsObjectTags()
    {
        // Arrange
        var html = "<object data=\"malware.swf\"></object><p>Safe</p>";

        // Act
        var result = _sut.Sanitize(html);

        // Assert
        result.Should().NotContain("<object");
        result.Should().Contain("<p>Safe</p>");
    }

    #endregion

    #region Preserving safe HTML

    [Fact]
    public void Sanitize_PreservesParagraphs()
    {
        // Arrange
        var html = "<p>Hello World</p>";

        // Act
        var result = _sut.Sanitize(html);

        // Assert
        result.Should().Be("<p>Hello World</p>");
    }

    [Fact]
    public void Sanitize_PreservesHeadings()
    {
        // Arrange
        var html = "<h1>Title</h1><h2>Subtitle</h2><h3>Section</h3>";

        // Act
        var result = _sut.Sanitize(html);

        // Assert
        result.Should().Contain("<h1>Title</h1>");
        result.Should().Contain("<h2>Subtitle</h2>");
        result.Should().Contain("<h3>Section</h3>");
    }

    [Fact]
    public void Sanitize_PreservesLinks()
    {
        // Arrange
        var html = "<a href=\"https://example.com\" target=\"_blank\" rel=\"noopener\">Visit</a>";

        // Act
        var result = _sut.Sanitize(html);

        // Assert
        result.Should().Contain("<a");
        result.Should().Contain("href=\"https://example.com\"");
        result.Should().Contain("Visit</a>");
    }

    [Fact]
    public void Sanitize_PreservesImages()
    {
        // Arrange
        var html = "<img src=\"https://example.com/photo.jpg\" alt=\"A photo\" />";

        // Act
        var result = _sut.Sanitize(html);

        // Assert
        result.Should().Contain("<img");
        result.Should().Contain("src=\"https://example.com/photo.jpg\"");
        result.Should().Contain("alt=\"A photo\"");
    }

    [Fact]
    public void Sanitize_PreservesLists()
    {
        // Arrange
        var html = "<ul><li>Item 1</li><li>Item 2</li></ul>";

        // Act
        var result = _sut.Sanitize(html);

        // Assert
        result.Should().Contain("<ul>");
        result.Should().Contain("<li>Item 1</li>");
        result.Should().Contain("<li>Item 2</li>");
    }

    [Fact]
    public void Sanitize_PreservesFormattingTags()
    {
        // Arrange
        var html = "<p><strong>Bold</strong> and <em>italic</em> and <u>underline</u></p>";

        // Act
        var result = _sut.Sanitize(html);

        // Assert
        result.Should().Contain("<strong>Bold</strong>");
        result.Should().Contain("<em>italic</em>");
        result.Should().Contain("<u>underline</u>");
    }

    [Fact]
    public void Sanitize_PreservesTableStructure()
    {
        // Arrange
        var html = "<table><thead><tr><th>Header</th></tr></thead><tbody><tr><td>Data</td></tr></tbody></table>";

        // Act
        var result = _sut.Sanitize(html);

        // Assert
        result.Should().Contain("<table>");
        result.Should().Contain("<th>Header</th>");
        result.Should().Contain("<td>Data</td>");
    }

    [Fact]
    public void Sanitize_PreservesCodeBlocks()
    {
        // Arrange
        var html = "<pre><code>var x = 1;</code></pre>";

        // Act
        var result = _sut.Sanitize(html);

        // Assert
        result.Should().Contain("<pre>");
        result.Should().Contain("<code>");
        result.Should().Contain("var x = 1;");
    }

    [Fact]
    public void Sanitize_PreservesClassAndStyleAttributes()
    {
        // Arrange
        var html = "<p class=\"highlight\" style=\"color: red;\">Styled text</p>";

        // Act
        var result = _sut.Sanitize(html);

        // Assert
        result.Should().Contain("class=\"highlight\"");
        // HtmlSanitizer normalizes CSS color values (e.g. "red" -> "rgba(255, 0, 0, 1)")
        result.Should().Contain("style=\"color:");
    }

    #endregion

    #region Mixed safe and dangerous content

    [Fact]
    public void Sanitize_RemovesDangerousButPreservesSafe_InMixedContent()
    {
        // Arrange
        var html = "<h1>Title</h1><script>evil()</script><p onclick=\"hack()\">Text</p><img src=\"photo.jpg\" onerror=\"steal()\" alt=\"pic\" />";

        // Act
        var result = _sut.Sanitize(html);

        // Assert
        result.Should().Contain("<h1>Title</h1>");
        result.Should().NotContain("<script");
        result.Should().NotContain("onclick");
        result.Should().NotContain("onerror");
        result.Should().Contain("<p>");
        result.Should().Contain("Text");
        result.Should().Contain("<img");
        result.Should().Contain("alt=\"pic\"");
    }

    #endregion
}
