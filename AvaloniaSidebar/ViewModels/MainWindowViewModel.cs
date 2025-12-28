using System;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using backend.api.oura.intr;

namespace AvaloniaSidebar.ViewModels;

public class MainWindowViewModel : ViewModelBase, IDisposable
{
    private readonly IOuraService _ouraService;
    private readonly Timer _pollingTimer;
    private const int PollingIntervalSeconds = 30;
    
    private string _steps = "--";
    private string _stepGoal = "Goal: --";
    private double _stepsProgress = 0;
    private double _stepsProgressWidth = 0;
    private string _heartRateBpm = "--";
    private string _heartRateTimestamp = "";
    private string _heartRateColor = "#888888";
    private string _stressLevel = "--";
    private string _stressColor = "#888888";
    private bool _isLoading;
    private string? _errorMessage;

    public MainWindowViewModel(IOuraService ouraService)
    {
        _ouraService = ouraService ?? throw new ArgumentNullException(nameof(ouraService));
        
        _pollingTimer = new Timer(_ => 
        {
            _ = Task.Run(async () => await LoadAllDataAsync());
        }, null, TimeSpan.Zero, TimeSpan.FromSeconds(PollingIntervalSeconds));
    }

    public string Steps
    {
        get => _steps;
        private set => SetProperty(ref _steps, value);
    }

    public string StepGoal
    {
        get => _stepGoal;
        private set => SetProperty(ref _stepGoal, value);
    }

    public double StepsProgress
    {
        get => _stepsProgress;
        private set
        {
            if (SetProperty(ref _stepsProgress, value))
            {
                StepsProgressWidth = (value / 100.0) * 198;
            }
        }
    }

    public double StepsProgressWidth
    {
        get => _stepsProgressWidth;
        private set => SetProperty(ref _stepsProgressWidth, value);
    }

    public string HeartRateBpm
    {
        get => _heartRateBpm;
        private set => SetProperty(ref _heartRateBpm, value);
    }

    public string HeartRateTimestamp
    {
        get => _heartRateTimestamp;
        private set => SetProperty(ref _heartRateTimestamp, value);
    }

    public string HeartRateColor
    {
        get => _heartRateColor;
        private set => SetProperty(ref _heartRateColor, value);
    }

    public string StressLevel
    {
        get => _stressLevel;
        private set => SetProperty(ref _stressLevel, value);
    }

    public string StressColor
    {
        get => _stressColor;
        private set => SetProperty(ref _stressColor, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        private set => SetProperty(ref _isLoading, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public async Task LoadAllDataAsync()
    {
        Console.WriteLine("Loading all data...");
        if (IsLoading)
            return;

        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var heartRateTask = LoadHeartRateDataAsync();
            var activityTask = LoadActivityDataAsync();
            var stressTask = LoadStressDataAsync();
            
            await Task.WhenAll(heartRateTask, activityTask, stressTask);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            Console.WriteLine($"Error loading data: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadHeartRateDataAsync()
    {
        try
        {
            var heartRateResponse = await _ouraService.GetHeartRateAsync();
            
            if (heartRateResponse?.Data != null && heartRateResponse.Data.Count > 0)
            {
                var validReadings = heartRateResponse.Data
                    .Where(hr => hr.Bpm.HasValue)
                    .Select(hr => hr.Bpm!.Value)
                    .ToList();
                
                if (validReadings.Count > 0)
                {
                    var averageBpm = (int)Math.Round(validReadings.Average());
                    HeartRateBpm = $"{averageBpm} bpm";
                    HeartRateTimestamp = "24h avg";
                    
                    // Set color based on resting heart rate range (40-100 bpm = green, otherwise orange)
                    if (averageBpm >= 40 && averageBpm <= 80)
                    {
                        HeartRateColor = "#4CAF50"; // Green
                    }
                    else
                    {
                        HeartRateColor = "#FF9800"; // Orange
                    }
                }
                else
                {
                    HeartRateBpm = "--";
                    HeartRateTimestamp = "";
                    HeartRateColor = "#888888"; // Gray for no data
                }
            }
            else
            {
                HeartRateBpm = "--";
                HeartRateTimestamp = "";
                HeartRateColor = "#888888"; // Gray for no data
            }
        }
        catch (Exception ex)
        {
            HeartRateBpm = "--";
            HeartRateTimestamp = "";
            HeartRateColor = "#888888"; // Gray for error
            Console.WriteLine($"Error loading heart rate: {ex.Message}");
        }
    }

    private async Task LoadActivityDataAsync()
    {
        try
        {
            var activityResponse = await _ouraService.GetDailyActivityAsync();
            
            if (activityResponse?.Data != null && activityResponse.Data.Count > 0)
            {
                var todayActivity = activityResponse.Data.First();
                var steps = todayActivity.Steps ?? 0;
                var stepGoal = todayActivity.TargetSteps ??
                              (todayActivity.TargetMeters.HasValue 
                                  ? (int)(todayActivity.TargetMeters.Value / 0.762) 
                                  : 10000);

                Steps = steps.ToString("N0");
                StepGoal = $"{steps:N0} / {stepGoal:N0} steps";
                
                if (stepGoal > 0)
                {
                    StepsProgress = Math.Min(100, (steps / (double)stepGoal) * 100);
                }
                else
                {
                    StepsProgress = 0;
                }
            }
            else
            {
                Steps = "0";
                StepGoal = "0 / -- steps";
                StepsProgress = 0;
            }
        }
        catch (Exception ex)
        {
            Steps = "--";
            StepGoal = "--";
            StepsProgress = 0;
            Console.WriteLine($"Error loading activity: {ex.Message}");
        }
    }

    private async Task LoadStressDataAsync()
    {
        try
        {
            var stressResponse = await _ouraService.GetDailyStressAsync();
            
            if (stressResponse?.Data != null && stressResponse.Data.Count > 0)
            {
                var todayStress = stressResponse.Data.First();
                var stressHigh = todayStress.StressHigh ?? 0;
                var stressMedium = todayStress.StressMedium ?? 0;
                var stressLow = todayStress.StressLow ?? 0;
                
                if (stressHigh > stressMedium && stressHigh > stressLow)
                {
                    StressLevel = "High";
                    StressColor = "#F44336";
                }
                else if (stressMedium > stressLow)
                {
                    StressLevel = "Medium";
                    StressColor = "#FF9800";
                }
                else
                {
                    StressLevel = "Low";
                    StressColor = "#4CAF50";
                }
            }
            else
            {
                StressLevel = "--";
                StressColor = "#888888";
            }
        }
        catch (Exception ex)
        {
            StressLevel = "--";
            StressColor = "#888888";
            Console.WriteLine($"Error loading stress: {ex.Message}");
        }
    }

    private string FormatRelativeTime(string timestamp)
    {
        try
        {
            if (DateTimeOffset.TryParse(timestamp, out var timeOffset))
            {
                var now = DateTimeOffset.UtcNow;
                var diff = now - timeOffset;
                Console.WriteLine(diff);
                if (diff.TotalMinutes < 1)
                    return "Just now";
                else if (diff.TotalMinutes < 60)
                    return $"{(int)diff.TotalMinutes} min ago";
                else if (diff.TotalHours < 24)
                    return $"{(int)diff.TotalHours} hr ago";
                else
                    return $"{(int)diff.TotalDays} day ago";
            }
        }
        catch
        {
        }
        
        return "";
    }

    public void Dispose()
    {
        _pollingTimer?.Dispose();
    }
}
