using System;
using System.IO;
using System.Text.Json;
using backend.api.llm.intr;
using backend.api.llm.models;

namespace backend.api.llm.impl;

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

        // File doesn't exist or is invalid - create it with default structure
        return CreateDefaultConfigFile();
    }

    private LlmConfig CreateDefaultConfigFile()
    {
        // Ensure the config directory exists
        var configDirectory = Path.GetDirectoryName(_configFilePath);
        if (!string.IsNullOrEmpty(configDirectory) && !Directory.Exists(configDirectory))
        {
            Directory.CreateDirectory(configDirectory);
        }

        // Get solution root and create models directory
        var solutionRoot = GetSolutionRoot();
        var modelsDirectory = Path.Combine(solutionRoot, "models");
        
        // Create models directory if it doesn't exist
        if (!Directory.Exists(modelsDirectory))
        {
            Directory.CreateDirectory(modelsDirectory);
        }

        // Create default config with absolute path to models directory
        var defaultModelPath = Path.Combine(modelsDirectory, "llama-3.1-8b-instruct.Q4_K_M.gguf");
        var defaultConfig = new LlmConfig
        {
            ModelPath = defaultModelPath,
            Backend = "cpu",
            ContextSize = 4096,
            GpuLayerCount = 0
        };

        // Serialize and write to file
        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        
        var json = JsonSerializer.Serialize(defaultConfig, jsonOptions);
        File.WriteAllText(_configFilePath, json);

        _cachedConfig = defaultConfig;
        return defaultConfig;
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
