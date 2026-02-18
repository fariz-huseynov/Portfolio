namespace Portfolio.Application.Common;

public static class FileValidation
{
    // Magic bytes for file type validation
    private static readonly Dictionary<string, byte[]> FileSignatures = new()
    {
        { ".jpg", new byte[] { 0xFF, 0xD8, 0xFF } },
        { ".jpeg", new byte[] { 0xFF, 0xD8, 0xFF } },
        { ".png", new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } },
        { ".gif", new byte[] { 0x47, 0x49, 0x46, 0x38 } },
        { ".pdf", new byte[] { 0x25, 0x50, 0x44, 0x46 } }
    };

    // Dangerous file extensions that should never be allowed
    private static readonly HashSet<string> DangerousExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".exe", ".dll", ".bat", ".cmd", ".com", ".pif", ".scr", ".vbs", ".js", ".jar",
        ".msi", ".app", ".deb", ".rpm", ".dmg", ".pkg", ".sh", ".bash", ".ps1", ".psm1",
        ".reg", ".vb", ".ws", ".wsf", ".scf", ".lnk", ".inf", ".cpl", ".ex", ".ex_",
        ".asp", ".aspx", ".php", ".jsp", ".jspx", ".do", ".action", ".py", ".rb", ".pl",
        ".cgi", ".htaccess", ".htpasswd", ".ini", ".config", ".cer", ".csr", ".pem",
        ".der", ".p7b", ".p7c", ".p12", ".pfx", ".svg" // SVG can contain XSS
    };

    // Suspicious patterns in file content (for images and PDFs)
    private static readonly byte[][] SuspiciousPatterns = new[]
    {
        // Script tags in various encodings
        System.Text.Encoding.ASCII.GetBytes("<script"),
        System.Text.Encoding.ASCII.GetBytes("javascript:"),
        System.Text.Encoding.ASCII.GetBytes("vbscript:"),
        System.Text.Encoding.ASCII.GetBytes("onload="),
        System.Text.Encoding.ASCII.GetBytes("onerror="),

        // Executable signatures
        new byte[] { 0x4D, 0x5A }, // MZ - Windows executable
        new byte[] { 0x7F, 0x45, 0x4C, 0x46 }, // ELF - Linux executable

        // Archive formats that could contain malware
        new byte[] { 0x50, 0x4B, 0x03, 0x04 }, // ZIP
        new byte[] { 0x52, 0x61, 0x72, 0x21 }, // RAR
    };

    public static bool IsValidFileSignature(Stream fileStream, string extension)
    {
        extension = extension.ToLowerInvariant();
        if (!FileSignatures.TryGetValue(extension, out var signature))
            return false;

        var headerBytes = new byte[signature.Length];
        fileStream.Position = 0;
        var bytesRead = fileStream.Read(headerBytes, 0, signature.Length);
        fileStream.Position = 0;

        if (bytesRead < signature.Length)
            return false;

        return headerBytes.Take(signature.Length).SequenceEqual(signature);
    }

    public static bool IsAllowedExtension(string extension, string[] allowedExtensions)
    {
        extension = extension.ToLowerInvariant();

        // Check if it's explicitly allowed
        if (!allowedExtensions.Contains(extension))
            return false;

        // Double-check it's not in dangerous list (defense in depth)
        if (DangerousExtensions.Contains(extension))
            return false;

        return true;
    }

    public static bool ContainsSuspiciousContent(Stream fileStream)
    {
        fileStream.Position = 0;
        const int bufferSize = 8192; // Read first 8KB for scanning
        var buffer = new byte[bufferSize];
        var bytesRead = fileStream.Read(buffer, 0, bufferSize);
        fileStream.Position = 0;

        if (bytesRead == 0)
            return false;

        // Check for suspicious patterns
        foreach (var pattern in SuspiciousPatterns)
        {
            if (ContainsPattern(buffer, bytesRead, pattern))
                return true;
        }

        return false;
    }

    public static bool HasDoubleExtension(string filename)
    {
        // Check for double extensions like "image.jpg.exe"
        var parts = filename.Split('.');
        if (parts.Length <= 2)
            return false; // Normal file with single extension

        // Check if any part before the last one looks like an extension
        for (int i = 1; i < parts.Length - 1; i++)
        {
            var possibleExt = "." + parts[i].ToLowerInvariant();
            if (DangerousExtensions.Contains(possibleExt) ||
                FileSignatures.ContainsKey(possibleExt))
            {
                return true;
            }
        }

        return false;
    }

    public static string SanitizeFilename(string filename)
    {
        // Remove any path traversal attempts
        filename = Path.GetFileName(filename);

        // Remove null bytes
        filename = filename.Replace("\0", string.Empty);

        // Remove potentially dangerous characters
        var invalidChars = Path.GetInvalidFileNameChars();
        foreach (var c in invalidChars)
        {
            filename = filename.Replace(c.ToString(), string.Empty);
        }

        // Remove additional suspicious characters
        filename = filename.Replace("..", string.Empty);
        filename = filename.Replace(":", string.Empty);
        filename = filename.Replace(";", string.Empty);

        // Ensure filename is not empty after sanitization
        if (string.IsNullOrWhiteSpace(filename))
        {
            filename = "file";
        }

        return filename;
    }

    private static bool ContainsPattern(byte[] buffer, int length, byte[] pattern)
    {
        if (pattern.Length > length)
            return false;

        for (int i = 0; i <= length - pattern.Length; i++)
        {
            bool match = true;
            for (int j = 0; j < pattern.Length; j++)
            {
                if (buffer[i + j] != pattern[j])
                {
                    match = false;
                    break;
                }
            }
            if (match)
                return true;
        }

        return false;
    }
}
