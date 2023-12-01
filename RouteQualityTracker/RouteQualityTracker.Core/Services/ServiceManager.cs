using RouteQualityTracker.Core.Interfaces;

namespace RouteQualityTracker.Core.Services;

public class ServiceManager : IServiceManager
{
    private bool IsServiceRunning { get; set; }

    public void ToggleService()
    {
        if (IsServiceRunning)
        {
            OnServiceStop?.Invoke(this, null!);
        }
        else
        {
            OnServiceStart?.Invoke(this, null!);
        }
    }

    public void SetStatus(bool isRunning, Exception? ex = null)
    {
        IsServiceRunning = isRunning;

        if (IsServiceRunning)
        {
            OnServiceStarted?.Invoke(this, null!);
        }
        else
        {
            OnServiceStopped?.Invoke(this, null!);
            if (ex != null)
            {
                OnServiceStartError?.Invoke(this, ex);
            }
        }
    }

    public event EventHandler? OnServiceStart;
    public event EventHandler? OnServiceStop;
    public event EventHandler? OnServiceStarted;
    public event EventHandler? OnServiceStopped;
    public event EventHandler<Exception>? OnServiceStartError;
}