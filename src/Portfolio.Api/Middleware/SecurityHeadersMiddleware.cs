namespace Portfolio.Api.Middleware;

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;

        // Prevent MIME-type sniffing - Critical for file uploads!
        headers["X-Content-Type-Options"] = "nosniff";

        // Prevent clickjacking
        headers["X-Frame-Options"] = "DENY";

        // Control referrer information
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

        // Restrict permissions/features the browser can use
        headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";

        // Prevent XSS in older browsers (modern browsers use CSP)
        headers["X-XSS-Protection"] = "1; mode=block";

        // Only serve over HTTPS
        headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";

        // Content Security Policy - Prevent execution of uploaded files
        // For static file requests (uploaded content), apply strict CSP
        if (context.Request.Path.StartsWithSegments("/uploads"))
        {
            headers["Content-Security-Policy"] =
                "default-src 'none'; " +
                "style-src 'none'; " +
                "script-src 'none'; " +
                "object-src 'none'; " +
                "base-uri 'none'; " +
                "form-action 'none'; " +
                "frame-ancestors 'none'";

            // Force download for potentially dangerous files
            headers["Content-Disposition"] = "inline";
            headers["X-Download-Options"] = "noopen";
        }

        await _next(context);
    }
}
