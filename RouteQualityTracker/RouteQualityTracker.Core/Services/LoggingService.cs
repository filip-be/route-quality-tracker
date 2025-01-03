using RouteQualityTracker.Core.Interfaces;

namespace RouteQualityTracker.Core.Services;

public class LoggingService : ILoggingService
{
    private readonly TimeProvider _timeProvider;

    public LoggingService(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public event EventHandler<string>? OnLogDebugMessage;

    public void LogDebugMessage(string message)
    {
        var currentTime = _timeProvider.GetLocalNow().TimeOfDay;

        OnLogDebugMessage?.Invoke(this, $"{currentTime:hh\\:mm\\:ss} {message}");
    }
}
