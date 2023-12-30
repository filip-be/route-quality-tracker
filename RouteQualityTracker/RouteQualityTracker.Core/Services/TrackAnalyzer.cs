using RouteQualityTracker.Core.Interfaces;
using RouteQualityTracker.Core.Models;
using System.Xml;

namespace RouteQualityTracker.Core.Services;

public class TrackAnalyzer : ITrackAnalyzer
{
    public XmlDocument MarkupTrack(Stream input, IList<RouteQualityRecord> records)
    {
        throw new NotImplementedException();
    }
}
