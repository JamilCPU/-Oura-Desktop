using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AvaloniaSidebar.Utils;
using backend.api.llm.impl;
using backend.api.llm.intr;
using LLama;
using LLama.Common;

namespace AvaloniaSidebar.Services;

public class LocalLlmService : ILocalLlmService, IDisposable
{
    private readonly ILlmConfigProvider _configProvider;
    private readonly IModelDownloadService _modelDownloadService;
    private readonly Logger _logger;
    private LLamaWeights? _weights;
    private LLamaContext? _context;
    private InteractiveExecutor? _executor;
    private bool _initialized;

    public LocalLlmService(ILlmConfigProvider configProvider, IModelDownloadService modelDownloadService)
    {
        _configProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));
        _modelDownloadService = modelDownloadService ?? throw new ArgumentNullException(nameof(modelDownloadService));
        _logger = new Logger("LLM");
    }

    public async Task InitializeAsync()
    {
        if (_initialized)
            return;

        var config = _configProvider.GetConfig();
        
        // Check if model file exists, if not, download it
        if (string.IsNullOrEmpty(config.ModelPath) || !File.Exists(config.ModelPath))
        {
            _logger.Log("Downloading model...");
            
            try
            {
                await _modelDownloadService.DownloadModelAsync(
                    ModelDownloadService.DefaultModelUrl,
                    config.ModelPath,
                    null,
                    CancellationToken.None);
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

        // Validate file format - check if it's actually a GGUF file or HTML error page
        using (var fileStream = new FileStream(config.ModelPath, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            var buffer = new byte[Math.Min(1024, fileInfo.Length)];
            var bytesRead = fileStream.Read(buffer, 0, buffer.Length);
            
            if (bytesRead > 0)
            {
                var fileStart = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
                
                // Check if it's an HTML error page
                if (fileStart.TrimStart().StartsWith("<!DOCTYPE", StringComparison.OrdinalIgnoreCase) ||
                    fileStart.TrimStart().StartsWith("<html", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        $"The downloaded file appears to be an HTML error page, not a model file. " +
                        $"This usually means the download URL is incorrect or the file is not accessible. " +
                        $"File path: {config.ModelPath}");
                }
                
                // Check for GGUF magic bytes (GGUF files start with "GGUF" or have specific header)
                // Note: GGUF format may vary, but we can at least check it's not text/HTML
                if (fileStart.Contains("<", StringComparison.Ordinal) && fileStart.Contains(">", StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        $"The file appears to contain HTML/XML content, not a valid GGUF model file. " +
                        $"File path: {config.ModelPath}");
                }
            }
        }

        var modelParams = new ModelParams(config.ModelPath)
        {
            ContextSize = (uint)config.ContextSize,
            GpuLayerCount = config.Backend.ToLower() == "cpu" ? 0 : config.GpuLayerCount
        };

        try
        {
            _logger.Log("Loading model...");
            
            // Try to load with more detailed error handling
            try
            {
                _weights = LLamaWeights.LoadFromFile(modelParams);
            }
            catch (DllNotFoundException dllEx)
            {
                throw new InvalidOperationException(
                    $"Missing native library dependency. LLamaSharp requires native libraries to be available. " +
                    $"Error: {dllEx.Message}. " +
                    $"Make sure LLamaSharp.Backend.Cpu is properly installed.", dllEx);
            }
            catch (BadImageFormatException badImageEx)
            {
                throw new InvalidOperationException(
                    $"Native library architecture mismatch. The LLamaSharp native library doesn't match your system architecture (x64/x86/ARM64). " +
                    $"Error: {badImageEx.Message}. " +
                    $"Make sure you're using the correct LLamaSharp.Backend package for your system.", badImageEx);
            }
            catch (Exception loadEx)
            {
                // Check if the file might be corrupted or wrong format
                var firstBytes = new byte[16];
                using (var fs = new FileStream(config.ModelPath, FileMode.Open, FileAccess.Read))
                {
                    var bytesRead = fs.Read(firstBytes, 0, 16);
                    if (bytesRead < 16)
                    {
                        Array.Resize(ref firstBytes, bytesRead);
                    }
                }
                var hexStart = BitConverter.ToString(firstBytes).Replace("-", " ");
                
                // Check for GGUF magic bytes and version
                var isGguf = firstBytes.Length >= 4 && 
                    firstBytes[0] == 0x47 && firstBytes[1] == 0x47 && firstBytes[2] == 0x55 && firstBytes[3] == 0x46; // "GGUF"
                
                var ggufVersion = isGguf && firstBytes.Length >= 8 
                    ? BitConverter.ToUInt32(firstBytes, 4) 
                    : (uint?)null;
                
                var formatNote = isGguf 
                    ? $"Valid GGUF v{ggufVersion ?? 0} file, but LLamaSharp failed to load it."
                    : "File does not appear to be a valid GGUF file.";
                
                throw new InvalidOperationException(
                    $"Failed to load model: {formatNote} {loadEx.Message}", loadEx);
            }
            
            var context = _weights.CreateContext(modelParams);
            _context = context;
            _executor = new InteractiveExecutor(context);
            _initialized = true;
            
            _logger.Log("Model loaded successfully.");
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
            MaxTokens = 2048,

            AntiPrompts = new[] { "User:" },
            SamplingPipeline = new LLama.Sampling.DefaultSamplingPipeline
            {
                Temperature = 0.7f
            }
        };

        var response = new System.Text.StringBuilder();
        var tokenCount = 0;
        var firstTokenReceived = false;
        
        await foreach (var token in _executor.InferAsync(prompt, inferenceParams, cancellationToken))
        {
            if (!firstTokenReceived)
            {
                _logger.Log($"First token received: '{token}'");
                firstTokenReceived = true;
            }
            
            response.Append(token);
            tokenCount++;
            
            // Log progress every 50 tokens
            if (tokenCount % 50 == 0)
            {
                _logger.LogProgress(".");
            }
        }
        
        var responseText = response.ToString();
        _logger.Log($"Generated {tokenCount} tokens, response length: {responseText.Length} chars");
        
        if (tokenCount == 0)
        {
            _logger.LogError("No tokens generated - InferAsync returned empty sequence", null);
            _logger.Log($"Prompt length: {prompt.Length} chars, ends with: ...{prompt.Substring(Math.Max(0, prompt.Length - 100))}");
        }

        return responseText;
    }

    public void Dispose()
    {
        // InteractiveExecutor doesn't implement IDisposable in LLamaSharp 0.13.0
        _context?.Dispose();
        _weights?.Dispose();
        _initialized = false;
    }
}



