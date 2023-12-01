using RouteQualityTracker.Core.Enums;

namespace RouteQualityTracker.Core.Interfaces;

public interface IQualityTrackingService
{
    void StartTracking();
    
    void StopTracking();

    void ToggleRouteQuality();

    RouteQuality GetCurrentRouteQuality();

    event EventHandler<RouteQuality> OnRouteQualityChanged;
}
