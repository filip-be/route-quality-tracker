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
        using var inputStream = PrepareGpxXml(new DateTime(2023, 12, 12, 0, 0, 0, DateTimeKind.Utc));

        var qualityPoints = new List<RouteQualityRecord>
        {
            new()
            {
                Date = new DateTime(2023, 12, 01, 0, 0, 0, DateTimeKind.Utc),
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
            new DateTime(2023, 12, 16, 09, 10, 0, DateTimeKind.Utc),
            new DateTime(2023, 12, 16, 09, 11, 0, DateTimeKind.Utc));

        var qualityPoints = new List<RouteQualityRecord>
        {
            new()
            {
                Date = new DateTime(2023, 12, 16, 09, 10, 0, DateTimeKind.Utc),
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
            new DateTime(2023, 12, 16, 09, 10, 0, DateTimeKind.Utc),
            new DateTime(2023, 12, 16, 09, 11, 0, DateTimeKind.Utc),
            new DateTime(2023, 12, 16, 09, 12, 0, DateTimeKind.Utc));

        var qualityPoints = new List<RouteQualityRecord>
        {
            new()
            {
                Date = new DateTime(2023, 12, 16, 09, 10, 0, DateTimeKind.Utc),
                RouteQuality = RouteQualityEnum.Standard
            },
            new()
            {
                Date = new DateTime(2023, 12, 16, 09, 11, 0, DateTimeKind.Utc),
                RouteQuality = RouteQualityEnum.Bad
            }
        };

        var result = await _trackAnalyzer.MarkupTrack(inputStream, qualityPoints);

        result.Tracks.Count.Should().Be(2);
    }

    // See https://osmand.net/docs/technical/osmand-file-formats/osmand-gpx#track-appearance
    [Test]
    public async Task MarkupTrack_AddsColorToTrack()
    {
        using var inputStream = PrepareGpxXml(
            new DateTime(2023, 12, 16, 09, 10, 0, DateTimeKind.Utc),
            new DateTime(2023, 12, 16, 09, 11, 0, DateTimeKind.Utc),
            new DateTime(2023, 12, 16, 09, 12, 0, DateTimeKind.Utc),
            new DateTime(2023, 12, 16, 09, 13, 0, DateTimeKind.Utc));

        var qualityPoints = new List<RouteQualityRecord>
        {
            new()
            {
                Date = new DateTime(2023, 12, 16, 09, 10, 0, DateTimeKind.Utc),
                RouteQuality = RouteQualityEnum.Bad
            },
            new()
            {
                Date = new DateTime(2023, 12, 16, 09, 11, 0, DateTimeKind.Utc),
                RouteQuality = RouteQualityEnum.Standard
            },
            new()
            {
                Date = new DateTime(2023, 12, 16, 09, 12, 0, DateTimeKind.Utc),
                RouteQuality = RouteQualityEnum.Good
            }
        };

        var result = await _trackAnalyzer.MarkupTrack(inputStream, qualityPoints);

        result.Tracks.Count.Should().Be(3);
        result.Tracks[0].TrackQuality.Should().Be(RouteQualityEnum.Bad);
        result.Tracks[1].TrackQuality.Should().Be(RouteQualityEnum.Standard);
        result.Tracks[2].TrackQuality.Should().Be(RouteQualityEnum.Good);
    }


    // See https://osmand.net/docs/technical/osmand-file-formats/osmand-gpx#track-appearance
    [Test]
    public async Task MarkupTrack_AddsNameToTrack()
    {
        using var inputStream = PrepareGpxXml(
            new DateTime(2023, 12, 16, 09, 10, 0, DateTimeKind.Utc),
            new DateTime(2023, 12, 16, 09, 11, 0, DateTimeKind.Utc),
            new DateTime(2023, 12, 16, 09, 12, 0, DateTimeKind.Utc),
            new DateTime(2023, 12, 16, 09, 13, 0, DateTimeKind.Utc));

        var qualityPoints = new List<RouteQualityRecord>
        {
            new()
            {
                Date = new DateTime(2023, 12, 16, 09, 10, 0, DateTimeKind.Utc),
                RouteQuality = RouteQualityEnum.Bad
            },
            new()
            {
                Date = new DateTime(2023, 12, 16, 09, 11, 0, DateTimeKind.Utc),
                RouteQuality = RouteQualityEnum.Standard
            },
            new()
            {
                Date = new DateTime(2023, 12, 16, 09, 12, 0, DateTimeKind.Utc),
                RouteQuality = RouteQualityEnum.Good
            }
        };

        var result = await _trackAnalyzer.MarkupTrack(inputStream, qualityPoints);

        result.Tracks.Count.Should().Be(3);
        result.Tracks[0].Name.Should().Be(RouteQualityEnum.Bad.ToString());
        result.Tracks[1].Name.Should().Be(RouteQualityEnum.Standard.ToString());
        result.Tracks[2].Name.Should().Be(RouteQualityEnum.Good.ToString());
    }

    [Test]
    public async Task MarkupTrack_AddsColorToTrack_WithQuickRouteQualitySwitch()
    {
        using var inputStream = PrepareGpxXml(
            new DateTime(2023, 12, 16, 09, 10, 0, DateTimeKind.Utc),
            new DateTime(2023, 12, 16, 09, 11, 0, DateTimeKind.Utc),
            new DateTime(2023, 12, 16, 09, 11, 1, DateTimeKind.Utc),
            new DateTime(2023, 12, 16, 09, 11, 2, DateTimeKind.Utc),
            new DateTime(2023, 12, 16, 09, 11, 3, DateTimeKind.Utc),
            new DateTime(2023, 12, 16, 09, 12, 0, DateTimeKind.Utc),
            new DateTime(2023, 12, 16, 09, 12, 1, DateTimeKind.Utc),
            new DateTime(2023, 12, 16, 09, 12, 2, DateTimeKind.Utc),
            new DateTime(2023, 12, 16, 09, 12, 3, DateTimeKind.Utc),
            new DateTime(2023, 12, 16, 09, 13, 0, DateTimeKind.Utc));

        var qualityPoints = new List<RouteQualityRecord>
        {
            new()
            {
                Date = new DateTime(2023, 12, 16, 09, 10, 0, DateTimeKind.Utc),
                RouteQuality = RouteQualityEnum.Bad
            },
            // This record route quality should be ignored as next record differs only in 5 secods
            new()
            {
                Date = new DateTime(2023, 12, 16, 09, 11, 0, DateTimeKind.Utc),
                RouteQuality = RouteQualityEnum.Good
            },
            // This record route quality should be used instead
            new()
            {
                Date = new DateTime(2023, 12, 16, 09, 11, 5, DateTimeKind.Utc),
                RouteQuality = RouteQualityEnum.Standard
            },
            new()
            {
                Date = new DateTime(2023, 12, 16, 09, 12, 0, DateTimeKind.Utc),
                RouteQuality = RouteQualityEnum.Bad
            }
        };

        var result = await _trackAnalyzer.MarkupTrack(inputStream, qualityPoints);

        result.Tracks.Count.Should().Be(3);
        result.Tracks[0].TrackQuality.Should().Be(RouteQualityEnum.Bad);
        result.Tracks[1].TrackQuality.Should().Be(RouteQualityEnum.Standard);
        result.Tracks[2].TrackQuality.Should().Be(RouteQualityEnum.Bad);
    }

    [Test]
    public async Task MarkupTrack_IgnoresQuickRouteQualitySwitch()
    {
        using var inputStream = PrepareGpxXml(
            new DateTime(2023, 12, 16, 09, 10, 0, DateTimeKind.Utc),
            new DateTime(2023, 12, 16, 09, 10, 1, DateTimeKind.Utc),
            new DateTime(2023, 12, 16, 09, 11, 0, DateTimeKind.Utc),
            new DateTime(2023, 12, 16, 09, 11, 1, DateTimeKind.Utc),
            new DateTime(2023, 12, 16, 09, 12, 0, DateTimeKind.Utc));

        var qualityPoints = new List<RouteQualityRecord>
        {
            new()
            {
                Date = new DateTime(2023, 12, 16, 09, 10, 0, DateTimeKind.Utc),
                RouteQuality = RouteQualityEnum.Bad
            },
            new()
            {
                Date = new DateTime(2023, 12, 16, 09, 11, 0, DateTimeKind.Utc),
                RouteQuality = RouteQualityEnum.Good
            },
            // This record should be ignored as it differs only in 5 seconds from previous
            new()
            {
                Date = new DateTime(2023, 12, 16, 09, 11, 05, DateTimeKind.Utc),
                RouteQuality = RouteQualityEnum.Standard
            }
        };

        var result = await _trackAnalyzer.MarkupTrack(inputStream, qualityPoints);

        result.Tracks.Count.Should().Be(2, "because 5-seconds long record should be ignored");
    }

    [Test]
    public async Task MarkupTrack_MergesQualityRecordsWithSameQuality()
    {
        using var inputStream = PrepareGpxXml(
            new DateTime(2023, 12, 16, 09, 10, 0, DateTimeKind.Utc),
            new DateTime(2023, 12, 16, 09, 10, 1, DateTimeKind.Utc),
            new DateTime(2023, 12, 16, 09, 11, 0, DateTimeKind.Utc),
            new DateTime(2023, 12, 16, 09, 11, 1, DateTimeKind.Utc),
            new DateTime(2023, 12, 16, 09, 12, 0, DateTimeKind.Utc));

        var qualityPoints = new List<RouteQualityRecord>
        {
            new()
            {
                Date = new DateTime(2023, 12, 16, 09, 10, 0, DateTimeKind.Utc),
                RouteQuality = RouteQualityEnum.Bad
            },
            new()
            {
                Date = new DateTime(2023, 12, 16, 09, 11, 0, DateTimeKind.Utc),
                RouteQuality = RouteQualityEnum.Good
            },
            // This record has the same quality as previous and should be ignored
            new()
            {
                Date = new DateTime(2023, 12, 16, 09, 11, 30, DateTimeKind.Utc),
                RouteQuality = RouteQualityEnum.Good
            }
        };

        var result = await _trackAnalyzer.MarkupTrack(inputStream, qualityPoints);

        result.Tracks.Count.Should().Be(2, "because last quality records is the same as previous");
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
