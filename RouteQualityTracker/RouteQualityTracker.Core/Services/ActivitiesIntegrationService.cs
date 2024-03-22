using RouteQualityTracker.Core.Interfaces;

namespace RouteQualityTracker.Core.Services;

public class ActivitiesIntegrationService : IActivitiesIntegrationService
{
    public void AuthenticateViaStrava(string clientId)
    {
        OnAuthenticateViaStrava?.Invoke(this, clientId);
    }

    public event EventHandler<string>? OnAuthenticateViaStrava;
}