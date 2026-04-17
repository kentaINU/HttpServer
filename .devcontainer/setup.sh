#!/bin/bash
if [ ! -f "MyWebSystem.sln" ]; then
    dotnet new sln -n MyWebSystem
    dotnet new console -n HttpServer.App
    dotnet new classlib -n HttpServer.Core
    dotnet new xunit -n HttpServer.Tests

    dotnet sln add HttpServer.App/HttpServer.App.csproj
    dotnet sln add HttpServer.Core/HttpServer.Core.csproj
    dotnet sln add HttpServer.Tests/HttpServer.Tests.csproj

    dotnet add HttpServer.App/HttpServer.App.csproj reference HttpServer.Core/HttpServer.Core.csproj
    dotnet add HttpServer.Tests/HttpServer.Tests.csproj reference HttpServer.Core/HttpServer.Core.csproj
    
    dotnet restore
fi