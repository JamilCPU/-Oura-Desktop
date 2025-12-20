using System;
using System.Text.Json.Serialization;

namespace backend.api.oura.models;

public class DailyActivity
{
    [JsonPropertyName("day")]
    public string Day { get; set; } = string.Empty;
    
    [JsonPropertyName("steps")]
    public int? Steps { get; set; }
    
    [JsonPropertyName("calories_total")]
    public int? CaloriesTotal { get; set; }
    
    [JsonPropertyName("calories_active")]
    public int? CaloriesActive { get; set; }
    
    [JsonPropertyName("distance")]
    public double? Distance { get; set; }
    
    [JsonPropertyName("activity_score")]
    public int? ActivityScore { get; set; }
    
    [JsonPropertyName("equivalent_walking_distance")]
    public double? EquivalentWalkingDistance { get; set; }
    
    [JsonPropertyName("inactive_time")]
    public int? InactiveTime { get; set; }
    
    [JsonPropertyName("low_activity_time")]
    public int? LowActivityTime { get; set; }
    
    [JsonPropertyName("medium_activity_time")]
    public int? MediumActivityTime { get; set; }
    
    [JsonPropertyName("high_activity_time")]
    public int? HighActivityTime { get; set; }
}

public class DailyReadiness
{
    [JsonPropertyName("day")]
    public string Day { get; set; } = string.Empty;
    
    [JsonPropertyName("score")]
    public int? Score { get; set; }
    
    [JsonPropertyName("temperature_deviation")]
    public double? TemperatureDeviation { get; set; }
    
    [JsonPropertyName("resting_heart_rate")]
    public int? RestingHeartRate { get; set; }
    
    [JsonPropertyName("hrv_balance")]
    public int? HrvBalance { get; set; }
    
    [JsonPropertyName("recovery_index")]
    public int? RecoveryIndex { get; set; }
    
    [JsonPropertyName("sleep_balance")]
    public int? SleepBalance { get; set; }
    
    [JsonPropertyName("previous_day_activity")]
    public int? PreviousDayActivity { get; set; }
    
    [JsonPropertyName("activity_balance")]
    public int? ActivityBalance { get; set; }
}
