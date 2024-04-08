using Android.App;
using Android.Media.Session;
using RouteQualityTracker.Core.Models;

namespace RouteQualityTracker.Platforms.Android;

public interface IAndroidNotificationService
{
    Notification CreateActivityNotification(Service service, string? text);

    void CreateNotificationChannel(Service parentService, NotificationManager notificationManager, int notificationId);

    void HandleRouteQualityChangeEvent(object? sender, RouteQualityEnum routeQuality);
    
    void SetMediaSession(MediaSession? mediaSession);
}
