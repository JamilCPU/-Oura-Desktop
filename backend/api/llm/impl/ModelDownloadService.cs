using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using backend.api.llm.intr;

namespace backend.api.llm.impl;

public class ModelDownloadService : IModelDownloadService
{
    private readonly HttpClient _httpClient;
    
    public const string DefaultModelUrl = "https://huggingface.co/bartowski/Meta-Llama-3.1-8B-Instruct-GGUF/resolve/main/Meta-Llama-3.1-8B-Instruct-Q4_K_M.gguf?download=true";
    
    public ModelDownloadService()
    {
        _httpClient = new HttpClient();
        // Set a longer timeout for large file downloads (30 minutes)
        _httpClient.Timeout = TimeSpan.FromMinutes(30);
    }
    
    public async Task DownloadModelAsync(string modelUrl, string destinationPath, IProgress<long>? progress = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(modelUrl))
            throw new ArgumentException("Model URL cannot be null or empty.", nameof(modelUrl));
        
        if (string.IsNullOrWhiteSpace(destinationPath))
            throw new ArgumentException("Destination path cannot be null or empty.", nameof(destinationPath));
        
        // Ensure destination directory exists
        var destinationDirectory = Path.GetDirectoryName(destinationPath);
        if (!string.IsNullOrEmpty(destinationDirectory) && !Directory.Exists(destinationDirectory))
        {
            Directory.CreateDirectory(destinationDirectory);
        }
        
        // Check available disk space (rough estimate - need at least 5GB free)
        var driveInfo = new DriveInfo(Path.GetPathRoot(destinationPath) ?? destinationPath);
        if (driveInfo.AvailableFreeSpace < 5L * 1024 * 1024 * 1024) // 5GB
        {
            throw new InvalidOperationException(
                $"Insufficient disk space. At least 5GB free space required. Available: {driveInfo.AvailableFreeSpace / (1024.0 * 1024 * 1024):F2} GB");
        }
        
        // Delete existing file if it exists (in case of partial download)
        if (File.Exists(destinationPath))
        {
            File.Delete(destinationPath);
        }
        
        try
        {
            using var response = await _httpClient.GetAsync(modelUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var totalBytes = response.Content.Headers.ContentLength ?? -1L;
            var canReportProgress = totalBytes > 0 && progress != null;
            
            using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);
            
            var buffer = new byte[8192];
            var totalBytesRead = 0L;
            int bytesRead;
            
            Console.WriteLine($"Downloading model to {destinationPath}...");
            if (totalBytes > 0)
            {
                Console.WriteLine($"Total size: {totalBytes / (1024.0 * 1024 * 1024):F2} GB");
            }
            
            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                totalBytesRead += bytesRead;
                
                if (canReportProgress)
                {
                    progress?.Report(totalBytesRead);
                    
                    // Log progress every 10%
                    var percentComplete = (double)totalBytesRead / totalBytes * 100;
                    if (totalBytesRead % (totalBytes / 10) < 8192 || totalBytesRead == totalBytes)
                    {
                        Console.WriteLine($"Download progress: {percentComplete:F1}% ({totalBytesRead / (1024.0 * 1024 * 1024):F2} GB / {totalBytes / (1024.0 * 1024 * 1024):F2} GB)");
                    }
                }
            }
            
            // Verify file was downloaded
            if (!File.Exists(destinationPath))
            {
                throw new InvalidOperationException("Download completed but file was not created.");
            }
            
            var fileInfo = new FileInfo(destinationPath);
            if (fileInfo.Length == 0)
            {
                File.Delete(destinationPath);
                throw new InvalidOperationException("Downloaded file is empty. Download may have failed.");
            }
            
            // If we know the expected size, verify it matches
            if (totalBytes > 0 && fileInfo.Length != totalBytes)
            {
                File.Delete(destinationPath);
                throw new InvalidOperationException(
                    $"Downloaded file size ({fileInfo.Length}) does not match expected size ({totalBytes}). File may be corrupted.");
            }
            
            Console.WriteLine($"Model downloaded successfully to {destinationPath}");
            Console.WriteLine($"File size: {fileInfo.Length / (1024.0 * 1024 * 1024):F2} GB");
        }
        catch (HttpRequestException ex)
        {
            // Clean up partial download on error
            if (File.Exists(destinationPath))
            {
                try { File.Delete(destinationPath); } catch { }
            }
            
            throw new InvalidOperationException(
                $"Failed to download model from {modelUrl}. Network error: {ex.Message}", ex);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            // Clean up partial download on timeout
            if (File.Exists(destinationPath))
            {
                try { File.Delete(destinationPath); } catch { }
            }
            
            throw new InvalidOperationException(
                $"Download timed out. The model file is large and may take several minutes to download. Please check your internet connection and try again.", ex);
        }
        catch (OperationCanceledException)
        {
            // Clean up partial download on cancellation
            if (File.Exists(destinationPath))
            {
                try { File.Delete(destinationPath); } catch { }
            }
            
            throw;
        }
    }
}
