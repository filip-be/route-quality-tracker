using Android.App;
using Android.Content;
using Android.Content.PM;
using RouteQualityTracker.Core.Interfaces;
using RouteQualityTracker.Core.Services;
using RouteQualityTracker.Platforms.Android;
using RouteQualityTracker.Services;

namespace RouteQualityTracker;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    private readonly IServiceManager _serviceManager;
    private readonly ISettingsService _settingsService;

    public MainActivity()
    {
        _serviceManager = ServiceHelper.Services.GetService<IServiceManager>()!;
        _settingsService = ServiceHelper.Services.GetService<ISettingsService>()!;
        _serviceManager.OnServiceStart += OnServiceStart;
        _serviceManager.OnServiceStop += OnServiceStop;
    }

    private void OnServiceStop(object? sender, EventArgs e)
    {
        try
        {
            var intent = GetServiceIntent();
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
        try
        {
            var intent = GetServiceIntent();
            StartForegroundService(intent);
        }
        catch(Exception ex)
        {
            _serviceManager.SetStatus(false, ex);
        }
    }

    private Intent GetServiceIntent()
    {
        if(_settingsService.Settings.UseCustomDevice)
        {
            return new Intent(this, typeof(BlePositioningDeviceService));
        }

        if(_settingsService.Settings.UseHeadset || _settingsService.Settings.UseMediaControls)
        {
            return new Intent(this, typeof(MediaButtonHandlerService));
        }

        throw new NotImplementedException();
    }
}
