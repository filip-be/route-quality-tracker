using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using RouteQualityTracker.Core.Interfaces;
using RouteQualityTracker.Core.Services;

namespace RouteQualityTracker.Platforms.Android;

[Service(ForegroundServiceType = ForegroundService.TypeConnectedDevice, Exported = true)]
public class BlePositioningDeviceService : Service
{
    private readonly IServiceManager _serviceManager = ServiceHelper.Services.GetService<IServiceManager>()!;
    private readonly IAndroidNotificationService _androidNotificationService = ServiceHelper.Services.GetService<IAndroidNotificationService>()!;
    private readonly IQualityTrackingService _qualityTrackerService = ServiceHelper.Services.GetService<IQualityTrackingService>()!;


    private NotificationManager NotificationManager => (NotificationManager)GetSystemService(NotificationService)!;
    private const int NotificationId = 1010;

    public override void OnCreate()
    {
        base.OnCreate();

        _androidNotificationService.CreateNotificationChannel(this, NotificationManager, NotificationId);
    }

    public override IBinder? OnBind(Intent? intent)
    {
        return null;
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
        
        if (OperatingSystem.IsAndroidVersionAtLeast(29))
        {
            StartForeground(NotificationId, notification, ForegroundService.TypeConnectedDevice);
        }
        else
        {
            StartForeground(NotificationId, notification);
        }

        _qualityTrackerService.OnRouteQualityChanged += _androidNotificationService.HandleRouteQualityChangeEvent;

        _serviceManager.SetStatus(true);

        return StartCommandResult.NotSticky;
    }

    class CustomBluetoothGattCallback : BluetoothGattCallback
    {
        public override void OnConnectionStateChange(BluetoothGatt? gatt, [GeneratedEnum] GattStatus status, [GeneratedEnum] ProfileState newState)
        {
            //connectionstatus
            base.OnConnectionStateChange(gatt, status, newState);
        }
    }

}
