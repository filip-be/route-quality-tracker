using Microsoft.VisualStudio.Threading;

namespace RouteQualityTracker.Core.Interfaces;

public interface IActivitiesIntegrationService
{
    void AuthenticateViaStrava(string clientId);

    event EventHandler<string>? OnAuthenticateViaStrava;

    void NotifyStravaAuthenticationHasCompleted();

    event AsyncEventHandler? OnStravaAuthenticationCompleted;
}
