using System.Threading.Tasks;
using backend.api.oura.models;

namespace backend.api.oura.intr;

public interface IOuraService
{
    Task<ApiResponse<PersonalInfo>?> GetPersonalInfoAsync();
    Task<ApiResponse<DailySleep>?> GetDailySleepAsync(string? startDate = null, string? endDate = null);
    Task<ApiResponse<Sleep>?> GetSleepAsync(string? startDate = null, string? endDate = null);
    Task<ApiResponse<DailyActivity>?> GetDailyActivityAsync(string? startDate = null, string? endDate = null);
    Task<ApiResponse<DailyReadiness>?> GetDailyReadinessAsync(string? startDate = null, string? endDate = null);
    Task<ApiResponse<HeartRate>?> GetHeartRateAsync(string? startDate = null, string? endDate = null);
    Task<ApiResponse<Workout>?> GetWorkoutsAsync(string? startDate = null, string? endDate = null);
    Task<ApiResponse<Session>?> GetSessionsAsync(string? startDate = null, string? endDate = null);
    Task<ApiResponse<Tag>?> GetTagsAsync(string? startDate = null, string? endDate = null);
    Task<ApiResponse<DailyStress>?> GetDailyStressAsync(string? startDate = null, string? endDate = null);
    Task<ApiResponse<RestModePeriod>?> GetRestModePeriodsAsync(string? startDate = null, string? endDate = null);
    Task<ApiResponse<RingConfiguration>?> GetRingConfigurationAsync();
    Task<ApiResponse<DailySpo2>?> GetDailySpo2Async(string? startDate = null, string? endDate = null);
    Task<ApiResponse<DailyResilience>?> GetDailyResilienceAsync(string? startDate = null, string? endDate = null);
    Task<ApiResponse<Vo2Max>?> GetVo2MaxAsync(string? startDate = null, string? endDate = null);
}
