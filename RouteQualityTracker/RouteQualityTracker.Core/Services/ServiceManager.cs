using RouteQualityTracker.Core.Interfaces;

namespace RouteQualityTracker.Core.Services;

public class ServiceManager : IServiceManager
{
    private readonly ILoggingService _loggingService;

    public bool IsRunning { get; internal set; }

    public ServiceManager(ILoggingService loggingService)
    {
        _loggingService = loggingService;
    }

    public void ToggleService()
    {
        if (IsRunning)
        {
            _loggingService.Debug("Stopping service...");
            OnServiceStop?.Invoke(this, null!);
        }
        else
        {
            _loggingService.Debug("Starting service...");
            OnServiceStart?.Invoke(this, null!);
        }
    }

    public void SetStatus(bool isRunning, Exception? ex = null)
    {
        IsRunning = isRunning;

        if (IsRunning)
        {
            _loggingService.Debug("Service is running");
            OnServiceStarted?.Invoke(this, null!);
        }
        else
        {
            _loggingService.Debug("Service stopped");
            OnServiceStopped?.Invoke(this, null!);
            if (ex != null)
            {
                _loggingService.Debug($"Error occured: {ex.Message}");
                OnServiceStartError?.Invoke(this, ex);
            }
        }
    }

    public void DisplayMessage(string message)
    {
        _loggingService.Trace(message);
        OnDisplayMessage?.Invoke(this, message);
    }

    public event EventHandler? OnServiceStart;
    public event EventHandler? OnServiceStop;
    public event EventHandler? OnServiceStarted;
    public event EventHandler? OnServiceStopped;
    public event EventHandler<Exception>? OnServiceStartError;
    public event EventHandler<string>? OnDisplayMessage;
}