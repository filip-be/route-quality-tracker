using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using RouteQualityTracker.Interfaces;

namespace RouteQualityTracker.Platforms.Android;

public class AndroidForegroundService : Service
{
    public AndroidForegroundService(IForegroundService foregroundService)
    {
        foregroundService.OnServiceStart += OnServiceStart;
        foregroundService.OnServiceStop += OnServiceStop;
    }

    private void OnServiceStop(object? sender, EventArgs e)
    {
        StopForeground(StopForegroundFlags.Remove);
    }

    private void OnServiceStart(object? sender, EventArgs e)
    {
        var serviceIntent = new Intent(this, typeof(AndroidForegroundService));
        
        StartForegroundService(serviceIntent);
    }

    public override IBinder? OnBind(Intent? intent)
    {
        return null;
    }
}