using FluentAssertions;
using NUnit.Framework;
using RouteQualityTracker.Core.Interfaces;
using RouteQualityTracker.Core.Models;
using RouteQualityTracker.Core.Services;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace RouteQualityTracker.Tests.Services;

[TestFixture]
public class TrackAnalyzerTests
{
    private ITrackAnalyzer _trackAnalyzer;

    [SetUp]
    public void SetUp()
    {
        _trackAnalyzer = new TrackAnalyzer();
    }

    [Test]
    public async Task MarkupTrack_Throws_WhenTrack_HasNoGpxTag()
    {
        var inputDocument = """
            <?xml version='1.0' encoding='UTF-8'?>
            <wrongFormat />
            """;
        var inputBytes = Encoding.UTF8.GetBytes(inputDocument);
        using var inputStream = new MemoryStream(inputBytes);

        var qualityPoints = new List<RouteQualityRecord> { new() };

        Func<Task> act = async () => { await _trackAnalyzer.MarkupTrack(inputStream, qualityPoints); };

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Test]
    public async Task MarkupTrack_Throws_WhenTrack_HasNoTrackPoints()
    {
        using var inputStream = PrepareGpxXml();

        var qualityPoints = new List<RouteQualityRecord> { new() };

        Func<Task> act = async () => { await _trackAnalyzer.MarkupTrack(inputStream, qualityPoints); };

        await act.Should().ThrowAsync<InvalidDataException>();
    }

    [Test]
    public async Task MarkupTrack_Throws_WhenThereIsNoRouteQualityData()
    {
        using var inputStream = PrepareGpxXml(new DateTimeOffset());

        var qualityPoints = new List<RouteQualityRecord>();

        Func<Task> act = async () => { await _trackAnalyzer.MarkupTrack(inputStream, qualityPoints); };

        await act.Should().ThrowAsync<InvalidDataException>();
    }

    [Test]
    public async Task MarkupTrack_Throws_WhenQualityData_IsFromAnotherDay()
    {
        using var inputStream = PrepareGpxXml(new DateTime(2023, 12, 12));

        var qualityPoints = new List<RouteQualityRecord>
        {
            new()
            {
                Date = new DateTime(2023, 12, 01),
                RouteQuality = RouteQualityEnum.Standard
            }
        };

        Func<Task> act = async () => { await _trackAnalyzer.MarkupTrack(inputStream, qualityPoints); };

        await act.Should().ThrowAsync<InvalidDataException>();
    }

    [Test]
    public async Task MarkupTrack_Returns_GpxData()
    {
        using var inputStream = PrepareGpxXml(
            new DateTime(2023, 12, 16, 09, 10, 0),
            new DateTime(2023, 12, 16, 09, 11, 0));

        var qualityPoints = new List<RouteQualityRecord>
        {
            new()
            {
                Date = new DateTime(2023, 12, 16, 09, 10, 0),
                RouteQuality = RouteQualityEnum.Standard
            }
        };

        var result = await _trackAnalyzer.MarkupTrack(inputStream, qualityPoints);

        result.Should().NotBeNull();
    }

    [Test]
    public async Task MarkupTrack_SplitsGpxIntoMultipleTracks()
    {
        using var inputStream = PrepareGpxXml(
            new DateTime(2023, 12, 16, 09, 10, 0),
            new DateTime(2023, 12, 16, 09, 11, 0),
            new DateTime(2023, 12, 16, 09, 12, 0));

        var qualityPoints = new List<RouteQualityRecord>
        {
            new()
            {
                Date = new DateTime(2023, 12, 16, 09, 10, 0),
                RouteQuality = RouteQualityEnum.Standard
            },
            new()
            {
                Date = new DateTime(2023, 12, 16, 09, 11, 0),
                RouteQuality = RouteQualityEnum.Bad
            }
        };

        var result = await _trackAnalyzer.MarkupTrack(inputStream, qualityPoints);

        result.Tracks.Count().Should().Be(2);
    }

    // See https://osmand.net/docs/technical/osmand-file-formats/osmand-gpx#track-appearance
    [Test]
    public async Task MarkupTrack_AddsColorToTrack()
    {
        using var inputStream = PrepareGpxXml(
            new DateTime(2023, 12, 16, 09, 10, 0),
            new DateTime(2023, 12, 16, 09, 11, 0),
            new DateTime(2023, 12, 16, 09, 12, 0),
            new DateTime(2023, 12, 16, 09, 13, 0));

        var qualityPoints = new List<RouteQualityRecord>
        {
            new()
            {
                Date = new DateTime(2023, 12, 16, 09, 10, 0),
                RouteQuality = RouteQualityEnum.Bad
            },
            new()
            {
                Date = new DateTime(2023, 12, 16, 09, 11, 0),
                RouteQuality = RouteQualityEnum.Standard
            },
            new()
            {
                Date = new DateTime(2023, 12, 16, 09, 12, 0),
                RouteQuality = RouteQualityEnum.Good
            }
        };

        var result = await _trackAnalyzer.MarkupTrack(inputStream, qualityPoints);

        result.Tracks.Count().Should().Be(3);
        result.Tracks[0].Color.Should().Be(TrackColor.Bad);
        result.Tracks[1].Color.Should().Be(TrackColor.Standard);
        result.Tracks[2].Color.Should().Be(TrackColor.Good);
    }

    [Test]
    public async Task MarkupTrack_IgnoresQuickRouteQualitySwitch()
    {
        using var inputStream = PrepareGpxXml(
            new DateTime(2023, 12, 16, 09, 10, 0),
            new DateTime(2023, 12, 16, 09, 11, 0),
            new DateTime(2023, 12, 16, 09, 12, 0));

        var qualityPoints = new List<RouteQualityRecord>
        {
            new()
            {
                Date = new DateTime(2023, 12, 16, 09, 10, 0),
                RouteQuality = RouteQualityEnum.Bad
            },
            new()
            {
                Date = new DateTime(2023, 12, 16, 09, 11, 0),
                RouteQuality = RouteQualityEnum.Good
            },
            new()
            {
                Date = new DateTime(2023, 12, 16, 09, 11, 05),
                RouteQuality = RouteQualityEnum.Standard
            }
        };

        var result = await _trackAnalyzer.MarkupTrack(inputStream, qualityPoints);

        result.Tracks.Count().Should().Be(2, "because 5-seconds long record should be ignored");
        result.Tracks[0].Color.Should().Be(TrackColor.Bad, "because initial quality record was bad");
        result.Tracks[0].Color.Should().Be(TrackColor.Standard, "because last quality record was standard");
    }

    // Match records to points (before/after, etc.)[Test]
    [Test]
    public async Task MarkupTrack_MatchesRecordsToTrackPoints()
    {
        var firstTrackStart = new DateTime(2023, 12, 16, 09, 10, 0);
        var secondTrackStart = new DateTime(2023, 12, 16, 09, 11, 0);
        var thirdTrackStart = new DateTime(2023, 12, 16, 09, 12, 0);

        using var inputStream = PrepareGpxXml(
            firstTrackStart,
            secondTrackStart,
            thirdTrackStart,
            new DateTime(2023, 12, 16, 09, 13, 0));

        var qualityPoints = new List<RouteQualityRecord>
        {
            new()
            {
                Date = new DateTime(2023, 12, 16, 09, 10, 0),
                RouteQuality = RouteQualityEnum.Bad
            },
            new()
            {
                Date = new DateTime(2023, 12, 16, 09, 10, 50),
                RouteQuality = RouteQualityEnum.Standard
            },
            new()
            {
                Date = new DateTime(2023, 12, 16, 09, 12, 10),
                RouteQuality = RouteQualityEnum.Good
            }
        };

        var result = await _trackAnalyzer.MarkupTrack(inputStream, qualityPoints);

        result.Tracks.Count().Should().Be(3);
        result.Tracks[0].Color.Should().Be(TrackColor.Bad);
        result.Tracks[0].StartTime.Should().Be(firstTrackStart);
        result.Tracks[1].Color.Should().Be(TrackColor.Standard);
        result.Tracks[1].StartTime.Should().Be(secondTrackStart);
        result.Tracks[2].Color.Should().Be(TrackColor.Standard);
        result.Tracks[2].StartTime.Should().Be(thirdTrackStart);
    }

    private MemoryStream PrepareGpxXml(params DateTimeOffset[] trackPoints)
    {
        XNamespace gpxNamespace = "http://www.topografix.com/GPX/1/1";
        var inputDocument = $"""
            <?xml version='1.0' encoding='UTF-8'?>
            <gpx version="1.1" creator="https://www.komoot.dez" xmlns="{gpxNamespace}" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:schemaLocation="http://www.topografix.com/GPX/1/1 http://www.topografix.com/GPX/1/1/gpx.xsd">
              <metadata>
                <name>Cycling</name>
                <author>
                  <link href="https://www.komoot.de">
                    <text>komoot</text>
                    <type>text/html</type>
                  </link>
                </author>
              </metadata>
              <trk>
                <name>Cycling</name>
                <trkseg>
                </trkseg>
              </trk>
            </gpx>
            """;

        var emptyGpxBytes = Encoding.UTF8.GetBytes(inputDocument);
        using var emptyGpxStream = new MemoryStream(emptyGpxBytes);
        var gpx = XDocument.Load(emptyGpxStream);

        var gpxNamespaceManager = new XmlNamespaceManager(new NameTable());
        gpxNamespaceManager.AddNamespace("gpx", gpxNamespace.ToString());
        var segment = gpx.XPathSelectElement("//gpx:trkseg", gpxNamespaceManager)!;

        foreach (var trackPoint in trackPoints)
        {
            var trackPointElement = new XElement(gpxNamespace + "trkpt",
                new XAttribute(gpxNamespace + "lat", "51.118080"),
                new XAttribute(gpxNamespace + "lon", "17.090176"),
                new XElement(gpxNamespace + "time", trackPoint.ToString()));

            segment.Add(trackPointElement);
        }

        var gpxString = gpx.ToString();
        var gpxBytes = Encoding.UTF8.GetBytes(gpxString);
        return new MemoryStream(gpxBytes);
    }
}
