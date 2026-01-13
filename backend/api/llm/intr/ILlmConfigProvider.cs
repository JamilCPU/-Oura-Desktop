using backend.api.llm.models;

namespace backend.api.llm.intr;

public interface ILlmConfigProvider
{
    LlmConfig GetConfig();
    bool IsConfigured();
}
