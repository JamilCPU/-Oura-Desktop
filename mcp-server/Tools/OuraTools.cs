using System.ComponentModel;
using System.Text.Json;
using System.Threading.Tasks;
using backend.api.oura.intr;
using ModelContextProtocol.Server;

namespace mcp_server.Tools;

[McpServerToolType]
public class OuraTools
{
    private readonly IOuraService _ouraService;

    public OuraTools(IOuraService ouraService)
    {
        _ouraService = ouraService ?? throw new ArgumentNullException(nameof(ouraService));
    }

    [McpServerTool]
    [Description("Gets personal information including age, weight, and height.")]
    public async Task<string> GetPersonalInfo()
    {
        var result = await _ouraService.GetPersonalInfoAsync();
        return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool]
    [Description("Gets daily sleep data including sleep score, duration, and efficiency.")]
    public async Task<string> GetDailySleep()
    {
        var result = await _ouraService.GetDailySleepAsync();
        return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool]
    [Description("Gets detailed sleep session data including sleep stages and timestamps.")]
    public async Task<string> GetSleep()
    {
        var result = await _ouraService.GetSleepAsync();
        return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool]
    [Description("Gets daily activity data including steps, calories, and activity score.")]
    public async Task<string> GetDailyActivity()
    {
        var result = await _ouraService.GetDailyActivityAsync();
        return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool]
    [Description("Gets daily readiness score indicating recovery and readiness for activity.")]
    public async Task<string> GetDailyReadiness()
    {
        var result = await _ouraService.GetDailyReadinessAsync();
        return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool]
    [Description("Gets heart rate data including BPM readings and timestamps.")]
    public async Task<string> GetHeartRate()
    {
        var result = await _ouraService.GetHeartRateAsync();
        return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool]
    [Description("Gets workout data including exercise sessions and metrics.")]
    public async Task<string> GetWorkouts()
    {
        var result = await _ouraService.GetWorkoutsAsync();
        return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool]
    [Description("Gets session data including meditation and other activity sessions.")]
    public async Task<string> GetSessions()
    {
        var result = await _ouraService.GetSessionsAsync();
        return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool]
    [Description("Gets tag data for organizing and categorizing activities.")]
    public async Task<string> GetTags()
    {
        var result = await _ouraService.GetTagsAsync();
        return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool]
    [Description("Gets daily stress data including stress scores and recovery metrics.")]
    public async Task<string> GetDailyStress()
    {
        var result = await _ouraService.GetDailyStressAsync();
        return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool]
    [Description("Gets rest mode period data indicating periods of rest and recovery.")]
    public async Task<string> GetRestModePeriods()
    {
        var result = await _ouraService.GetRestModePeriodsAsync();
        return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool]
    [Description("Gets ring configuration settings and preferences.")]
    public async Task<string> GetRingConfiguration()
    {
        var result = await _ouraService.GetRingConfigurationAsync();
        return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool]
    [Description("Gets daily SpO2 (blood oxygen saturation) data.")]
    public async Task<string> GetDailySpo2()
    {
        var result = await _ouraService.GetDailySpo2Async();
        return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool]
    [Description("Gets daily resilience metrics indicating ability to recover from stress.")]
    public async Task<string> GetDailyResilience()
    {
        var result = await _ouraService.GetDailyResilienceAsync();
        return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool]
    [Description("Gets VO2 max data indicating cardiovascular fitness level.")]
    public async Task<string> GetVo2Max()
    {
        var result = await _ouraService.GetVo2MaxAsync();
        return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
    }
}
