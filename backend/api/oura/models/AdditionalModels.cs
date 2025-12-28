using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace backend.api.oura.models;

public class Tag
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("day")]
    public string Day { get; set; } = string.Empty;
    
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
    
    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = new();
}

public class DailyStress
{
    [JsonPropertyName("day")]
    public string Day { get; set; } = string.Empty;
    
    [JsonPropertyName("stress_high")]
    public int? StressHigh { get; set; }
    
    [JsonPropertyName("stress_medium")]
    public int? StressMedium { get; set; }
    
    [JsonPropertyName("stress_low")]
    public int? StressLow { get; set; }
    
    [JsonPropertyName("recovery_high")]
    public int? RecoveryHigh { get; set; }
    
    [JsonPropertyName("recovery_medium")]
    public int? RecoveryMedium { get; set; }
    
    [JsonPropertyName("recovery_low")]
    public int? RecoveryLow { get; set; }
}

public class RestModePeriod
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("start_datetime")]
    public string StartDatetime { get; set; } = string.Empty;
    
    [JsonPropertyName("end_datetime")]
    public string? EndDatetime { get; set; }
}

public class RingConfiguration
{
    [JsonPropertyName("color")]
    public string Color { get; set; } = string.Empty;
    
    [JsonPropertyName("design")]
    public string Design { get; set; } = string.Empty;
    
    [JsonPropertyName("firmware_version")]
    public string FirmwareVersion { get; set; } = string.Empty;
    
    [JsonPropertyName("hardware_type")]
    public string HardwareType { get; set; } = string.Empty;
}

public class DailySpo2
{
    [JsonPropertyName("day")]
    public string Day { get; set; } = string.Empty;
    
    [JsonPropertyName("spo2_percentage")]
    public Spo2Percentage? Spo2Percentage { get; set; }
    
    [JsonPropertyName("spo2_quality")]
    public string Spo2Quality { get; set; } = string.Empty;
}

public class Spo2Percentage
{
    [JsonPropertyName("average")]
    public double? Average { get; set; }
}

public class DailyResilience
{
    [JsonPropertyName("day")]
    public string Day { get; set; } = string.Empty;
    
    [JsonPropertyName("resilience")]
    public int? Resilience { get; set; }
}

public class Vo2Max
{
    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = string.Empty;
    
    [JsonPropertyName("vo2_max")]
    public double? Vo2MaxValue { get; set; }
}
