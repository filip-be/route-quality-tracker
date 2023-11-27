namespace RouteQualityTracker.Interfaces;

public interface IServiceManager
{
    bool ToggleService();

    event EventHandler OnServiceStart;

    event EventHandler OnServiceStop;

    event EventHandler OnServiceStartError;
}