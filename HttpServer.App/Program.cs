using System.Net;
using System.Net.Sockets;
using System.Text;
using HttpServer.Core; 

var listener = new TcpListener(IPAddress.Any, 8080);
listener.Start();
Console.WriteLine("Server started on http://localhost:8080");

while (true)
{
    using var client = await listener.AcceptTcpClientAsync();
    using var stream = client.GetStream();

    var buffer = new byte[2048];
    var bytesRead = await stream.ReadAsync(buffer);
    var rawRequest = Encoding.UTF8.GetString(buffer, 0, bytesRead);

    // Coreプロジェクトのクラスで解析
    var request = HttpRequest.Parse(rawRequest);
    
    // ヘッダーからUser-Agentを取得（Core側の更新が必要です）
    var userAgent = request.Headers.GetValueOrDefault("User-Agent", "Unknown Browser");
    
    Console.WriteLine($"[{DateTime.Now:T}] {request.Method} {request.Path}");

    // レスポンスボディの作成
    var responseBody = $"""
        <html>
            <head><meta charset="UTF-8"></head>
            <body>
                <h1>自作HTTPサーバー稼働中</h1>
                <p><b>アクセスパス:</b> {request.Path}</p>
                <p><b>あなたのブラウザ:</b> {userAgent}</p>
                <hr>
                <p>Processed by HttpServer.Core</p>
            </body>
        </html>
        """;

    // HTTPレスポンス全体の組み立て
    var responseHeader = 
        "HTTP/1.1 200 OK\r\n" +
        "Content-Type: text/html; charset=UTF-8\r\n" +
        $"Content-Length: {Encoding.UTF8.GetByteCount(responseBody)}\r\n" +
        "Connection: close\r\n" +
        "\r\n";

    var fullResponse = responseHeader + responseBody;

    await stream.WriteAsync(Encoding.UTF8.GetBytes(fullResponse));
}