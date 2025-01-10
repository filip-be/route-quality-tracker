using Microsoft.Extensions.Logging;

namespace RouteQualityTracker.Core.Interfaces;

public interface ILoggingService
{
    void LogMessage(LogLevel logLevel, string message);

    void Trace(string message) => LogMessage(LogLevel.Trace, message);
    void Debug(string message) => LogMessage(LogLevel.Debug, message);
    void Info(string message) => LogMessage(LogLevel.Information, message);
    void Error(string message) => LogMessage(LogLevel.Error, message);

    event EventHandler<string>? OnLogDebugMessage;
}
