using RouteQualityTracker.Core.Interfaces;
using RouteQualityTracker.Core.Models;

namespace RouteQualityTracker.Core.Services;

public class QualityTrackingService : IQualityTrackingService
{
    private RouteQualityEnum _currentRouteQualityEnum = RouteQualityEnum.Unknown;
    private bool _isQualityIncreasing;

    private IList<RouteQualityRecord> _routeQualityData = new List<RouteQualityRecord>();

    public QualityTrackingService()
    {
        OnRouteQualityChanged += OnRouteQualityChangedInternal;
    }

    private void OnRouteQualityChangedInternal(object? sender, RouteQualityEnum e)
    {
        _routeQualityData.Add(new RouteQualityRecord
        {
            Date = DateTime.UtcNow,
            RouteQuality =_currentRouteQualityEnum
        });
    }

    public void StartTracking()
    {
        _routeQualityData.Clear();
        _currentRouteQualityEnum = RouteQualityEnum.Standard;
        _isQualityIncreasing = true;
        OnRouteQualityChanged?.Invoke(this, _currentRouteQualityEnum);
    }

    public void StopTracking()
    {
        _currentRouteQualityEnum = RouteQualityEnum.Unknown;
    }

    public void ToggleRouteQuality()
    {
        switch (_currentRouteQualityEnum)
        {
            case RouteQualityEnum.Bad:
                _currentRouteQualityEnum = RouteQualityEnum.Standard;
                _isQualityIncreasing = true;
                break;
            case RouteQualityEnum.Standard:
                _currentRouteQualityEnum = _isQualityIncreasing ? RouteQualityEnum.Good : RouteQualityEnum.Bad;
                break;
            case RouteQualityEnum.Good:
                _currentRouteQualityEnum = RouteQualityEnum.Standard;
                _isQualityIncreasing = false;
                break;
        }
        OnRouteQualityChanged?.Invoke(this, _currentRouteQualityEnum);
    }

    public RouteQualityEnum GetCurrentRouteQuality()
    {
        return _currentRouteQualityEnum;
    }
    public IList<RouteQualityRecord> GetRouteQualityRecords()
    {
        return _routeQualityData;
    }

    public event EventHandler<RouteQualityEnum>? OnRouteQualityChanged;
}
