namespace RouteQualityTracker.Core.Interfaces;

public interface IActivitiesIntegrationService
{
    void AuthenticateViaStrava(string clientId);

    event EventHandler<string>? OnAuthenticateViaStrava;
}
