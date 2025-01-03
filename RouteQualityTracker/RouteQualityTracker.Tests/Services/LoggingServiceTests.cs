using Moq;
using NUnit.Framework;
using RouteQualityTracker.Core.Services;

namespace RouteQualityTracker.Tests.Services;

[TestFixture]
public class LoggingServiceTests
{
    [Test]
    public void LogDebugMessage_WhenCalled_RaiseOnLogDebugMessageEvent()
    {
        // Arrange
        const string message = "Test message";
        string? raisedMessage = null;

        var date = new DateTimeOffset(2021, 1, 1, 15, 0, 7, TimeSpan.Zero);
        var timePoviderFake = new Mock<TimeProvider>();
        timePoviderFake.Setup(x => x.LocalTimeZone).Returns(TimeZoneInfo.Utc);
        timePoviderFake.Setup(x => x.GetUtcNow()).Returns(date);

        var loggingService = new LoggingService(timePoviderFake.Object);
        loggingService.OnLogDebugMessage += (sender, message) => raisedMessage = message;

        // Act
        loggingService.LogDebugMessage(message);

        // Assert
        Assert.That(raisedMessage, Is.EqualTo($"15:00:07 {message}"));
    }
}
