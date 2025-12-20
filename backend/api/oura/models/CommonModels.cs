using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace backend.api.oura.models;

public class ApiResponse<T>
{
    [JsonPropertyName("data")]
    public List<T> Data { get; set; } = new();
    
    [JsonPropertyName("next_token")]
    public string? NextToken { get; set; }
}

public class PersonalInfo
{
    [JsonPropertyName("age")]
    public int? Age { get; set; }
    
    [JsonPropertyName("weight")]
    public double? Weight { get; set; }
    
    [JsonPropertyName("height")]
    public double? Height { get; set; }
}

