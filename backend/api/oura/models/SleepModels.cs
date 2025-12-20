using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace backend.api.oura.models;

public class DailySleep
{
    [JsonPropertyName("day")]
    public string Day { get; set; } = string.Empty;
    
    [JsonPropertyName("total_sleep_duration")]
    public int? TotalSleepDuration { get; set; }
    
    [JsonPropertyName("deep_sleep_duration")]
    public int? DeepSleepDuration { get; set; }
    
    [JsonPropertyName("rem_sleep_duration")]
    public int? RemSleepDuration { get; set; }
    
    [JsonPropertyName("light_sleep_duration")]
    public int? LightSleepDuration { get; set; }
    
    [JsonPropertyName("awake_time")]
    public int? AwakeTime { get; set; }
    
    [JsonPropertyName("efficiency")]
    public int? Efficiency { get; set; }
    
    [JsonPropertyName("average_hrv")]
    public double? AverageHrv { get; set; }
    
    [JsonPropertyName("lowest_heart_rate")]
    public int? LowestHeartRate { get; set; }
    
    [JsonPropertyName("score")]
    public int? Score { get; set; }
}

public class Sleep
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("contributors")]
    public SleepContributors? Contributors { get; set; }
    
    [JsonPropertyName("day")]
    public string Day { get; set; } = string.Empty;
    
    [JsonPropertyName("score")]
    public int? Score { get; set; }
    
    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = string.Empty;
}

public class SleepContributors
{
    [JsonPropertyName("deep_sleep")]
    public int? DeepSleep { get; set; }
    
    [JsonPropertyName("efficiency")]
    public int? Efficiency { get; set; }
    
    [JsonPropertyName("latency")]
    public int? Latency { get; set; }
    
    [JsonPropertyName("rem_sleep")]
    public int? RemSleep { get; set; }
    
    [JsonPropertyName("restfulness")]
    public int? Restfulness { get; set; }
    
    [JsonPropertyName("timing")]
    public int? Timing { get; set; }
    
    [JsonPropertyName("total_sleep")]
    public int? TotalSleep { get; set; }
}
