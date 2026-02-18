using Ganss.Xss;
using Portfolio.Application.Interfaces;

namespace Portfolio.Infrastructure.Services;

public class HtmlSanitizerService : IHtmlSanitizerService
{
    private readonly HtmlSanitizer _sanitizer;

    public HtmlSanitizerService()
    {
        _sanitizer = new HtmlSanitizer();

        // Allow safe structural and formatting tags
        _sanitizer.AllowedTags.Clear();
        foreach (var tag in AllowedTags)
            _sanitizer.AllowedTags.Add(tag);

        // Allow safe attributes
        _sanitizer.AllowedAttributes.Clear();
        foreach (var attr in AllowedAttributes)
            _sanitizer.AllowedAttributes.Add(attr);

        // Allow safe CSS properties for basic styling
        _sanitizer.AllowedCssProperties.Clear();
        foreach (var prop in AllowedCssProperties)
            _sanitizer.AllowedCssProperties.Add(prop);

        // Allow safe URI schemes
        _sanitizer.AllowedSchemes.Clear();
        _sanitizer.AllowedSchemes.Add("http");
        _sanitizer.AllowedSchemes.Add("https");
        _sanitizer.AllowedSchemes.Add("mailto");
    }

    public string Sanitize(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return html;

        return _sanitizer.Sanitize(html);
    }

    private static readonly string[] AllowedTags =
    [
        // Headings
        "h1", "h2", "h3", "h4", "h5", "h6",
        // Block elements
        "p", "div", "blockquote", "pre", "hr", "br",
        // Lists
        "ul", "ol", "li",
        // Inline formatting
        "strong", "b", "em", "i", "u", "s", "mark", "small", "sub", "sup",
        // Code
        "code", "kbd", "samp",
        // Links and images
        "a", "img",
        // Tables
        "table", "thead", "tbody", "tfoot", "tr", "th", "td", "caption",
        // Definition lists
        "dl", "dt", "dd",
        // Other
        "figure", "figcaption", "details", "summary", "span"
    ];

    private static readonly string[] AllowedAttributes =
    [
        "href", "src", "alt", "title", "width", "height",
        "class", "id", "style",
        "target", "rel",
        "colspan", "rowspan", "scope",
        "start", "type"
    ];

    private static readonly string[] AllowedCssProperties =
    [
        "color", "background-color", "font-size", "font-weight", "font-style",
        "text-align", "text-decoration", "margin", "padding",
        "border", "border-radius", "width", "height", "max-width"
    ];
}
