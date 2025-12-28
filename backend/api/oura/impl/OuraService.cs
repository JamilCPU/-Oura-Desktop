using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using backend.api.oura.intr;
using backend.api.oura.models;

namespace backend.api.oura.impl;

public class OuraService : IOuraService
{
    private readonly IOuraClient _client;
    private readonly string _today;
    private readonly string _yesterday;
    private readonly string _lastWeek;
    
    public OuraService(IOuraClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        
        var todayDate = DateTime.UtcNow;
        _today = todayDate.ToString("yyyy-MM-dd");
        _yesterday = todayDate.AddDays(-1).ToString("yyyy-MM-dd");
        _lastWeek = todayDate.AddDays(-7).ToString("yyyy-MM-dd");
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
    
    public async Task<ApiResponse<DailySleep>?> GetDailySleepAsync()
    {
        var endpoint = BuildEndpoint("usercollection/daily_sleep", _yesterday, _today);
        return await _client.GetAsync<ApiResponse<DailySleep>>(endpoint);
    }
    
    public async Task<ApiResponse<Sleep>?> GetSleepAsync()
    {
        var endpoint = BuildEndpoint("usercollection/sleep", _yesterday, _today);
        return await _client.GetAsync<ApiResponse<Sleep>>(endpoint);
    }
    
    public async Task<ApiResponse<DailyActivity>?> GetDailyActivityAsync()
    {
        var endpoint = BuildEndpoint("usercollection/daily_activity", _yesterday, _today);
        return await _client.GetAsync<ApiResponse<DailyActivity>>(endpoint);
    }
    
    public async Task<ApiResponse<DailyReadiness>?> GetDailyReadinessAsync()
    {
        var endpoint = BuildEndpoint("usercollection/daily_readiness", _yesterday, _today);
        return await _client.GetAsync<ApiResponse<DailyReadiness>>(endpoint);
    }
    
    public async Task<ApiResponse<HeartRate>?> GetHeartRateAsync()
    {
        var endpoint = BuildEndpoint("usercollection/heartrate", _yesterday, _today);
        return await _client.GetAsync<ApiResponse<HeartRate>>(endpoint);
    }
    
    public async Task<ApiResponse<Workout>?> GetWorkoutsAsync()
    {
        var endpoint = BuildEndpoint("usercollection/workout", _yesterday, _today);
        return await _client.GetAsync<ApiResponse<Workout>>(endpoint);
    }
    
    public async Task<ApiResponse<Session>?> GetSessionsAsync()
    {
        var endpoint = BuildEndpoint("usercollection/session", _yesterday, _today);
        return await _client.GetAsync<ApiResponse<Session>>(endpoint);
    }
    
    public async Task<ApiResponse<Tag>?> GetTagsAsync()
    {
        var endpoint = BuildEndpoint("usercollection/tag", _yesterday, _today);
        return await _client.GetAsync<ApiResponse<Tag>>(endpoint);
    }
    
    public async Task<ApiResponse<DailyStress>?> GetDailyStressAsync()
    {
        var endpoint = BuildEndpoint("usercollection/daily_stress", _lastWeek, _today);
        return await _client.GetAsync<ApiResponse<DailyStress>>(endpoint);
    }
    
    public async Task<ApiResponse<RestModePeriod>?> GetRestModePeriodsAsync()
    {
        var endpoint = BuildEndpoint("usercollection/rest_mode_period", _yesterday, _today);
        return await _client.GetAsync<ApiResponse<RestModePeriod>>(endpoint);
    }
    
    public async Task<ApiResponse<RingConfiguration>?> GetRingConfigurationAsync()
    {
        return await _client.GetAsync<ApiResponse<RingConfiguration>>("usercollection/ring_configuration");
    }
    
    public async Task<ApiResponse<DailySpo2>?> GetDailySpo2Async()
    {
        var endpoint = BuildEndpoint("usercollection/daily_spo2", _yesterday, _today);
        return await _client.GetAsync<ApiResponse<DailySpo2>>(endpoint);
    }
    
    public async Task<ApiResponse<DailyResilience>?> GetDailyResilienceAsync()
    {
        var endpoint = BuildEndpoint("usercollection/daily_resilience", _yesterday, _today);
        return await _client.GetAsync<ApiResponse<DailyResilience>>(endpoint);
    }
    
    public async Task<ApiResponse<Vo2Max>?> GetVo2MaxAsync()
    {
        var endpoint = BuildEndpoint("usercollection/vO2_max", _lastWeek, _today);
        return await _client.GetAsync<ApiResponse<Vo2Max>>(endpoint);
    }
}
