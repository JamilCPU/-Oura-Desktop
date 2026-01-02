using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol.Protocol;

namespace AvaloniaSidebar.Services;

public interface IMcpClientService
{
    Task InitializeAsync();
    Task<IReadOnlyList<Tool>> ListToolsAsync(CancellationToken cancellationToken = default);
    Task<CallToolResult> CallToolAsync(string name, Dictionary<string, object?> arguments, CancellationToken cancellationToken = default);
    void Dispose();
}
