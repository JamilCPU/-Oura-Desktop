using System;
using System.IO;
using System.Text.Json;
using backend.api.auth.intr;
using backend.api.auth.models;

namespace backend.api.auth.impl;

public class OAuthConfigProvider : IOAuthConfigProvider
{
    private readonly string _configFilePath;
    
    private OAuthConfig? _cachedConfig;

    public OAuthConfigProvider()
    {
        var solutionRoot = GetSolutionRoot();
        _configFilePath = Path.Combine(solutionRoot, "backend", "config", "oauth-config.json");
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

        if (!string.IsNullOrEmpty(envClientId) && !string.IsNullOrEmpty(envClientSecret))
        {
            _cachedConfig = new OAuthConfig
            {
                ClientId = envClientId,
                ClientSecret = envClientSecret,
                RedirectUri = "http://localhost:8080/callback",
                Scopes = new[] { "daily", "heartrate" }
            };
            return _cachedConfig;
        }

        var exampleConfigPath = Path.Combine(Path.GetDirectoryName(_configFilePath) ?? "", "oura-config.example.json");
        throw new InvalidOperationException(
            $"OAuth configuration not found. Please create {_configFilePath} " +
            $"or set OURA_CLIENT_ID and OURA_CLIENT_SECRET environment variables. " +
            $"See {exampleConfigPath} for an example. " +
            $"The config file should contain: {{\"client_id\":\"YOUR_CLIENT_ID\",\"client_secret\":\"YOUR_CLIENT_SECRET\",\"redirect_uri\":\"http://localhost:8080/callback\",\"scopes\":[\"daily\",\"heartrate\"]}}");
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
