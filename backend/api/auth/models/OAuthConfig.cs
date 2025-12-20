using System.Text.Json.Serialization;

namespace backend.api.auth.models;

public class OAuthConfig
{
    [JsonPropertyName("client_id")]
    public string ClientId { get; set; } = string.Empty;
    
    [JsonPropertyName("client_secret")]
    public string ClientSecret { get; set; } = string.Empty;
    
    [JsonPropertyName("redirect_uri")]
    public string RedirectUri { get; set; } = "https://github.com/JamilCPU/-Oura-Desktop";
    
    [JsonPropertyName("scopes")]
    public string[] Scopes { get; set; } = { "daily", "heartrate" };
}
