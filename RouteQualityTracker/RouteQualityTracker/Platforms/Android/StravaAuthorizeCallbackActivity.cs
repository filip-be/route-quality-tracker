using Android.App;
using Android.Content;
using Android.OS;
using RouteQualityTracker.Core.Services;
using RouteQualityTracker.Services;


namespace RouteQualityTracker.Platforms.Android;

[Activity(Label = "Callback", Exported = true, MainLauncher = true)]
[IntentFilter([Intent.ActionView], Categories = [Intent.CategoryDefault, Intent.CategoryBrowsable],
    DataScheme = "https", DataHost = "route-quality-tracker-app.com", DataPath = "/StravaAuthorize")]
public class StravaAuthorizeCallbackActivity : Activity
{
    private readonly ISettingsService _settingsService = ServiceHelper.Services.GetService<ISettingsService>()!;

    public const int ActivityRequestCode = 100;

    protected override void OnResume()
    {
        Console.WriteLine(@"StravaAuthorizeCallbackActivity started");
        base.OnResume();
        
        if(Intent?.Data is null) return;

        try
        {
            var requestCode = Intent.Data.GetQueryParameter("code");
            Console.WriteLine("Request code received");

            _settingsService.UpdateStravaApiCode(requestCode);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error occurred: {ex}");
            _settingsService.UpdateStravaApiCode(null);
        }
        finally
        {
            Finish();
        }
        
    }
}