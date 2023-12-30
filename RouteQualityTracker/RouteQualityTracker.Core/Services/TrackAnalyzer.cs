using RouteQualityTracker.Core.Gpx;
using RouteQualityTracker.Core.Interfaces;
using RouteQualityTracker.Core.Models;
using System.Xml;

namespace RouteQualityTracker.Core.Services;

public class TrackAnalyzer : ITrackAnalyzer
{
    public async Task<XmlDocument> MarkupTrack(Stream input, IList<RouteQualityRecord> records)
    {
        if (!await GpxData.CanRead(input))
        {
            throw new InvalidOperationException("stream is not valid GPX file");
        }

        var gpxData = new GpxData(input);

        var waypoints = gpxData.Waypoints
            ?.Where(w => w.TimeUtc is not null)
            .OrderBy(s => s.TimeUtc)
            .ToList();

        if (waypoints?.Any() != true)
        {
            throw new InvalidDataException("track has no waypoints");
        }

        var qualityTrackingStart = records.First().Date;

        if (qualityTrackingStart.Date < waypoints.First().TimeUtc!.Value.Date
            || qualityTrackingStart.Date > waypoints.Last().TimeUtc!.Value.Date)
        {
            throw new InvalidDataException("quality tracking data doesn't match GPX data");
        }

        return null;
    }
}
