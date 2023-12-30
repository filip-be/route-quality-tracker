using RouteQualityTracker.Core.Models;
using System.Xml;

namespace RouteQualityTracker.Core.Interfaces;

public interface ITrackAnalyzer
{
    Task<XmlDocument> MarkupTrack(Stream input, IList<RouteQualityRecord> records);
}
