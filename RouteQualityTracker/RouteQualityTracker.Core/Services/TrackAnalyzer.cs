using RouteQualityTracker.Core.Gpx;
using RouteQualityTracker.Core.Interfaces;
using RouteQualityTracker.Core.Models;
using System.Xml;

namespace RouteQualityTracker.Core.Services;

public class TrackAnalyzer : ITrackAnalyzer
{
    public async Task<GpxData> MarkupTrack(Stream input, IList<RouteQualityRecord> records)
    {
        if (!await GpxData.CanRead(input))
        {
            throw new InvalidOperationException("unsupported file format");
        }

        var gpxData = new GpxData();
        await gpxData.Load(input);

        var waypoints = gpxData.Waypoints
            ?.Where(w => w.TimeUtc is not null)
            .OrderBy(s => s.TimeUtc)
            .ToList();

        if (waypoints?.Any() != true)
        {
            throw new InvalidDataException("track has no waypoints");
        }

        if (records?.Any() != true)
        {
            throw new InvalidDataException("there is no route quality records");
        }

        var qualityTrackingStart = records.First().Date;

        if (qualityTrackingStart.Date < waypoints.First().TimeUtc!.Value.Date
            || qualityTrackingStart.Date > waypoints.Last().TimeUtc!.Value.Date)
        {
            throw new InvalidDataException("quality tracking data doesn't match GPX data");
        }

        return gpxData;
    }
}
