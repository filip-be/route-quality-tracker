using Android.App;
using Android.Content;
using Android.Content.PM;
using RouteQualityTracker.Core.Interfaces;
using RouteQualityTracker.Core.Services;
using RouteQualityTracker.Platforms.Android;

namespace RouteQualityTracker;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    private readonly IServiceManager _serviceManager;

    public MainActivity()
    {
        _serviceManager = ServiceHelper.Services.GetService<IServiceManager>()!;
        _serviceManager.OnServiceStart += OnServiceStart;
        _serviceManager.OnServiceStop += OnServiceStop;
    }

    private void OnServiceStop(object? sender, EventArgs e)
    {
        var intent = new Intent(this, typeof(MediaButtonHandlerHandlerService));

        try
        {
            StopService(intent);
            _serviceManager.SetStatus(false);
        }
        catch (Exception ex)
        {
            _serviceManager.SetStatus(false, ex);
        }
    }

    private void OnServiceStart(object? sender, EventArgs e)
    {
        var intent = new Intent(this, typeof(MediaButtonHandlerHandlerService));

        try
        {
            StartForegroundService(intent);
        }
        catch(Exception ex)
        {
            _serviceManager.SetStatus(false, ex);
        }
    }
}
