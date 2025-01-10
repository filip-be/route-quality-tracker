using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RouteQualityTracker.Core.Interfaces;
using RouteQualityTracker.Core.Models;
using RouteQualityTracker.Core.Services;

namespace RouteQualityTracker.Tests.Services;

[TestFixture]
public class LoggingServiceTests
{
    private readonly DateTime _currentTime = new DateTime(2021, 1, 1, 15, 0, 7);
    private readonly string _expectedDateString = "15:00:07";
    private readonly AppSettings _fakeSettings = new();

    private Mock<TimeProvider> _timeProviderFake = null!;
    private Mock<ISettingsService> _settingsServiceFake = null!;

    [SetUp]
    public void SetUp()
    {
        _timeProviderFake = new Mock<TimeProvider>();
        _timeProviderFake.Setup(x => x.LocalTimeZone).Returns(TimeZoneInfo.Utc);
        _timeProviderFake.Setup(x => x.GetUtcNow()).Returns(_currentTime);

        _settingsServiceFake = new Mock<ISettingsService>();
        _settingsServiceFake.Setup(x => x.Settings).Returns(_fakeSettings);
    }

    [TestCase(LogLevel.Debug, LogLevel.None, false)]
    [TestCase(LogLevel.Debug, LogLevel.Trace, true)]
    [TestCase(LogLevel.Debug, LogLevel.Debug, true)]
    [TestCase(LogLevel.Debug, LogLevel.Information, false)]
    [TestCase(LogLevel.Information, LogLevel.None, false)]
    [TestCase(LogLevel.Information, LogLevel.Trace, true)]
    [TestCase(LogLevel.Information, LogLevel.Debug, true)]
    [TestCase(LogLevel.Information, LogLevel.Information, true)]
    [TestCase(LogLevel.Information, LogLevel.Warning, false)]
    [TestCase(LogLevel.Warning, LogLevel.None, false)]
    [TestCase(LogLevel.Warning, LogLevel.Trace, true)]
    [TestCase(LogLevel.Warning, LogLevel.Debug, true)]
    [TestCase(LogLevel.Warning, LogLevel.Information, true)]
    [TestCase(LogLevel.Warning, LogLevel.Warning, true)]
    [TestCase(LogLevel.Warning, LogLevel.Error, false)]
    [TestCase(LogLevel.Error, LogLevel.None, false)]
    [TestCase(LogLevel.Error, LogLevel.Trace, true)]
    [TestCase(LogLevel.Error, LogLevel.Debug, true)]
    [TestCase(LogLevel.Error, LogLevel.Information, true)]
    [TestCase(LogLevel.Error, LogLevel.Warning, true)]
    [TestCase(LogLevel.Error, LogLevel.Error, true)]
    public void LogMessage_ShouldLogMessage_IfLogLevelSettingsAllowsThis(
        LogLevel messageLogLevel, LogLevel settingsLogLevel, bool messageShouldBeLogged)
    {
        // Arrange
        const string message = "Debug message";
        string? raisedMessage = null;

        _fakeSettings.LogLevel = settingsLogLevel;

        var loggingService = new LoggingService(_timeProviderFake.Object, _settingsServiceFake.Object);
        loggingService.OnLogDebugMessage += (sender, msg) => raisedMessage = msg;

        // Act
        loggingService.LogMessage(messageLogLevel, message);

        // Assert
        if (messageShouldBeLogged)
        {
            Assert.That(raisedMessage, Is.EqualTo($"{_expectedDateString} {message}"));
        }
        else
        {
            Assert.That(raisedMessage, Is.Null);
        }
    }

    [Test]
    public void Trace_ShouldLogMessage()
    {
        // Arrange
        const string message = "Debug message";
        string? raisedMessage = null;

        _fakeSettings.LogLevel = LogLevel.Trace;

        ILoggingService loggingService = new LoggingService(_timeProviderFake.Object, _settingsServiceFake.Object);
        loggingService.OnLogDebugMessage += (sender, msg) => raisedMessage = msg;

        // Act
        loggingService.Trace(message);

        // Assert
        Assert.That(raisedMessage, Is.EqualTo($"{_expectedDateString} {message}"));
    }

    [Test]
    public void Debug_ShouldLogMessage()
    {
        // Arrange
        const string message = "Debug message";
        string? raisedMessage = null;

        _fakeSettings.LogLevel = LogLevel.Debug;

        ILoggingService loggingService = new LoggingService(_timeProviderFake.Object, _settingsServiceFake.Object);
        loggingService.OnLogDebugMessage += (sender, msg) => raisedMessage = msg;

        // Act
        loggingService.Debug(message);

        // Assert
        Assert.That(raisedMessage, Is.EqualTo($"{_expectedDateString} {message}"));
    }

    [Test]
    public void Info_ShouldLogMessage()
    {
        // Arrange
        const string message = "Info message";
        string? raisedMessage = null;

        _fakeSettings.LogLevel = LogLevel.Information;

        ILoggingService loggingService = new LoggingService(_timeProviderFake.Object, _settingsServiceFake.Object);
        loggingService.OnLogDebugMessage += (sender, msg) => raisedMessage = msg;

        // Act
        loggingService.Info(message);

        // Assert
        Assert.That(raisedMessage, Is.EqualTo($"{_expectedDateString} {message}"));
    }

    [Test]
    public void Error_ShouldLogMessage()
    {
        // Arrange
        const string message = "Error message";
        string? raisedMessage = null;

        _fakeSettings.LogLevel = LogLevel.Error;

        ILoggingService loggingService = new LoggingService(_timeProviderFake.Object, _settingsServiceFake.Object);
        loggingService.OnLogDebugMessage += (sender, msg) => raisedMessage = msg;

        // Act
        loggingService.Error(message);

        // Assert
        Assert.That(raisedMessage, Is.EqualTo($"{_expectedDateString} {message}"));
    }
}
