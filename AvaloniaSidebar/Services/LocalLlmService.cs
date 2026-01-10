using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using backend.api.auth.impl;
using backend.api.auth.intr;
using backend.api.auth.models;
using LLama;
using LLama.Common;

namespace AvaloniaSidebar.Services;

public class LocalLlmService : ILocalLlmService, IDisposable
{
    private readonly ILlmConfigProvider _configProvider;
    private LLamaWeights? _weights;
    private LLamaContext? _context;
    private InteractiveExecutor? _executor;
    private bool _initialized;

    public LocalLlmService(ILlmConfigProvider configProvider)
    {
        _configProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));
    }

    public async Task InitializeAsync()
    {
        if (_initialized)
            return;

        var config = _configProvider.GetConfig();
        
        if (string.IsNullOrEmpty(config.ModelPath) || !File.Exists(config.ModelPath))
        {
            throw new InvalidOperationException(
                $"Model file not found at {config.ModelPath}. Please ensure the model file exists.");
        }

        var modelParams = new ModelParams(config.ModelPath)
        {
            ContextSize = (uint)config.ContextSize,
            GpuLayerCount = config.Backend.ToLower() == "cpu" ? 0 : config.GpuLayerCount
        };

        _weights = LLamaWeights.LoadFromFile(modelParams);
        var context = _weights.CreateContext(modelParams);
        _context = context;
        _executor = new InteractiveExecutor(context);
        _initialized = true;

        await Task.CompletedTask;
    }

    public async Task<string> GenerateResponseAsync(string prompt, CancellationToken cancellationToken = default)
    {
        if (!_initialized || _executor == null)
        {
            throw new InvalidOperationException("LLM service not initialized. Call InitializeAsync first.");
        }

        var inferenceParams = new InferenceParams
        {
            Temperature = 0.7f,
            MaxTokens = 512,
            AntiPrompts = new[] { "User:", "\n\n" }
        };

        var response = new System.Text.StringBuilder();
        
        await foreach (var token in _executor.InferAsync(prompt, inferenceParams, cancellationToken))
        {
            response.Append(token);
        }

        return response.ToString();
    }

    public void Dispose()
    {
        // InteractiveExecutor doesn't implement IDisposable in LLamaSharp 0.13.0
        _context?.Dispose();
        _weights?.Dispose();
        _initialized = false;
    }
}



