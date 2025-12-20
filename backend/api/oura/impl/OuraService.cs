using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using backend.api.oura.intr;
using backend.api.oura.models;

namespace backend.api.oura.impl;

public class OuraService : IOuraService
{
    private readonly IOuraClient _client;
    
    public OuraService(IOuraClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }
    
    private string BuildEndpoint(string basePath, string? startDate = null, string? endDate = null)
    {
        var endpoint = basePath;
        var queryParams = new List<string>();
        
        if (!string.IsNullOrEmpty(startDate))
            queryParams.Add($"start_date={Uri.EscapeDataString(startDate)}");
        
        if (!string.IsNullOrEmpty(endDate))
            queryParams.Add($"end_date={Uri.EscapeDataString(endDate)}");
        
        if (queryParams.Count > 0)
            endpoint += "?" + string.Join("&", queryParams);
        
        return endpoint;
    }
    
    public async Task<ApiResponse<PersonalInfo>?> GetPersonalInfoAsync()
    {
        return await _client.GetAsync<ApiResponse<PersonalInfo>>("usercollection/personal_info");
    }
    
    public async Task<ApiResponse<DailySleep>?> GetDailySleepAsync(string? startDate = null, string? endDate = null)
    {
        var endpoint = BuildEndpoint("usercollection/daily_sleep", startDate, endDate);
        return await _client.GetAsync<ApiResponse<DailySleep>>(endpoint);
    }
    
    public async Task<ApiResponse<Sleep>?> GetSleepAsync(string? startDate = null, string? endDate = null)
    {
        var endpoint = BuildEndpoint("usercollection/sleep", startDate, endDate);
        return await _client.GetAsync<ApiResponse<Sleep>>(endpoint);
    }
    
    public async Task<ApiResponse<DailyActivity>?> GetDailyActivityAsync(string? startDate = null, string? endDate = null)
    {
        var endpoint = BuildEndpoint("usercollection/daily_activity", startDate, endDate);
        return await _client.GetAsync<ApiResponse<DailyActivity>>(endpoint);
    }
    
    public async Task<ApiResponse<DailyReadiness>?> GetDailyReadinessAsync(string? startDate = null, string? endDate = null)
    {
        var endpoint = BuildEndpoint("usercollection/daily_readiness", startDate, endDate);
        return await _client.GetAsync<ApiResponse<DailyReadiness>>(endpoint);
    }
    
    public async Task<ApiResponse<HeartRate>?> GetHeartRateAsync(string? startDate = null, string? endDate = null)
    {
        var endpoint = BuildEndpoint("usercollection/heartrate", startDate, endDate);
        return await _client.GetAsync<ApiResponse<HeartRate>>(endpoint);
    }
    
    public async Task<ApiResponse<Workout>?> GetWorkoutsAsync(string? startDate = null, string? endDate = null)
    {
        var endpoint = BuildEndpoint("usercollection/workout", startDate, endDate);
        return await _client.GetAsync<ApiResponse<Workout>>(endpoint);
    }
    
    public async Task<ApiResponse<Session>?> GetSessionsAsync(string? startDate = null, string? endDate = null)
    {
        var endpoint = BuildEndpoint("usercollection/session", startDate, endDate);
        return await _client.GetAsync<ApiResponse<Session>>(endpoint);
    }
    
    public async Task<ApiResponse<Tag>?> GetTagsAsync(string? startDate = null, string? endDate = null)
    {
        var endpoint = BuildEndpoint("usercollection/tag", startDate, endDate);
        return await _client.GetAsync<ApiResponse<Tag>>(endpoint);
    }
    
    public async Task<ApiResponse<DailyStress>?> GetDailyStressAsync(string? startDate = null, string? endDate = null)
    {
        var endpoint = BuildEndpoint("usercollection/daily_stress", startDate, endDate);
        return await _client.GetAsync<ApiResponse<DailyStress>>(endpoint);
    }
    
    public async Task<ApiResponse<RestModePeriod>?> GetRestModePeriodsAsync(string? startDate = null, string? endDate = null)
    {
        var endpoint = BuildEndpoint("usercollection/rest_mode_period", startDate, endDate);
        return await _client.GetAsync<ApiResponse<RestModePeriod>>(endpoint);
    }
    
    public async Task<ApiResponse<RingConfiguration>?> GetRingConfigurationAsync()
    {
        return await _client.GetAsync<ApiResponse<RingConfiguration>>("usercollection/ring_configuration");
    }
    
    public async Task<ApiResponse<DailySpo2>?> GetDailySpo2Async(string? startDate = null, string? endDate = null)
    {
        var endpoint = BuildEndpoint("usercollection/daily_spo2", startDate, endDate);
        return await _client.GetAsync<ApiResponse<DailySpo2>>(endpoint);
    }
    
    public async Task<ApiResponse<DailyResilience>?> GetDailyResilienceAsync(string? startDate = null, string? endDate = null)
    {
        var endpoint = BuildEndpoint("usercollection/daily_resilience", startDate, endDate);
        return await _client.GetAsync<ApiResponse<DailyResilience>>(endpoint);
    }
    
    public async Task<ApiResponse<Vo2Max>?> GetVo2MaxAsync(string? startDate = null, string? endDate = null)
    {
        var endpoint = BuildEndpoint("usercollection/vo2_max", startDate, endDate);
        return await _client.GetAsync<ApiResponse<Vo2Max>>(endpoint);
    }
}
