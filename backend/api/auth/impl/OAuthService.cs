using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using backend.api.auth.intr;
using backend.api.auth.models;

namespace backend.api.auth.impl;

public class OAuthService : IOAuthService
{
    private readonly IOAuthConfigProvider _configProvider;
    private readonly ITokenStore _tokenStore;
    private readonly HttpClient _httpClient;
    
    private const string AuthorizationUrl = "https://cloud.ouraring.com/oauth/authorize";
    private const string TokenUrl = "https://api.ouraring.com/oauth/token";
    private const int CallbackPort = 8080;
    private const string CallbackPath = "/callback";
    
    public OAuthService(IOAuthConfigProvider configProvider, ITokenStore tokenStore)
    {
        _configProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));
        _tokenStore = tokenStore ?? throw new ArgumentNullException(nameof(tokenStore));
        _httpClient = new HttpClient();
    }
    
    public async Task<bool> HasValidTokensAsync()
    {
        return await _tokenStore.HasValidTokensAsync();
    }
    
    public async Task StartAuthorizationFlowAsync()
    {
        var config = _configProvider.GetConfig();
        var state = GenerateRandomState();
        
        var authUrl = BuildAuthorizationUrl(config, state);
        
        var listener = new HttpListener();
        listener.Prefixes.Add($"http://localhost:{CallbackPort}/");
        listener.Start();
        
        OpenBrowser(authUrl);
        
        var authorizationCode = await WaitForAuthorizationCodeAsync(listener, state);
        
        listener.Stop();
        
        var tokens = await ExchangeCodeForTokensAsync(config, authorizationCode);
        await _tokenStore.SaveTokensAsync(tokens);
    }
    
    public async Task<string> GetAccessTokenAsync()
    {
        var tokens = await _tokenStore.LoadTokensAsync();
        if (tokens == null)
            throw new InvalidOperationException("No tokens found. Please authorize first.");
        
        if (tokens.IsExpired)
        {
            await RefreshTokenAsync();
            tokens = await _tokenStore.LoadTokensAsync();
        }
        
        return tokens?.AccessToken ?? throw new InvalidOperationException("Failed to get access token.");
    }
    
    public async Task RefreshTokenAsync()
    {
        var tokens = await _tokenStore.LoadTokensAsync();
        if (tokens == null || string.IsNullOrEmpty(tokens.RefreshToken))
            throw new InvalidOperationException("No refresh token available. Please re-authorize.");
        
        var config = _configProvider.GetConfig();
        
        var request = new HttpRequestMessage(HttpMethod.Post, TokenUrl);
        var formData = new Dictionary<string, string>
        {
            { "grant_type", "refresh_token" },
            { "refresh_token", tokens.RefreshToken },
            { "client_id", config.ClientId },
            { "client_secret", config.ClientSecret }
        };
        
        request.Content = new FormUrlEncodedContent(formData);
        
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync();
        var newTokens = JsonSerializer.Deserialize<OAuthTokens>(json);
        
        if (newTokens == null)
            throw new InvalidOperationException("Failed to refresh token.");
        
        await _tokenStore.SaveTokensAsync(newTokens);
    }
    
    public async Task ClearTokensAsync()
    {
        await _tokenStore.ClearTokensAsync();
    }
    
    private string BuildAuthorizationUrl(OAuthConfig config, string state)
    {
        var redirectUri = Uri.EscapeDataString(config.RedirectUri);
        
        return $"{AuthorizationUrl}?response_type=code&client_id={config.ClientId}&redirect_uri={redirectUri}&state={state}";
    }
    
    private string GenerateRandomState()
    {
        return Guid.NewGuid().ToString("N");
    }
    
    private void OpenBrowser(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch
        {
            throw new InvalidOperationException($"Failed to open browser. Please navigate to: {url}");
        }
    }
    
    private async Task<string> WaitForAuthorizationCodeAsync(HttpListener listener, string expectedState)
    {
        var context = await listener.GetContextAsync();
        var request = context.Request;
        var response = context.Response;
        
        var queryParams = ParseQueryString(request.Url?.Query ?? "");
        var error = queryParams.Get("error");
        
        if (!string.IsNullOrEmpty(error))
        {
            var errorDescription = queryParams.Get("error_description") ?? "Unknown error";
            SendResponse(response, $"<html><body><h1>Authorization Failed</h1><p>{errorDescription}</p><p>You can close this window.</p></body></html>");
            throw new InvalidOperationException($"OAuth authorization failed: {errorDescription}");
        }
        
        var code = queryParams.Get("code");
        var state = queryParams.Get("state");
        
        if (string.IsNullOrEmpty(code))
        {
            SendResponse(response, "<html><body><h1>Authorization Failed</h1><p>No authorization code received.</p><p>You can close this window.</p></body></html>");
            throw new InvalidOperationException("No authorization code received from Oura.");
        }
        
        if (state != expectedState)
        {
            SendResponse(response, "<html><body><h1>Security Error</h1><p>State mismatch detected.</p><p>You can close this window.</p></body></html>");
            throw new InvalidOperationException("State mismatch - possible CSRF attack.");
        }
        
        SendResponse(response, "<html><body><h1>Authorization Successful</h1><p>You can close this window and return to the application.</p></body></html>");
        
        return code;
    }
    
    private static NameValueCollection ParseQueryString(string query)
    {
        var result = new NameValueCollection();
        if (string.IsNullOrEmpty(query))
            return result;
        
        if (query.StartsWith("?"))
            query = query.Substring(1);
        
        var pairs = query.Split('&');
        foreach (var pair in pairs)
        {
            var parts = pair.Split('=', 2);
            if (parts.Length == 2)
            {
                result.Add(Uri.UnescapeDataString(parts[0]), Uri.UnescapeDataString(parts[1]));
            }
        }
        
        return result;
    }
    
    private void SendResponse(HttpListenerResponse response, string html)
    {
        var buffer = Encoding.UTF8.GetBytes(html);
        response.ContentLength64 = buffer.Length;
        response.ContentType = "text/html; charset=utf-8";
        response.OutputStream.Write(buffer, 0, buffer.Length);
        response.Close();
    }
    
    private async Task<OAuthTokens> ExchangeCodeForTokensAsync(OAuthConfig config, string authorizationCode)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, TokenUrl);
        var formData = new Dictionary<string, string>
        {
            { "grant_type", "authorization_code" },
            { "code", authorizationCode },
            { "redirect_uri", config.RedirectUri },
            { "client_id", config.ClientId },
            { "client_secret", config.ClientSecret }
        };
        
        request.Content = new FormUrlEncodedContent(formData);
        
        var response = await _httpClient.SendAsync(request);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"Token exchange failed with status {(int)response.StatusCode} {response.StatusCode}. " +
                $"Response: {errorBody}");
        }
        
        var json = await response.Content.ReadAsStringAsync();
        var tokens = JsonSerializer.Deserialize<OAuthTokens>(json);
        
        if (tokens == null)
            throw new InvalidOperationException("Failed to parse token response from Oura.");
        
        return tokens;
    }
}
