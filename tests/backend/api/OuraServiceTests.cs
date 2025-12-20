using System;
using System.Text.Json;
using backend.api.auth.impl;
using backend.api.oura.impl;
using backend.api.oura.intr;
using backend.api.oura.models;
using Xunit;
using Xunit.Abstractions;

namespace backend.api.oura.tests;

public class OuraServiceTests
{
    private readonly IOuraService _ouraService;
    private readonly ITestOutputHelper _output;

    public OuraServiceTests(ITestOutputHelper output)
    {
        _output = output;
        
        var configProvider = new OAuthConfigProvider();
        var tokenStore = new TokenStore();
        var oauthService = new OAuthService(configProvider, tokenStore);
        var ouraClient = new OuraClient(oauthService);
        _ouraService = new OuraService(ouraClient);
    }
    
    private void LogResponse<T>(string endpoint, T? response)
    {
        if (response == null)
        {
            var message = $"Endpoint: {endpoint} - Response: null";
            Console.WriteLine(message);
            _output.WriteLine(message);
            return;
        }
        
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
        
        var logMessage = $"Endpoint: {endpoint}\nResponse Body:\n{json}";
        Console.WriteLine(logMessage);
        _output.WriteLine(logMessage);
    }

    [Fact]
    public async Task GetPersonalInfoAsync_ReturnsValidData()
    {
        var result = await _ouraService.GetPersonalInfoAsync();

        LogResponse("usercollection/personal_info", result);
        
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task GetDailySleepAsync_ReturnsValidData()
    {
        var result = await _ouraService.GetDailySleepAsync();

        LogResponse("usercollection/daily_sleep", result);
        
        Assert.NotNull(result);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task GetDailySleepAsync_WithDates_ReturnsValidData()
    {
        var startDate = DateTime.UtcNow.AddDays(-7).ToString("yyyy-MM-dd");
        var endDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
        
        var result = await _ouraService.GetDailySleepAsync(startDate, endDate);
        
        LogResponse($"usercollection/daily_sleep?start_date={startDate}&end_date={endDate}", result);

        Assert.NotNull(result);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task GetAllEndpoints_ReturnValidData()
    {
        await CallAndLogEndpoint("usercollection/personal_info", () => _ouraService.GetPersonalInfoAsync());
        await CallAndLogEndpoint("usercollection/daily_sleep", () => _ouraService.GetDailySleepAsync());
        await CallAndLogEndpoint("usercollection/sleep", () => _ouraService.GetSleepAsync());
        await CallAndLogEndpoint("usercollection/daily_activity", () => _ouraService.GetDailyActivityAsync());
        await CallAndLogEndpoint("usercollection/daily_readiness", () => _ouraService.GetDailyReadinessAsync());
        await CallAndLogEndpoint("usercollection/heartrate", () => _ouraService.GetHeartRateAsync());
        await CallAndLogEndpoint("usercollection/workout", () => _ouraService.GetWorkoutsAsync());
        await CallAndLogEndpoint("usercollection/session", () => _ouraService.GetSessionsAsync());
        await CallAndLogEndpoint("usercollection/tag", () => _ouraService.GetTagsAsync());
        await CallAndLogEndpoint("usercollection/daily_stress", () => _ouraService.GetDailyStressAsync());
        await CallAndLogEndpoint("usercollection/rest_mode_period", () => _ouraService.GetRestModePeriodsAsync());
        await CallAndLogEndpoint("usercollection/ring_configuration", () => _ouraService.GetRingConfigurationAsync());
        await CallAndLogEndpoint("usercollection/daily_spo2", () => _ouraService.GetDailySpo2Async());
        await CallAndLogEndpoint("usercollection/daily_resilience", () => _ouraService.GetDailyResilienceAsync());
        await CallAndLogEndpoint("usercollection/vo2_max", () => _ouraService.GetVo2MaxAsync());
    }
    
    private async Task CallAndLogEndpoint<T>(string endpointName, Func<Task<T?>> endpointCall) where T : class
    {
        try
        {
            var result = await endpointCall();
            LogResponse(endpointName, result);
            Assert.NotNull(result);
        }
        catch (System.Net.Http.HttpRequestException ex)
        {
            _output.WriteLine($"Endpoint {endpointName} failed: {ex.Message}");
            Console.WriteLine($"Endpoint {endpointName} failed: {ex.Message}");
            throw;
        }
    }
}
