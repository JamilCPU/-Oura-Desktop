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

        // Validate file before attempting to load
        if (!File.Exists(config.ModelPath))
        {
            throw new FileNotFoundException($"Model file not found at: {config.ModelPath}");
        }

        var fileInfo = new FileInfo(config.ModelPath);
        if (fileInfo.Length == 0)
        {
            throw new InvalidOperationException($"Model file is empty: {config.ModelPath}");
        }

        // Check if file is reasonably sized (at least 1GB for a GGUF model)
        if (fileInfo.Length < 1024L * 1024 * 1024)
        {
            throw new InvalidOperationException(
                $"Model file appears to be too small ({fileInfo.Length / (1024.0 * 1024 * 1024):F2} GB). " +
                $"Expected at least 1GB. File may be corrupted or incomplete: {config.ModelPath}");
        }

        Console.WriteLine($"Loading model from: {config.ModelPath}");
        Console.WriteLine($"Model file size: {fileInfo.Length / (1024.0 * 1024 * 1024):F2} GB");

        var modelParams = new ModelParams(config.ModelPath)
        {
            ContextSize = (uint)config.ContextSize,
            GpuLayerCount = config.Backend.ToLower() == "cpu" ? 0 : config.GpuLayerCount
        };

        try
        {
            Console.WriteLine("Loading LLamaWeights...");
            _weights = LLamaWeights.LoadFromFile(modelParams);
            Console.WriteLine("LLamaWeights loaded successfully.");
            
            Console.WriteLine("Creating LLamaContext...");
            var context = _weights.CreateContext(modelParams);
            _context = context;
            Console.WriteLine("LLamaContext created successfully.");
            
            Console.WriteLine("Creating InteractiveExecutor...");
            _executor = new InteractiveExecutor(context);
            Console.WriteLine("InteractiveExecutor created successfully.");
            
            _initialized = true;
            Console.WriteLine("LocalLlmService initialized successfully.");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to load model from '{config.ModelPath}'. " +
                $"File exists: {File.Exists(config.ModelPath)}, " +
                $"File size: {fileInfo.Length} bytes ({fileInfo.Length / (1024.0 * 1024 * 1024):F2} GB). " +
                $"Error: {ex.Message}. " +
                $"Inner exception: {ex.InnerException?.Message ?? "None"}", ex);
        }

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



