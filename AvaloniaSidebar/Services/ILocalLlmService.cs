using System.Threading;
using System.Threading.Tasks;

namespace AvaloniaSidebar.Services;

public interface ILocalLlmService
{
    Task<string> GenerateResponseAsync(string prompt, CancellationToken cancellationToken = default);
    Task InitializeAsync();
    void Dispose();
}
