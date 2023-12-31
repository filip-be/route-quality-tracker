using RouteQualityTracker.Core.Gpx;
using RouteQualityTracker.Core.Models;
using System.Xml;

namespace RouteQualityTracker.Core.Interfaces;

public interface ITrackAnalyzer
{
    Task<GpxData> MarkupTrack(Stream input, IList<RouteQualityRecord> records);
}
