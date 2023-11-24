using RouteQualityTracker.Interfaces;

namespace RouteQualityTracker.Services;

public class ForegroundService : IForegroundService
{
    private bool IsServiceRunning { get; set; }
        
    public bool ToggleService()
    {
        if (IsServiceRunning)
        {
            IsServiceRunning = false;
            OnServiceStop?.Invoke(this, null!);
        }
        else
        {
            IsServiceRunning = true;
            OnServiceStart?.Invoke(this, null!);
        }

        return IsServiceRunning;
    }

    public event EventHandler? OnServiceStart;
    public event EventHandler? OnServiceStop;
    public event EventHandler? OnServiceStartError;
}