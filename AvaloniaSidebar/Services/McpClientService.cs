using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using ModelContextProtocol;

namespace AvaloniaSidebar.Services;

public class McpClientService : IMcpClientService, IDisposable
{
    private McpClient? _client;
    private bool _initialized;

    public async Task InitializeAsync()
    {
        if (_initialized)
            return;

        var serverExecutablePath = GetServerExecutablePath();
        
        if (string.IsNullOrEmpty(serverExecutablePath) || !File.Exists(serverExecutablePath))
        {
            throw new InvalidOperationException(
                $"MCP server executable not found at {serverExecutablePath}. Please build the mcp-server project first.");
        }

        var clientTransport = new StdioClientTransport(new StdioClientTransportOptions
        {
            Name = "OuraMcpServer",
            Command = serverExecutablePath,
            Arguments = Array.Empty<string>()
        });

        _client = await McpClient.CreateAsync(clientTransport);
        _initialized = true;
    }

    public async Task<IReadOnlyList<Tool>> ListToolsAsync(CancellationToken cancellationToken = default)
    {
        if (!_initialized || _client == null)
        {
            throw new InvalidOperationException("MCP client not initialized. Call InitializeAsync first.");
        }

        // ListToolsAsync returns IList<McpClientTool>, need to convert to IReadOnlyList<Tool>
        var clientTools = await _client.ListToolsAsync((ModelContextProtocol.RequestOptions?)null, cancellationToken);
        
        // Convert McpClientTool to Tool - map available properties
        var tools = clientTools.Select(ct => new Tool
        {
            Name = ct.Name,
            Description = ct.Description
        }).ToList().AsReadOnly();
        
        return tools;
    }

    public async Task<CallToolResult> CallToolAsync(string name, Dictionary<string, object?> arguments, CancellationToken cancellationToken = default)
    {
        if (!_initialized || _client == null)
        {
            throw new InvalidOperationException("MCP client not initialized. Call InitializeAsync first.");
        }

        // CallToolAsync signature: (string, Dictionary<string, object?>, RequestOptions?, IProgress<ProgressNotificationValue>?)
        // Passing null for both optional parameters
        return await _client.CallToolAsync(name, arguments, null, null);
    }

    private string GetServerExecutablePath()
    {
        var solutionRoot = GetSolutionRoot();
        var configuration = "Debug";
        
        #if RELEASE
        configuration = "Release";
        #endif

        var executableName = "mcp-server.exe";
        if (Environment.OSVersion.Platform == PlatformID.Unix || 
            Environment.OSVersion.Platform == PlatformID.MacOSX)
        {
            executableName = "mcp-server";
        }

        return Path.Combine(solutionRoot, "mcp-server", "bin", configuration, "net9.0", executableName);
    }

    private static string GetSolutionRoot()
    {
        var currentDir = Directory.GetCurrentDirectory();
        var directory = new DirectoryInfo(currentDir);
        
        while (directory != null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "-Oura-Desktop.sln")))
            {
                return directory.FullName;
            }
            directory = directory.Parent;
        }
        
        return currentDir;
    }

    public void Dispose()
    {
        // McpClient doesn't implement IDisposable, so we just mark as uninitialized
        // The client will be cleaned up when it goes out of scope
        _client = null;
        _initialized = false;
    }
}



