using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using backend.api.auth.intr;
using backend.api.auth.models;

namespace backend.api.auth.impl;

public class TokenStore : ITokenStore
{
    private readonly string _tokensFilePath;
    
    public TokenStore()
    {
        var solutionRoot = GetSolutionRoot();
        _tokensFilePath = Path.Combine(solutionRoot, "backend", "config", "oauth-tokens.json");
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
    
    public async Task<OAuthTokens?> LoadTokensAsync()
    {
        if (!File.Exists(_tokensFilePath))
            return null;
        
        try
        {
            var json = await File.ReadAllTextAsync(_tokensFilePath);
            var tokens = JsonSerializer.Deserialize<OAuthTokens>(json);
            return tokens;
        }
        catch
        {
            return null;
        }
    }
    
    public async Task SaveTokensAsync(OAuthTokens tokens)
    {
        var directory = Path.GetDirectoryName(_tokensFilePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        tokens.ExpiresAt = DateTime.UtcNow.AddSeconds(tokens.ExpiresIn);
        
        var json = JsonSerializer.Serialize(tokens, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
        
        await File.WriteAllTextAsync(_tokensFilePath, json);
    }
    
    public async Task<bool> HasValidTokensAsync()
    {
        var tokens = await LoadTokensAsync();
        return tokens != null && !tokens.IsExpired && !string.IsNullOrEmpty(tokens.AccessToken);
    }
    
    public async Task ClearTokensAsync()
    {
        if (File.Exists(_tokensFilePath))
        {
            await Task.Run(() => File.Delete(_tokensFilePath));
        }
    }
}
