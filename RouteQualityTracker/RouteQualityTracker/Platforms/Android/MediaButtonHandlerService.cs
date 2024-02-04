using _Microsoft.Android.Resource.Designer;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Media.Session;
using Android.OS;
using Android.Runtime;
using Android.Telephony;
using AndroidX.Core.App;
using RouteQualityTracker.Core.Interfaces;
using RouteQualityTracker.Core.Models;
using RouteQualityTracker.Core.Services;
using RouteQualityTracker.Services;

namespace RouteQualityTracker.Platforms.Android;

[Service(ForegroundServiceType = ForegroundService.TypeMediaPlayback, Exported = true)]
[IntentFilter([Intent.ActionMediaButton])]
public class MediaButtonHandlerService : Service
{
    private const string MediaSessionName = "MediaButtonHandlerSession";

    private MediaSession? _mediaSession;
    
    private NotificationManager NotificationManager => (NotificationManager)GetSystemService(NotificationService)!;
    private const int NotificationId = 1000;

    private readonly IServiceManager _serviceManager;
    private readonly IAndroidNotificationService _androidNotificationService;
    private readonly IQualityTrackingService _qualityTrackerService;

    public MediaButtonHandlerService()
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
            StartForeground(NotificationId, notification, ForegroundService.TypeMediaPlayback);
        }
        else
        {
            StartForeground(NotificationId, notification);
        }

        if (_mediaSession == null)
        {
            _mediaSession = new MediaSession(this, MediaSessionName);

            _mediaSession.SetFlags(MediaSessionFlags.HandlesMediaButtons);
            _mediaSession.SetCallback(new MediaSessionCallback());
        }

        _mediaSession.Active = true;

        _qualityTrackerService.OnRouteQualityChanged += _androidNotificationService.HandleRouteQualityChangeEvent;

        _serviceManager.SetStatus(true);

        return StartCommandResult.NotSticky;
    }

    public override IBinder? OnBind(Intent? intent)
    {
        return null;
    }
}
