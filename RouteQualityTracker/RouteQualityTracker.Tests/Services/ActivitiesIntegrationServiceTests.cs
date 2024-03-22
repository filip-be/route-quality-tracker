using FluentAssertions;
using NUnit.Framework;
using RouteQualityTracker.Core.Interfaces;
using RouteQualityTracker.Core.Services;

namespace RouteQualityTracker.Tests.Services;

[TestFixture]
public class ActivitiesIntegrationServiceTests
{
    private IActivitiesIntegrationService _service;

    [SetUp]
    public void SetUp()
    {
        _service = new ActivitiesIntegrationService();
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
