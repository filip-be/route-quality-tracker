namespace RouteQualityTracker.Core.Interfaces;

public interface IServiceManager
{
    void ToggleService();

    bool IsRunning { get; }

    void SetStatus(bool isRunning, Exception? ex = null);

    void DisplayMessage(string message);

    event EventHandler OnServiceStart;

    event EventHandler OnServiceStarted;

    event EventHandler OnServiceStop;

    event EventHandler OnServiceStopped;

    event EventHandler<Exception> OnServiceStartError;

    event EventHandler<string>? OnDisplayMessage;
}