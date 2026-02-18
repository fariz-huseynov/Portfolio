namespace Portfolio.Application.Interfaces;

public interface IHtmlSanitizerService
{
    string Sanitize(string html);
}
