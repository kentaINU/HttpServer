using System.Net;
using System.Net.Sockets;
using System.Text;
using HttpServer.Core;

const int Port = 8080;
var listener = new TcpListener(IPAddress.Any, Port);
listener.Start();

Console.WriteLine($"Server started on http://localhost:{Port}");

while (true)
{
    using var client = await listener.AcceptTcpClientAsync();
    using var stream = client.GetStream();

    var buffer = new byte[2048];
    var bytesRead = await stream.ReadAsync(buffer);
    if (bytesRead == 0) continue;

    var request = HttpRequest.Parse(Encoding.UTF8.GetString(buffer, 0, bytesRead));
    Console.WriteLine($"[{DateTime.Now:T}] {request.Method} {request.Path}");

    var rootPath = Path.GetFullPath("wwwroot");
    if (!TryResolvePath(rootPath, request.Path, out var fullPath))
    {
        Console.WriteLine($"[SECURITY ALERT] Blocked: {request.Path}");
        await SendResponseAsync(stream, "403 Forbidden", "text/html", "<h1>403 Forbidden</h1>");
        continue;
    }

    // --- メインロジック（判定と送信） ---

    // 1. POST送信（掲示板の投稿）
    if (request.Method == "POST" && request.Path == "/submit")
    {
        var decodedBody = WebUtility.UrlDecode(request.Body);
        var data = decodedBody.Split('&')
                    .Select(p => p.Split('='))
                    .ToDictionary(a => a[0], a => a.Length > 1 ? a[1] : "");

        var name = data.GetValueOrDefault("username", "匿名");
        var message = data.GetValueOrDefault("message", "本文なし");

        // ファイルに保存（永続化）
        var logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss},{name},{message}{Environment.NewLine}";
        await File.AppendAllTextAsync("messages.txt", logEntry);

        // 送信後はトップ(GET /)へリダイレクト
        var redirectHeader = "HTTP/1.1 303 See Other\r\nLocation: /\r\nConnection: close\r\n\r\n";
        await stream.WriteAsync(Encoding.UTF8.GetBytes(redirectHeader));
    }
    // 2. ルートアクセス（掲示板を表示）
    else if (request.Path == "/")
    {
        var html = await GenerateHomeHtml();
        await SendResponseAsync(stream, "200 OK", "text/html", html);
    }
    // 3. ファイルが存在する場合（画像やCSSなど）
    else if (File.Exists(fullPath))
    {
        await SendFileAsync(stream, fullPath);
    }
    // 4. ディレクトリの場合
    else if (Directory.Exists(fullPath))
    {
        var indexPath = Path.Combine(fullPath, "index.html");
        if (File.Exists(indexPath))
        {
            await SendFileAsync(stream, indexPath);
        }
        else
        {
            var html = GenerateDirectoryListing(request.Path, fullPath);
            await SendResponseAsync(stream, "200 OK", "text/html", html);
        }
    }
    // 5. どこにもヒットしない場合
    else
    {
        await SendResponseAsync(stream, "404 Not Found", "text/html", "<h1>404 Not Found</h1>");
    }
}

// --- ヘルパーメソッド群 ---

bool TryResolvePath(string root, string requestPath, out string fullPath)
{
    fullPath = string.Empty;
    if (requestPath.Contains("..")) return false;
    var resolved = Path.GetFullPath(Path.Combine(root, requestPath.TrimStart('/')));
    if (!resolved.StartsWith(root)) return false;
    fullPath = resolved;
    return true;
}

async Task SendResponseAsync(NetworkStream stream, string status, string contentType, string body)
{
    var contentBytes = Encoding.UTF8.GetBytes(body);
    var header = $"HTTP/1.1 {status}\r\nContent-Type: {contentType}; charset=UTF-8\r\nContent-Length: {contentBytes.Length}\r\nConnection: close\r\n\r\n";
    await stream.WriteAsync(Encoding.UTF8.GetBytes(header));
    await stream.WriteAsync(contentBytes);
}

async Task SendFileAsync(NetworkStream stream, string path)
{
    var content = await File.ReadAllBytesAsync(path);
    var mime = MimeMapping.GetMimeType(path);
    var header = $"HTTP/1.1 200 OK\r\nContent-Type: {mime}\r\nContent-Length: {content.Length}\r\nConnection: close\r\n\r\n";
    await stream.WriteAsync(Encoding.UTF8.GetBytes(header));
    await stream.WriteAsync(content);
}

string GenerateDirectoryListing(string requestPath, string physicalPath)
{
    var dir = new DirectoryInfo(physicalPath);
    var sb = new StringBuilder();
    sb.Append($"<html><body><h1>Index of {requestPath}</h1><hr><ul>");
    if (requestPath != "/") sb.Append("<li><a href='..'>.. (Back)</a></li>");
    foreach (var item in dir.GetFileSystemInfos())
    {
        var suffix = item is DirectoryInfo ? "/" : "";
        sb.Append($"<li><a href='{item.Name}{suffix}'>{item.Name}{suffix}</a></li>");
    }
    sb.Append("</ul><hr><i>HttpServer.Core</i></body></html>");
    return sb.ToString();
}

async Task<string> GenerateHomeHtml()
{
    var sb = new StringBuilder();
    sb.Append("<html><head><meta charset='UTF-8'><title>C# 自作掲示板</title></head><body style='font-family: sans-serif; max-width: 600px; margin: auto;'>");
    sb.Append("<h1>📝 簡易掲示板</h1>");
    sb.Append("""
        <form action="/submit" method="POST" style="background: #f9f9f9; padding: 20px; border-radius: 8px;">
            <div>名前: <br><input type="text" name="username" style="width: 100%;"></div><br>
            <div>メッセージ: <br><textarea name="message" style="width: 100%; height: 80px;"></textarea></div><br>
            <button type="submit" style="padding: 10px 20px;">投稿する</button>
        </form>
        <hr>
        """);

    if (File.Exists("messages.txt"))
    {
        var lines = await File.ReadAllLinesAsync("messages.txt");
        foreach (var line in lines.Reverse())
        {
            var parts = line.Split(',', 3);
            if (parts.Length >= 3)
            {
                sb.Append($"<div style='border-bottom: 1px solid #eee; padding: 10px;'>");
                sb.Append($"<strong>{WebUtility.HtmlEncode(parts[1])}</strong> <small style='color: #666;'>{parts[0]}</small><br>");
                sb.Append($"<p>{WebUtility.HtmlEncode(parts[2])}</p>");
                sb.Append("</div>");
            }
        }
    }
    sb.Append("</body></html>");
    return sb.ToString();
}