using Geo.Gps;
using Geo.Gps.Serialization;
using RouteQualityTracker.Core.Interfaces;
using RouteQualityTracker.Core.Models;
using System.Xml;

namespace RouteQualityTracker.Core.Services;

public class TrackAnalyzer : ITrackAnalyzer
{
    public XmlDocument MarkupTrack(Stream input, IList<RouteQualityRecord> records)
    {
        GpsData? gpsData = DeserializeGpxFile(input);

        if (gpsData is null)
        {
            throw new InvalidOperationException("stream is not valid GPX file");
        }

        var waypoints = gpsData.Tracks
            ?.SelectMany(t => t.Segments)
            .SelectMany(s => s.Waypoints)
            .Where(w => w.TimeUtc is not null)
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

    private static GpsData? DeserializeGpxFile(Stream input)
    {
        var gpx10 = new Gpx10Serializer();
        var gpx11 = new Gpx11Serializer();

        using var streamWrapper = new StreamWrapper(input);
        if (gpx10.CanDeSerialize(streamWrapper))
        {
            return gpx10.DeSerialize(streamWrapper);
        }
        else if (gpx11.CanDeSerialize(streamWrapper))
        {
            return gpx11.DeSerialize(streamWrapper);
        }

        return null;
    }
}
