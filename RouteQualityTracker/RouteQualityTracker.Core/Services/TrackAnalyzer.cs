using RouteQualityTracker.Core.Gpx;
using RouteQualityTracker.Core.Interfaces;
using RouteQualityTracker.Core.Models;

namespace RouteQualityTracker.Core.Services;

public class TrackAnalyzer : ITrackAnalyzer
{
    public TimeSpan MinimumQualityRecordTimeDifference { get; set; } = TimeSpan.FromSeconds(10);

    public async Task<GpxData> MarkupTrack(Stream input, IList<RouteQualityRecord> records, Func<double, Task>? updateProgressAction = null)
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

        var task = new Task<Task<List<GpxTrack>>>(async () => await SplitTracks(gpxData.Tracks[0], waypoints, records, updateProgressAction));
        task.Start();
        
        var tracks = await task.Unwrap().WaitAsync(new CancellationToken());
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

    private static async Task<List<GpxTrack>> SplitTracks(GpxTrack originalTrack, List<GpxWaypoint> wayPoints,
        ICollection<RouteQualityRecord> records, Func<double, Task>? updateProgressAction)
    {
        records.Add(new RouteQualityRecord
        {
            Date = DateTimeOffset.MaxValue,
            RouteQuality = RouteQualityEnum.Unknown
        });

        var newTracks = new List<GpxTrack>();

        double recordsCount = records.Count;
        var previousRouteQuality = RouteQualityEnum.Unknown;

        foreach (var item in records.Select((qualityRecord, index) => (qualityRecord, index)))
        {
            var progressAfterProcessingItem = (item.index + 1) / recordsCount;
            var trackQuality = previousRouteQuality;
            previousRouteQuality = item.qualityRecord.RouteQuality;

            var newTrack = GetTrackWithPointsBefore(item.qualityRecord.Date, wayPoints, originalTrack);

            if (newTrack is null)
            {
                if (updateProgressAction is not null)
                {
                    await updateProgressAction.Invoke(progressAfterProcessingItem);
                }

                continue;
            }

            newTrack.TrackQuality = trackQuality;
            newTrack.Name = trackQuality.ToString();
            newTracks.Add(newTrack);
            if (updateProgressAction is not null)
            {
                await updateProgressAction.Invoke(progressAfterProcessingItem);
            }
        }

        return newTracks;
    }

    private static GpxTrack? GetTrackWithPointsBefore(DateTimeOffset lookupDate, List<GpxWaypoint> wayPoints, GpxTrack originalTrack)
    {
        var firstNotMatchingPoint = wayPoints.Find(w => lookupDate.CompareTo(w.TimeUtc!.Value) <= 0);
        var firstNotMatchingPointIndex = firstNotMatchingPoint is not null
            ? wayPoints.IndexOf(firstNotMatchingPoint)
            : wayPoints.Count - 1;

        var trackWayPoints = wayPoints.Any()
            ? wayPoints[..firstNotMatchingPointIndex]
            : new List<GpxWaypoint>
            {
                Capacity = 0
            };

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
