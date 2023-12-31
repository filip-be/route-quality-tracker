using FluentAssertions;
using NUnit.Framework;
using RouteQualityTracker.Core.Interfaces;
using RouteQualityTracker.Core.Models;
using RouteQualityTracker.Core.Services;
using System.Text;

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
        var inputDocument = """
            <?xml version='1.0' encoding='UTF-8'?>
            <gpx version="1.1" creator="https://www.komoot.de" xmlns="http://www.topografix.com/GPX/1/1" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:schemaLocation="http://www.topografix.com/GPX/1/1 http://www.topografix.com/GPX/1/1/gpx.xsd">
              <trk>
                <name>Cycling</name>
                <trkseg>
                </trkseg>
              </trk>
            </gpx>
            """;

        var inputBytes = Encoding.UTF8.GetBytes(inputDocument);
        using var inputStream = new MemoryStream(inputBytes);

        var qualityPoints = new List<RouteQualityRecord> { new() };
                
        Func<Task> act = async () => { await _trackAnalyzer.MarkupTrack(inputStream, qualityPoints); };

        await act.Should().ThrowAsync<InvalidDataException>();
    }

    [Test]
    public async Task MarkupTrack_Throws_WhenThereIsNoRouteQualityData()
    {
        var inputDocument = """
            <?xml version='1.0' encoding='UTF-8'?>
            <gpx version="1.1" creator="https://www.komoot.de" xmlns="http://www.topografix.com/GPX/1/1" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:schemaLocation="http://www.topografix.com/GPX/1/1 http://www.topografix.com/GPX/1/1/gpx.xsd">
              <trk>
                <name>Cycling</name>
                <trkseg>
                  <trkpt lat="51.118080" lon="17.090176">
                    <ele>114.820376</ele>
                    <time>2023-12-16T09:23:48.000Z</time>
                  </trkpt>
                </trkseg>
              </trk>
            </gpx>
            """;

        var inputBytes = Encoding.UTF8.GetBytes(inputDocument);
        using var inputStream = new MemoryStream(inputBytes);

        var qualityPoints = new List<RouteQualityRecord>();

        Func<Task> act = async () => { await _trackAnalyzer.MarkupTrack(inputStream, qualityPoints); };

        await act.Should().ThrowAsync<InvalidDataException>();
    }

    [Test]
    public async Task MarkupTrack_Throws_WhenQualityData_IsFromAnotherDay()
    {
        var inputDocument = """
            <?xml version='1.0' encoding='UTF-8'?>
            <gpx version="1.1" creator="https://www.komoot.de" xmlns="http://www.topografix.com/GPX/1/1" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:schemaLocation="http://www.topografix.com/GPX/1/1 http://www.topografix.com/GPX/1/1/gpx.xsd">
              <trk>
                <name>Cycling</name>
                <trkseg>
                  <trkpt lat="51.118080" lon="17.090176">
                    <ele>114.820376</ele>
                    <time>2023-12-16T09:23:48.000Z</time>
                  </trkpt>
                  <trkpt lat="51.118123" lon="17.090189">
                      <ele>114.820376</ele>
                      <time>2023-12-16T09:23:49.000Z</time>
                  </trkpt>
                </trkseg>
              </trk>
            </gpx>
            """;

        var inputBytes = Encoding.UTF8.GetBytes(inputDocument);
        using var inputStream = new MemoryStream(inputBytes);

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
        var inputDocument = """
            <?xml version='1.0' encoding='UTF-8'?>
            <gpx version="1.1" creator="https://www.komoot.de" xmlns="http://www.topografix.com/GPX/1/1" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:schemaLocation="http://www.topografix.com/GPX/1/1 http://www.topografix.com/GPX/1/1/gpx.xsd">
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
                  <trkpt lat="51.118080" lon="17.090176">
                    <ele>114.820376</ele>
                    <time>2023-12-16T09:10:00.000Z</time>
                  </trkpt>
                  <trkpt lat="51.118123" lon="17.090189">
                      <ele>114.820376</ele>
                      <time>2023-12-16T09:11:00.000Z</time>
                  </trkpt>
                </trkseg>
              </trk>
            </gpx>
            """;

        var inputBytes = Encoding.UTF8.GetBytes(inputDocument);
        using var inputStream = new MemoryStream(inputBytes);

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
}
