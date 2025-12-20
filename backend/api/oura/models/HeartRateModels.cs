using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace backend.api.oura.models;

public class HeartRate
{
    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = string.Empty;
    
    [JsonPropertyName("bpm")]
    public int? Bpm { get; set; }
}

public class Workout
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("start_datetime")]
    public string StartDatetime { get; set; } = string.Empty;
    
    [JsonPropertyName("end_datetime")]
    public string EndDatetime { get; set; } = string.Empty;
    
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
    
    [JsonPropertyName("intensity")]
    public string Intensity { get; set; } = string.Empty;
    
    [JsonPropertyName("calories")]
    public int? Calories { get; set; }
    
    [JsonPropertyName("average_heart_rate")]
    public int? AverageHeartRate { get; set; }
    
    [JsonPropertyName("max_heart_rate")]
    public int? MaxHeartRate { get; set; }
    
    [JsonPropertyName("duration")]
    public int? Duration { get; set; }
}

public class Session
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("day")]
    public string Day { get; set; } = string.Empty;
    
    [JsonPropertyName("start_datetime")]
    public string StartDatetime { get; set; } = string.Empty;
    
    [JsonPropertyName("end_datetime")]
    public string EndDatetime { get; set; }
    
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
    
    [JsonPropertyName("heart_rate")]
    public SessionHeartRate? HeartRate { get; set; }
}

public class SessionHeartRate
{
    [JsonPropertyName("interval")]
    public int? Interval { get; set; }
    
    [JsonPropertyName("items")]
    public List<int> Items { get; set; } = new();
}
