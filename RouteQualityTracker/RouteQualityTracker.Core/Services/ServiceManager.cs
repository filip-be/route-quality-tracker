using RouteQualityTracker.Core.Interfaces;

namespace RouteQualityTracker.Core.Services;

public class ServiceManager : IServiceManager
{
    public bool IsRunning { get; internal set; }

    public void ToggleService()
    {
        if (IsRunning)
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
        IsRunning = isRunning;

        if (IsRunning)
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

    public void DisplayMessage(string message)
    {
        OnDisplayMessage?.Invoke(this, message);
    }

    public event EventHandler? OnServiceStart;
    public event EventHandler? OnServiceStop;
    public event EventHandler? OnServiceStarted;
    public event EventHandler? OnServiceStopped;
    public event EventHandler<Exception>? OnServiceStartError;
    public event EventHandler<string>? OnDisplayMessage;
}