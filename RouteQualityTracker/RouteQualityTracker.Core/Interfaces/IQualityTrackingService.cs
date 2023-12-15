using RouteQualityTracker.Core.Models;

namespace RouteQualityTracker.Core.Interfaces;

public interface IQualityTrackingService
{
    void StartTracking();
    
    void StopTracking();

    void ToggleRouteQuality();

    void SetRouteQuality(RouteQualityEnum quality);

    RouteQualityEnum GetCurrentRouteQuality();

    event EventHandler<RouteQualityEnum> OnRouteQualityChanged;

    IList<RouteQualityRecord> GetRouteQualityRecords();
}
