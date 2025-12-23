using System;
using System.Linq;
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
    private bool _isLoading;
    private string? _errorMessage;

    public MainWindowViewModel(IOuraService ouraService)
    {
        _ouraService = ouraService ?? throw new ArgumentNullException(nameof(ouraService));
        
        _pollingTimer = new Timer(_ => 
        {
            _ = Task.Run(async () => await LoadActivityDataAsync());
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

    public async Task LoadActivityDataAsync()
    {
        Console.WriteLine("Loading activity...");
        if (IsLoading)
            return;

        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
            var activityResponse = await _ouraService.GetDailyActivityAsync(today, today);
            Console.Write(activityResponse);
            if (activityResponse?.Data != null && activityResponse.Data.Count > 0)
            {
                var todayActivity = activityResponse.Data.First();
                var steps = todayActivity.Steps ?? 0;
                var stepGoal = todayActivity.TargetSteps ??
                              (todayActivity.TargetMeters.HasValue 
                                  ? (int)(todayActivity.TargetMeters.Value / 0.762) 
                                  : 10000);

                Steps = steps.ToString("N0");
                StepGoal = $"Goal: {stepGoal:N0}";
            }
            else
            {
                Steps = "0";
                StepGoal = "Goal: --";
            }
        }
        catch (Exception ex)
        {
            Steps = "Error";
            StepGoal = ex.Message;
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void Dispose()
    {
        _pollingTimer?.Dispose();
    }
}
