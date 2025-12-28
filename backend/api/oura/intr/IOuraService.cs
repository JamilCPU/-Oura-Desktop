using System.Threading.Tasks;
using backend.api.oura.models;

namespace backend.api.oura.intr;

public interface IOuraService
{
    Task<ApiResponse<PersonalInfo>?> GetPersonalInfoAsync();
    Task<ApiResponse<DailySleep>?> GetDailySleepAsync();
    Task<ApiResponse<Sleep>?> GetSleepAsync();
    Task<ApiResponse<DailyActivity>?> GetDailyActivityAsync();
    Task<ApiResponse<DailyReadiness>?> GetDailyReadinessAsync();
    Task<ApiResponse<HeartRate>?> GetHeartRateAsync();
    Task<ApiResponse<Workout>?> GetWorkoutsAsync();
    Task<ApiResponse<Session>?> GetSessionsAsync();
    Task<ApiResponse<Tag>?> GetTagsAsync();
    Task<ApiResponse<DailyStress>?> GetDailyStressAsync();
    Task<ApiResponse<RestModePeriod>?> GetRestModePeriodsAsync();
    Task<ApiResponse<RingConfiguration>?> GetRingConfigurationAsync();
    Task<ApiResponse<DailySpo2>?> GetDailySpo2Async();
    Task<ApiResponse<DailyResilience>?> GetDailyResilienceAsync();
    Task<ApiResponse<Vo2Max>?> GetVo2MaxAsync();
}
