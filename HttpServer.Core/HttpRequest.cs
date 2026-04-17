namespace HttpServer.Core;

public class HttpRequest
{
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public Dictionary<string, string> Headers { get; set; } = new();

    public static HttpRequest Parse(string rawRequest)
    {
        var request = new HttpRequest();
        var lines = rawRequest.Split("\r\n");
        if (lines.Length == 0 || string.IsNullOrWhiteSpace(lines[0])) return request;

        // 1行目のパース (GET /path HTTP/1.1)
        var firstLineParts = lines[0].Split(' ');
        if (firstLineParts.Length >= 2)
        {
            request.Method = firstLineParts[0];
            request.Path = firstLineParts[1];
        }

        // 2行目以降（ヘッダー）のパース
        foreach (var line in lines.Skip(1))
        {
            if (string.IsNullOrWhiteSpace(line)) break; 
            
            var parts = line.Split(": ", 2);
            if (parts.Length == 2)
            {
                request.Headers[parts[0]] = parts[1];
            }
        }
        return request;
    }
}