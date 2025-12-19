using System;
using System.IO;
using System.Text.Json;
using backend.api.auth.intr;
using backend.api.auth.models;

namespace backend.api.auth.impl;

public class OAuthConfigProvider : IOAuthConfigProvider
{
    private readonly string _configFilePath;
    private readonly string _exampleConfigPath;
    
    private OAuthConfig? _cachedConfig;

    public OAuthConfigProvider()
    {
        var solutionRoot = GetSolutionRoot();
        _configFilePath = Path.Combine(solutionRoot, "AvaloniaSidebar", "config", "oura-config.json");
        _exampleConfigPath = Path.Combine(solutionRoot, "AvaloniaSidebar", "config", "oura-config.example.json");
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

    public OAuthConfig GetConfig()
    {
        if (_cachedConfig != null)
            return _cachedConfig;

        if (File.Exists(_configFilePath))
        {
            try
            {
                var json = File.ReadAllText(_configFilePath);
                var config = JsonSerializer.Deserialize<OAuthConfig>(json);
                if (config != null && !string.IsNullOrEmpty(config.ClientId))
                {
                    _cachedConfig = config;
                    return config;
                }
            }
            catch (JsonException)
            {
            }
        }

        var envClientId = Environment.GetEnvironmentVariable("OURA_CLIENT_ID");
        var envClientSecret = Environment.GetEnvironmentVariable("OURA_CLIENT_SECRET");
        var envRedirectUri = Environment.GetEnvironmentVariable("OURA_REDIRECT_URI");

        if (!string.IsNullOrEmpty(envClientId) && !string.IsNullOrEmpty(envClientSecret))
        {
            _cachedConfig = new OAuthConfig
            {
                ClientId = envClientId,
                ClientSecret = envClientSecret,
                RedirectUri = envRedirectUri ?? "http://localhost:8080/callback",
                Scopes = new[] { "daily", "heartrate" }
            };
            return _cachedConfig;
        }

        throw new InvalidOperationException(
            $"OAuth configuration not found. Please create {_configFilePath} " +
            $"or set OURA_CLIENT_ID and OURA_CLIENT_SECRET environment variables. " +
            $"See {_exampleConfigPath} for an example.");
    }

    public bool IsConfigured()
    {
        try
        {
            var config = GetConfig();
            return !string.IsNullOrEmpty(config.ClientId) && 
                   !string.IsNullOrEmpty(config.ClientSecret);
        }
        catch
        {
            return false;
        }
    }
}
