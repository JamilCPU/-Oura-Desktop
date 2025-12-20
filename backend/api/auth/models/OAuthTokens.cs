using System;
using System.Text.Json.Serialization;

namespace backend.api.auth.models;

public class OAuthTokens
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;
    
    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = string.Empty;
    
    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = "bearer";
    
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
    
    public DateTime ExpiresAt { get; set; }
    
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
}
