namespace RouteQualityTracker.Core.Interfaces;

public interface IServiceManager
{
    void ToggleService();

    void SetStatus(bool isRunning, Exception? ex = null);

    event EventHandler OnServiceStart;

    event EventHandler OnServiceStarted;

    event EventHandler OnServiceStop;

    event EventHandler OnServiceStopped;

    event EventHandler<Exception> OnServiceStartError;
}