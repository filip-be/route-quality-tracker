using RouteQualityTracker.Core.Models;

namespace RouteQualityTracker.Core.Interfaces;

public interface INotificationService
{
    bool IsSendEmailEnabled { get; }

    Task SendEmail(RouteQualityEnum routeQuality);
}
