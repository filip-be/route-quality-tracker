namespace RouteQualityTracker.Core.Interfaces;

public interface ILoggingService
{
    void LogDebugMessage(string message);

    event EventHandler<string>? OnLogDebugMessage;
}
