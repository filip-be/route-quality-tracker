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

namespace RouteQualityTracker.Platforms.Android;

[Service(ForegroundServiceType = ForegroundService.TypeMediaPlayback, Exported = true)]
[IntentFilter(new[] { Intent.ActionMediaButton })]
public class MediaButtonHandlerHandlerService : Service
{
    private const string ChannelId = "MediaButtonHandlerServiceChannel";
    private const int NotificationId = 1000;
    private const string NotificationChannelName = "Media button handler service";
    private const string MediaSessionName = "MediaButtonHandlerSession";

    private NotificationManager NotificationManager => (NotificationManager)GetSystemService(NotificationService)!;

    private MediaSession? _mediaSession;

    private readonly IServiceManager _serviceManager;
    private readonly NotificationSettings _notificationSettings;

    public MediaButtonHandlerHandlerService()
    {
        _serviceManager = ServiceHelper.Services.GetService<IServiceManager>()!;
        _notificationSettings = ServiceHelper.Services.GetService<NotificationSettings>()!;
        var qualityTrackerService = ServiceHelper.Services.GetService<IQualityTrackingService>()!;
        qualityTrackerService.OnRouteQualityChanged += OnRouteQualityChanged;
    }

    private void OnRouteQualityChanged(object? sender, RouteQualityEnum routeQualityEnum)
    {
        var text = $"Route quality: {routeQualityEnum}";
        var notification = CreateActivityNotification(text);
        NotificationManager.Notify(NotificationId, notification);

        if (_notificationSettings.SendSms)
        {
            try
            {
                var manager = SmsManager.Default;
                manager.SendTextMessage(_notificationSettings.SmsNumber, null, text, null, null);
            }
            catch (Exception ex)
            {
                _serviceManager.SetStatus(false, ex);
            }
        }
    }

    public override void OnCreate()
    {
        base.OnCreate();

        CreateNotificationChannel();
    }

    [return: GeneratedEnum]
    public override StartCommandResult OnStartCommand(Intent? intent, [GeneratedEnum] StartCommandFlags flags, int startId)
    {
        var notification = CreateActivityNotification(null);

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

        _serviceManager.SetStatus(true);

        return StartCommandResult.NotSticky;
    }

    private void CreateNotificationChannel()
    {
        var serviceChannel = new NotificationChannel(
                ChannelId,
                NotificationChannelName,
                NotificationImportance.Low
        );

        NotificationManager.CreateNotificationChannel(serviceChannel);
    }

    public override IBinder? OnBind(Intent? intent)
    {
        return null;
    }

    private Notification CreateActivityNotification(string? text)
    {
        var notificationBuilder = new NotificationCompat
                .Builder(this, ChannelId)
            .SetAutoCancel(false)
            .SetOngoing(true)
            .SetSmallIcon(ResourceConstant.Mipmap.appicon)
            .SetContentTitle("Foreground Service")
            .SetContentText(text ?? string.Empty);

        return notificationBuilder.Build();
    }
}
