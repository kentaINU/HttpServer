using HttpServer.Core;
using Xunit;

namespace HttpServer.Tests;

public class ParserTests
{
    [Fact]
    public void Should_Parse_Get_Request()
    {
        var raw = "GET /test.html HTTP/1.1\r\nHost: localhost\r\n\r\n";
        var request = HttpRequest.Parse(raw);

        Assert.Equal("GET", request.Method);
        Assert.Equal("/test.html", request.Path);
    }
}