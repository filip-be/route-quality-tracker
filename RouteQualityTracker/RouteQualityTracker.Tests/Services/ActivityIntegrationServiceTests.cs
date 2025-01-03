using FluentAssertions;
using Moq;
using NUnit.Framework;
using RouteQualityTracker.Core.Interfaces;
using RouteQualityTracker.Core.Services;

namespace RouteQualityTracker.Tests.Services;

[TestFixture]
public class ActivityIntegrationServiceTests
{
    private IActivityIntegrationService _service;

    [SetUp]
    public void SetUp()
    {
        var fakeSettingsService = new Mock<ISettingsService>();
        var fakeClientFactory = new Mock<IHttpClientFactory>();
        _service = new ActivityIntegrationService(fakeSettingsService.Object, fakeClientFactory.Object);
    }

    [Test]
    public void AuthenticateViaStrava_Triggers_AuthenticateViaStravaEvent()
    {
        const string clientId = "123";
        string actualClientId = null;

        _service.OnAuthenticateViaStrava += (_, eventClientId) =>
        {
            actualClientId = eventClientId;
        };

        _service.AuthenticateViaStrava(clientId);

        actualClientId.Should().Be(clientId);
    }

}
