using Microsoft.VisualStudio.Threading;
using RouteQualityTracker.Core.Interfaces;

namespace RouteQualityTracker.Core.Services;

public class ActivityIntegrationService(ISettingsService settingsService, IHttpClientFactory httpClientFactory) : IActivityIntegrationService
{
    public const string StravaHttpClient = "StravaClient";

    public void AuthenticateViaStrava(string clientId)
    {
        OnAuthenticateViaStrava?.Invoke(this, clientId);
    }

    public event EventHandler<string>? OnAuthenticateViaStrava;

    public void NotifyStravaAuthenticationHasCompleted()
    {
        _ = OnStravaAuthenticationCompleted?.InvokeAsync(this, EventArgs.Empty);
    }

    public event AsyncEventHandler? OnStravaAuthenticationCompleted;

    public Task<bool> HasRequiredAccess()
    {
        if (string.IsNullOrEmpty(settingsService.Settings.StravaApiCode))
        {
            return Task.FromResult(false);
        }

        _ = httpClientFactory.CreateClient(StravaHttpClient);

        return Task.FromResult(false);
    }
}