using backend.api.auth.models;

namespace backend.api.auth.intr;

public interface ILlmConfigProvider
{
    LlmConfig GetConfig();
    bool IsConfigured();
}


