using backend.api.auth.models;

namespace backend.api.auth.intr;

public interface IOAuthConfigProvider
{
    OAuthConfig GetConfig();
    bool IsConfigured();
}
