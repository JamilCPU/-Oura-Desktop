using System.Text.Json.Serialization;

namespace backend.api.llm.models;

public class LlmConfig
{
    [JsonPropertyName("model_path")]
    public string ModelPath { get; set; } = string.Empty;
    
    [JsonPropertyName("backend")]
    public string Backend { get; set; } = "cpu";
    
    [JsonPropertyName("context_size")]
    public int ContextSize { get; set; } = 4096;
    
    [JsonPropertyName("gpu_layer_count")]
    public int GpuLayerCount { get; set; } = 0;
}
