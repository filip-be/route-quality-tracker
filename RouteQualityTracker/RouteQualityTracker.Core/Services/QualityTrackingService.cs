using RouteQualityTracker.Core.Enums;
using RouteQualityTracker.Core.Interfaces;

namespace RouteQualityTracker.Core.Services;

public class QualityTrackingService : IQualityTrackingService
{
    private RouteQuality _currentRouteQuality = RouteQuality.Unknown;
    private bool _isQualityIncreasing;


    public void StartTracking()
    {
        _currentRouteQuality = RouteQuality.Standard;
        _isQualityIncreasing = true;
        OnRouteQualityChanged?.Invoke(this, _currentRouteQuality);
    }

    public void StopTracking()
    {
        _currentRouteQuality = RouteQuality.Unknown;
    }

    public void ToggleRouteQuality()
    {
        switch (_currentRouteQuality)
        {
            case RouteQuality.Bad:
                _currentRouteQuality = RouteQuality.Standard;
                _isQualityIncreasing = true;
                break;
            case RouteQuality.Standard:
                _currentRouteQuality = _isQualityIncreasing ? RouteQuality.Good : RouteQuality.Bad;
                break;
            case RouteQuality.Good:
                _currentRouteQuality = RouteQuality.Standard;
                _isQualityIncreasing = false;
                break;
        }
        OnRouteQualityChanged?.Invoke(this, _currentRouteQuality);
    }

    public RouteQuality GetCurrentRouteQuality()
    {
        return _currentRouteQuality;
    }

    public event EventHandler<RouteQuality>? OnRouteQualityChanged;
}
