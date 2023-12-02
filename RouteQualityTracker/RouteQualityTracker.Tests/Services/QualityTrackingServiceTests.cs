using FluentAssertions;
using Moq;
using NUnit.Framework;
using RouteQualityTracker.Core.Interfaces;
using RouteQualityTracker.Core.Models;
using RouteQualityTracker.Core.Services;

namespace RouteQualityTracker.Tests.Services;


[TestFixture]
public class QualityTrackingServiceTests
{
    private Mock<TimeProvider> _timePoviderFake;
    private Mock<INotificationService> _notificationServiceFake;

    private IQualityTrackingService GetService => 
        new QualityTrackingService(_timePoviderFake.Object, _notificationServiceFake.Object);

    [SetUp]
    public void SetUp()
    {
        _notificationServiceFake = new Mock<INotificationService>();
        _timePoviderFake = new Mock<TimeProvider>();
        SetupCurrentDate(new DateTimeOffset());
    }

    private void SetupCurrentDate(DateTimeOffset date)
    {
        _timePoviderFake.Reset();
        _timePoviderFake.Setup(x => x.GetUtcNow()).Returns(date);
    }

    [Test]
    public void DefaultRouteQuality_IsUnknown()
    {
        var routeQuality = GetService.GetCurrentRouteQuality();

        routeQuality.Should().Be(RouteQualityEnum.Unknown);
    }

    [Test]
    public void RouteQuality_AfterStart_IsStandard()
    {
        var service = GetService;

        service.StartTracking();
        var routeQuality = service.GetCurrentRouteQuality();

        routeQuality.Should().Be(RouteQualityEnum.Standard);
    }

    [Test]
    public void Start_TriggersQualityChange_Event()
    {
        var service = GetService;
        RouteQualityEnum? routeQuality = null;

        service.OnRouteQualityChanged += (_, quality) =>
        {
            routeQuality = quality;
        };

        service.StartTracking();

        routeQuality.Should().Be(RouteQualityEnum.Standard);
    }

    [Test]
    public void Start_Saves_CurrentDate_ToEvents()
    {
        var expectedDate = new DateTimeOffset(2023, 12, 01, 00, 00, 00, TimeSpan.Zero);
        SetupCurrentDate(expectedDate);

        var service = GetService;

        service.StartTracking();

        var records = service.GetRouteQualityRecords();

        records.Single().Date.Should().Be(expectedDate);
    }

    [Test]
    public void RouteQuality_AfterStop_IsUnknown()
    {
        var service = GetService;

        service.StartTracking();
        service.StopTracking();

        var routeQuality = service.GetCurrentRouteQuality();

        routeQuality.Should().Be(RouteQualityEnum.Unknown);
    }

    [Test]
    public void Toggle_IteratesOverQuality()
    {
        var service = GetService;

        service.StartTracking();
        service.GetCurrentRouteQuality().Should()
            .Be(RouteQualityEnum.Standard, "because starting quality should be Standard");

        service.ToggleRouteQuality();
        service.GetCurrentRouteQuality().Should()
            .Be(RouteQualityEnum.Good, "because after Standard it should be Good");

        service.ToggleRouteQuality();
        service.GetCurrentRouteQuality().Should()
            .Be(RouteQualityEnum.Standard, "because after Good it should be Standard again");

        service.ToggleRouteQuality();
        service.GetCurrentRouteQuality().Should()
            .Be(RouteQualityEnum.Bad, "because after Standard it should be Bad when decreasing quality");

        service.ToggleRouteQuality();
        service.GetCurrentRouteQuality().Should()
            .Be(RouteQualityEnum.Standard, "because after Bad it should be Standard again");
    }

    [Test]
    public void Toggle_TriggersEvent()
    {
        var service = GetService;
        RouteQualityEnum? routeQuality = null;

        service.OnRouteQualityChanged += (_, quality) =>
        {
            routeQuality = quality;
        };

        service.StartTracking();
        service.ToggleRouteQuality();

        routeQuality.Should().NotBe(RouteQualityEnum.Unknown);
        routeQuality.Should().NotBe(RouteQualityEnum.Standard);
    }

    [Test]
    public void Toggle_AddsRouteQualityRecord()
    {
        var service = GetService;

        service.StartTracking();
        service.ToggleRouteQuality();

        service.GetRouteQualityRecords().Count.Should().Be(2);
    }

    [Test]
    public void QualityChange_DoNot_SendEmail_IfNotificationDisabled()
    {
        _notificationServiceFake.SetupGet(x => x.IsSendEmailEnabled).Returns(false);

        var service = GetService;
        service.StartTracking();

        _notificationServiceFake.Verify(x => x.SendEmail(It.IsAny<RouteQualityEnum>()), Times.Never());
    }

    [Test]
    public void QualityChange_SendEmail_IfNotificationEnabled()
    {
        _notificationServiceFake.SetupGet(x => x.IsSendEmailEnabled).Returns(true);

        var service = GetService;
        service.StartTracking();

        _notificationServiceFake.Verify(x => x.SendEmail(It.IsAny<RouteQualityEnum>()), Times.Once());
    }
}
