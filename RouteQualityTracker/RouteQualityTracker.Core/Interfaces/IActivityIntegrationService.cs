using Microsoft.VisualStudio.Threading;

namespace RouteQualityTracker.Core.Interfaces;

public interface IActivityIntegrationService
{
    void AuthenticateViaStrava(string clientId);

    event EventHandler<string>? OnAuthenticateViaStrava;

    void NotifyStravaAuthenticationHasCompleted();

    event AsyncEventHandler? OnStravaAuthenticationCompleted;

    Task<bool> HasRequiredAccess();
}
