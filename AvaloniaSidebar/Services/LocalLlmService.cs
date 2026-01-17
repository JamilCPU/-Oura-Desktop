using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using backend.api.llm.impl;
using backend.api.llm.intr;
using LLama;
using LLama.Common;

namespace AvaloniaSidebar.Services;

public class LocalLlmService : ILocalLlmService, IDisposable
{
    private readonly ILlmConfigProvider _configProvider;
    private readonly IModelDownloadService _modelDownloadService;
    private LLamaWeights? _weights;
    private LLamaContext? _context;
    private InteractiveExecutor? _executor;
    private bool _initialized;

    public LocalLlmService(ILlmConfigProvider configProvider, IModelDownloadService modelDownloadService)
    {
        _configProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));
        _modelDownloadService = modelDownloadService ?? throw new ArgumentNullException(nameof(modelDownloadService));
    }

    public async Task InitializeAsync()
    {
        if (_initialized)
            return;

        var config = _configProvider.GetConfig();
        
        // Check if model file exists, if not, download it
        if (string.IsNullOrEmpty(config.ModelPath) || !File.Exists(config.ModelPath))
        {
            Console.WriteLine($"Model file not found at {config.ModelPath}. Starting download...");
            
            try
            {
                await _modelDownloadService.DownloadModelAsync(
                    ModelDownloadService.DefaultModelUrl,
                    config.ModelPath,
                    null,
                    CancellationToken.None);
                
                Console.WriteLine("Model download completed successfully.");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to download model file. {ex.Message}. Please ensure you have a stable internet connection and sufficient disk space.", ex);
            }
            
            // Verify file exists after download
            if (!File.Exists(config.ModelPath))
            {
                throw new InvalidOperationException(
                    $"Model file was not created at {config.ModelPath} after download.");
            }
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



