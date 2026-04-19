namespace HttpServer.Core;

public static class MimeMapping
{
    private static readonly Dictionary<string, string> _mimeTypes = new()
    {
        { ".html", "text/html; charset=UTF-8" },
        { ".css", "text/css" },
        { ".js", "application/javascript" },
        { ".png", "image/png" },
        { ".jpg", "image/jpeg" },
        { ".gif", "image/gif" }
    };

    public static string GetMimeType(string path)
    {
        var extension = Path.GetExtension(path).ToLower();
        return _mimeTypes.GetValueOrDefault(extension, "application/octet-stream");
    }
}