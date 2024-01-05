using RouteQualityTracker.Core.Gpx;
using RouteQualityTracker.Core.Models;

namespace RouteQualityTracker.Core.Interfaces;

public interface ITrackAnalyzer
{
    TimeSpan MinimumQualityRecordTimeDifference { get; set; }

    Task<GpxData> MarkupTrack(Stream input, IList<RouteQualityRecord> records);
}
