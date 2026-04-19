namespace HttpServer.Core;

public class HttpRequest
{
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public Dictionary<string, string> Headers { get; set; } = new();
    // Bodyプロパティを追加
    public string Body { get; set; } = string.Empty;

    public static HttpRequest Parse(string rawRequest)
    {
        var request = new HttpRequest();
        
        // ヘッダーとボディの境界線（空行 \r\n\r\n）を探す
        var separator = "\r\n\r\n";
        var separatorIndex = rawRequest.IndexOf(separator);

        string headerPart;
        if (separatorIndex != -1)
        {
            // ヘッダー部分とボディ部分に分ける
            headerPart = rawRequest.Substring(0, separatorIndex);
            request.Body = rawRequest.Substring(separatorIndex + separator.Length);
        }
        else
        {
            // 空行がない場合はすべてヘッダーとして扱う（ボディなし）
            headerPart = rawRequest;
        }

        var lines = headerPart.Split("\r\n");
        if (lines.Length == 0 || string.IsNullOrWhiteSpace(lines[0])) return request;

        // 1. リクエストラインのパース (GET /path HTTP/1.1)
        var firstLineParts = lines[0].Split(' ');
        if (firstLineParts.Length >= 2)
        {
            request.Method = firstLineParts[0];
            request.Path = firstLineParts[1];
        }

        // 2. ヘッダーのパース
        foreach (var line in lines.Skip(1))
        {
            if (string.IsNullOrWhiteSpace(line)) continue; 
            
            var parts = line.Split(": ", 2);
            if (parts.Length == 2)
            {
                request.Headers[parts[0]] = parts[1];
            }
        }
        
        return request;
    }
}