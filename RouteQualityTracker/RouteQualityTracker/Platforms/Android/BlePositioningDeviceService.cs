using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using RouteQualityTracker.Core.Interfaces;
using RouteQualityTracker.Core.Models;
using RouteQualityTracker.Core.Services;

namespace RouteQualityTracker.Platforms.Android;

[Service(ForegroundServiceType = ForegroundService.TypeConnectedDevice, Exported = true)]
public class BlePositioningDeviceService : Service
{
    private IServiceManager _serviceManager;
    private readonly IAndroidNotificationService _androidNotificationService;
    private readonly IQualityTrackingService _qualityTrackerService;

    private NotificationManager NotificationManager => (NotificationManager)GetSystemService(NotificationService)!;
    private const int NotificationId = 1010;

    public BlePositioningDeviceService()
    {
        _serviceManager = ServiceHelper.Services.GetService<IServiceManager>()!;
        _androidNotificationService = ServiceHelper.Services.GetService<IAndroidNotificationService>()!;
        _qualityTrackerService = ServiceHelper.Services.GetService<IQualityTrackingService>()!;
    }

    public override void OnCreate()
    {
        base.OnCreate();

        _androidNotificationService.CreateNotificationChannel(this, NotificationManager, NotificationId);
    }

    public override IBinder? OnBind(Intent? intent)
    {
        throw new NotImplementedException();
    }

    public override bool StopService(Intent? name)
    {
        _qualityTrackerService.OnRouteQualityChanged -= _androidNotificationService.HandleRouteQualityChangeEvent;
        return base.StopService(name);
    }

    [return: GeneratedEnum]
    public override StartCommandResult OnStartCommand(Intent? intent, [GeneratedEnum] StartCommandFlags flags, int startId)
    {
        var notification = _androidNotificationService.CreateActivityNotification(this, null);

        // look for BLE Device before starting foreground service

        if (OperatingSystem.IsAndroidVersionAtLeast(29))
        {
            StartForeground(NotificationId, notification, ForegroundService.TypeConnectedDevice);
        }
        else
        {
            StartForeground(NotificationId, notification);
        }

        _qualityTrackerService.OnRouteQualityChanged += _androidNotificationService.HandleRouteQualityChangeEvent;

        return StartCommandResult.NotSticky;
    }
}
