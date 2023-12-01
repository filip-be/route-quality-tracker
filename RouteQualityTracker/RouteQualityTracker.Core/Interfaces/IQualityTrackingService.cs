using RouteQualityTracker.Core.Models;

namespace RouteQualityTracker.Core.Interfaces;

public interface IQualityTrackingService
{
    void StartTracking();
    
    void StopTracking();

    void ToggleRouteQuality();

    RouteQualityEnum GetCurrentRouteQuality();

    event EventHandler<RouteQualityEnum> OnRouteQualityChanged;

    IList<RouteQualityRecord> GetRouteQualityRecords();
}
