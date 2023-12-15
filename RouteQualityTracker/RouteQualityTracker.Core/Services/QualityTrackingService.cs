using RouteQualityTracker.Core.Interfaces;
using RouteQualityTracker.Core.Models;

namespace RouteQualityTracker.Core.Services;

public class QualityTrackingService : IQualityTrackingService
{
    private RouteQualityEnum _currentRouteQuality = RouteQualityEnum.Unknown;
    private bool _isQualityIncreasing;

    private IList<RouteQualityRecord> _routeQualityData = new List<RouteQualityRecord>();
    
    private readonly TimeProvider _timeProvider;
    private readonly INotificationService _notificationService;

    public QualityTrackingService(TimeProvider timeProvider, INotificationService notificationService)
    {
        OnRouteQualityChanged += OnRouteQualityChangedInternal;
        _timeProvider = timeProvider;
        _notificationService = notificationService;
    }

    private void OnRouteQualityChangedInternal(object? sender, RouteQualityEnum e)
    {
        _routeQualityData.Add(new RouteQualityRecord
        {
            Date = _timeProvider.GetUtcNow(),
            RouteQuality =_currentRouteQuality
        });

        if(_notificationService.IsSendEmailEnabled)
        {
            _notificationService.SendEmail(_currentRouteQuality);
        }
    }

    public void StartTracking()
    {
        _routeQualityData.Clear();
        _currentRouteQuality = RouteQualityEnum.Standard;
        _isQualityIncreasing = true;
        OnRouteQualityChanged?.Invoke(this, _currentRouteQuality);
    }

    public void StopTracking()
    {
        _currentRouteQuality = RouteQualityEnum.Unknown;
    }

    public void ToggleRouteQuality()
    {
        switch (_currentRouteQuality)
        {
            case RouteQualityEnum.Bad:
                _currentRouteQuality = RouteQualityEnum.Standard;
                _isQualityIncreasing = true;
                break;
            case RouteQualityEnum.Standard:
                _currentRouteQuality = _isQualityIncreasing ? RouteQualityEnum.Good : RouteQualityEnum.Bad;
                break;
            case RouteQualityEnum.Good:
                _currentRouteQuality = RouteQualityEnum.Standard;
                _isQualityIncreasing = false;
                break;
        }
        OnRouteQualityChanged?.Invoke(this, _currentRouteQuality);
    }

    public void SetRouteQuality(RouteQualityEnum quality)
    {
        _currentRouteQuality = quality;
        OnRouteQualityChanged?.Invoke(this, _currentRouteQuality);
    }

    public RouteQualityEnum GetCurrentRouteQuality()
    {
        return _currentRouteQuality;
    }
    public IList<RouteQualityRecord> GetRouteQualityRecords()
    {
        return _routeQualityData;
    }

    public event EventHandler<RouteQualityEnum>? OnRouteQualityChanged;
}
