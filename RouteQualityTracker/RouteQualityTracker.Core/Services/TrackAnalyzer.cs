using RouteQualityTracker.Core.Gpx;
using RouteQualityTracker.Core.Interfaces;
using RouteQualityTracker.Core.Models;

namespace RouteQualityTracker.Core.Services;

public class TrackAnalyzer : ITrackAnalyzer
{
    public TimeSpan MinimumQualityRecordTimeDifference { get; set; } = TimeSpan.FromSeconds(10);

    public async Task<GpxData> MarkupTrack(Stream input, IList<RouteQualityRecord> records)
    {
        if (!await GpxData.CanRead(input))
        {
            throw new InvalidOperationException("unsupported file format");
        }

        var gpxData = new GpxData();
        await gpxData.Load(input);

        var waypoints = gpxData.Tracks
            .SelectMany(t => t.WayPoints)
            .Where(w => w.TimeUtc is not null)
            .OrderBy(s => s.TimeUtc)
            .ToList();

        if (!waypoints.Any())
        {
            throw new InvalidDataException("track has no wayPoints");
        }

        if (!records.Any())
        {
            throw new InvalidDataException("there is no route quality records");
        }

        var qualityTrackingStart = records[0].Date;

        if (qualityTrackingStart.Date < waypoints[0].TimeUtc!.Value.Date
            || qualityTrackingStart.Date > waypoints[^1].TimeUtc!.Value.Date)
        {
            throw new InvalidDataException("quality tracking data doesn't match GPX data");
        }

        records = records.OrderBy(r => r.Date).ToList();
        records = FlattenRouteQualityRecords(records);

        var tracks = SplitTracks(gpxData.Tracks[0], waypoints, records);
        gpxData.Tracks = tracks;

        return gpxData;
    }

    private List<RouteQualityRecord> FlattenRouteQualityRecords(IEnumerable<RouteQualityRecord> records)
    {
        var updatedRecords = new List<RouteQualityRecord>();

        foreach (var record in records)
        {
            if (updatedRecords.Count == 0)
            {
                updatedRecords.Add(record);
                continue;
            }

            var lastRecord = updatedRecords[^1];
            var recordsTimeDifference = record.Date.Subtract(lastRecord.Date);

            if (lastRecord.RouteQuality == record.RouteQuality) continue;
            


            if (recordsTimeDifference.CompareTo(MinimumQualityRecordTimeDifference) > 0)
            {
                updatedRecords.Add(record);
            }
            else
            {
                lastRecord.RouteQuality = record.RouteQuality;
            }
        }

        return updatedRecords;
    }

    private static List<GpxTrack> SplitTracks(GpxTrack originalTrack, ICollection<GpxWaypoint> wayPoints, ICollection<RouteQualityRecord> records)
    {
        records.Add(new RouteQualityRecord
        {
            Date = DateTimeOffset.MaxValue,
            RouteQuality = RouteQualityEnum.Unknown
        });

        var newTracks = new List<GpxTrack>();

        var previousRouteQuality = RouteQualityEnum.Unknown;
        foreach (var qualityRecord in records)
        {
            var trackQuality = previousRouteQuality;
            previousRouteQuality = qualityRecord.RouteQuality;

            var newTrack = GetTrackWithPointsBefore(qualityRecord.Date, wayPoints, originalTrack);

            if (newTrack is null) continue;

            newTrack.TrackQuality = trackQuality;
            newTracks.Add(newTrack);
        }

        return newTracks;
    }

    private static GpxTrack? GetTrackWithPointsBefore(DateTimeOffset lookupDate, ICollection<GpxWaypoint> wayPoints, GpxTrack originalTrack)
    {
        var trackWayPoints = wayPoints
            .Where(w => lookupDate.CompareTo(w.TimeUtc!.Value) > 0)
            .ToList();

        if (trackWayPoints.Count == 0) return null;

        var track = originalTrack.CloneEmptyTrack();
        foreach (var wayPoint in trackWayPoints)
        {
            wayPoints.Remove(wayPoint);
        }
        track.WayPoints = trackWayPoints;

        return track;
    }
}
