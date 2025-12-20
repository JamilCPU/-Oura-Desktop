using System.Threading.Tasks;
using backend.api.auth.models;

namespace backend.api.auth.intr;

public interface ITokenStore
{
    Task<OAuthTokens?> LoadTokensAsync();
    Task SaveTokensAsync(OAuthTokens tokens);
    Task<bool> HasValidTokensAsync();
    Task ClearTokensAsync();
}
