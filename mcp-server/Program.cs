using System;
using System.Threading.Tasks;
using backend.api.auth.impl;
using backend.api.auth.intr;
using backend.api.oura.impl;
using backend.api.oura.intr;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Server;
using mcp_server.Tools;

namespace mcp_server;

class Program
{
    static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        
        // Register OAuth services
        builder.Services.AddSingleton<IOAuthConfigProvider, OAuthConfigProvider>();
        builder.Services.AddSingleton<ITokenStore, TokenStore>();
        builder.Services.AddSingleton<IOAuthService, OAuthService>();
        
        // Register Oura services
        builder.Services.AddSingleton<IOuraClient, OuraClient>();
        builder.Services.AddSingleton<IOuraService, OuraService>();
        
        // Register MCP tools
        builder.Services.AddSingleton<OuraTools>();
        
        // Configure MCP server
        builder.Services.AddMcpServer()
            .WithStdioServerTransport()
            .WithToolsFromAssembly();
        
        var host = builder.Build();
        await host.RunAsync();
    }
}
