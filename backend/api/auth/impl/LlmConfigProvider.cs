using System;
using System.IO;
using System.Text.Json;
using backend.api.auth.intr;
using backend.api.auth.models;

namespace backend.api.auth.impl;

public class LlmConfigProvider : ILlmConfigProvider
{
    private readonly string _configFilePath;
    private LlmConfig? _cachedConfig;

    public LlmConfigProvider()
    {
        var solutionRoot = GetSolutionRoot();
        _configFilePath = Path.Combine(solutionRoot, "backend", "config", "llm-config.json");
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

    public LlmConfig GetConfig()
    {
        if (_cachedConfig != null)
            return _cachedConfig;

        if (File.Exists(_configFilePath))
        {
            try
            {
                var json = File.ReadAllText(_configFilePath);
                var config = JsonSerializer.Deserialize<LlmConfig>(json);
                if (config != null && !string.IsNullOrEmpty(config.ModelPath))
                {
                    _cachedConfig = config;
                    return config;
                }
            }
            catch (JsonException)
            {
            }
        }

        throw new InvalidOperationException(
            $"LLM configuration not found. Please create {_configFilePath} " +
            $"with the following structure: {{\"model_path\":\"./models/llama-3.1-8b-instruct.Q4_K_M.gguf\",\"backend\":\"cpu\",\"context_size\":4096,\"gpu_layer_count\":0}}");
    }

    public bool IsConfigured()
    {
        try
        {
            var config = GetConfig();
            return !string.IsNullOrEmpty(config.ModelPath) && File.Exists(config.ModelPath);
        }
        catch
        {
            return false;
        }
    }
}
