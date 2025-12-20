using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using backend.api.auth.intr;
using backend.api.oura.intr;

namespace backend.api.oura.impl;

public class OuraClient : IOuraClient
{
    private readonly IOAuthService _oauthService;
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://api.ouraring.com/v2";
    
    public OuraClient(IOAuthService oauthService)
    {
        _oauthService = oauthService ?? throw new ArgumentNullException(nameof(oauthService));
        _httpClient = new HttpClient();
    }
    
    public async Task<T?> GetAsync<T>(string endpoint) where T : class
    {
        var accessToken = await _oauthService.GetAccessTokenAsync();
        
        var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/{endpoint.TrimStart('/')}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        var response = await _httpClient.SendAsync(request);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(
                $"Request to {endpoint} failed with status {(int)response.StatusCode} {response.StatusCode}. Response: {errorBody}");
        }
        
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(json);
    }
}
