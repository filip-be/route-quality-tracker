using System.Web;
using Microsoft.VisualStudio.Threading;
using RouteQualityTracker.Core.Interfaces;
using RouteQualityTracker.Core.Models;

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

    public async Task AuthorizeToStrava(string? requestCode)
    {
        if (string.IsNullOrEmpty(requestCode))
        {
            return;
        }

        var client = httpClientFactory.CreateClient(StravaHttpClient);

        var queryBuilder = HttpUtility.ParseQueryString(string.Empty);
        queryBuilder.Add("client_id", AppSettings.StravaClientId);
        queryBuilder.Add("client_secret", "");
        queryBuilder.Add("code", requestCode);
        queryBuilder.Add("grant_type", "authorization_code");

        var uriBuilder = new UriBuilder(new Uri("https://strava.com/oauth/token"));
        uriBuilder.Query = queryBuilder.ToString();

        var response = await client.PostAsync(uriBuilder.Uri, null);
        Console.WriteLine($"Request: {uriBuilder.Uri}");
        Console.WriteLine($"Response: {response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
    }

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