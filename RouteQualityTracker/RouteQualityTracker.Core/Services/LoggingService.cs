using Microsoft.Extensions.Logging;
using RouteQualityTracker.Core.Interfaces;

namespace RouteQualityTracker.Core.Services;

public class LoggingService : ILoggingService
{
    private readonly TimeProvider _timeProvider;
    private readonly ISettingsService _settingsService;

    public LoggingService(TimeProvider timeProvider, ISettingsService settingsService)
    {
        _timeProvider = timeProvider;
        _settingsService = settingsService;
    }

    public event EventHandler<string>? OnLogDebugMessage;

    public void LogMessage(LogLevel logLevel, string message)
    {
        if (logLevel < _settingsService.Settings.LogLevel)
        {
            return;
        }

        var currentTime = _timeProvider.GetLocalNow().TimeOfDay;
        OnLogDebugMessage?.Invoke(this, $"{currentTime:hh\\:mm\\:ss} {message}");
    }
}
