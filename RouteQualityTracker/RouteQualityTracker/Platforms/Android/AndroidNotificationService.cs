using _Microsoft.Android.Resource.Designer;
using Android.App;
using Android.Telephony;
using AndroidX.Core.App;
using RouteQualityTracker.Core.Interfaces;
using RouteQualityTracker.Core.Models;
using RouteQualityTracker.Services;

namespace RouteQualityTracker.Platforms.Android;

public class AndroidNotificationService(IServiceManager serviceManager, ISettingsService settingsService) : IAndroidNotificationService
{
    private const string ChannelId = "RouteQualityStatusChannel";
    private const string NotificationChannelName = "Route quality status";

    private int? _notificationId;
    private NotificationManager? _notificationManager;
    private Service? _parentService;

    public Notification CreateActivityNotification(Service service, string? text)
    {
        var notificationBuilder = new NotificationCompat
                .Builder(service, ChannelId)
            .SetAutoCancel(false)
            .SetOngoing(true)
            .SetSmallIcon(ResourceConstant.Mipmap.appicon)
            .SetContentTitle("Foreground Service")
            .SetContentText(text ?? string.Empty);

        return notificationBuilder.Build();
    }

    public void CreateNotificationChannel(Service parentService, NotificationManager notificationManager, int notificationId)
    {
        _parentService = parentService;
        _notificationId = notificationId;
        _notificationManager = notificationManager;

        var serviceChannel = new NotificationChannel(
                ChannelId,
                NotificationChannelName,
                NotificationImportance.Low
        );

        _notificationManager.CreateNotificationChannel(serviceChannel);
    }

    public void HandleRouteQualityChangeEvent(object? sender, RouteQualityEnum routeQuality)
    {
        if (_parentService is null || _notificationId is null || _notificationManager is null)
        {
            throw new InvalidOperationException("Notification service has not been initialized yet");
        }

        var text = $"Route quality: {routeQuality}";
        var notification = CreateActivityNotification(_parentService, text);
        _notificationManager.Notify(_notificationId.Value, notification);

        if (settingsService.Settings.SendSms)
        {
            try
            {
                var manager = SmsManager.Default!;
                manager.SendTextMessage(settingsService.Settings.SmsNumber, null, text, null, null);
            }
            catch (Exception ex)
            {
                serviceManager.SetStatus(false, ex);
            }
        }
    }
}
