using System.Threading.Tasks;

namespace backend.api.auth.intr;

public interface IOAuthService
{
    Task<bool> HasValidTokensAsync();
    Task StartAuthorizationFlowAsync();
    Task<string> GetAccessTokenAsync();
    Task RefreshTokenAsync();
    Task ClearTokensAsync();
}
