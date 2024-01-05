using System.Xml;
using System.Xml.Linq;

namespace RouteQualityTracker.Core.Gpx;

public class GpxData
{
    private XDocument _gpxData;
    private string _gpxNamespace;

    public IList<GpxWaypoint>? Waypoints => _gpxData.Root?.XPathSelectElements<GpxWaypoint>("//trkpt", _gpxNamespace);

    public IList<GpxTrack>? Tracks
    {
        get => _gpxData.Root?.XPathSelectElements<GpxTrack>("//trk", _gpxNamespace);
        set
        {
            var oldTracks = _gpxData.Root?.XPathSelectElements<GpxTrack>("//trk", _gpxNamespace);
            oldTracks?.ForEach(t => t.RemoveFromParent());

            value?.ToList().ForEach(t => _gpxData.Add(t.ToXElement()));
        }
    }

    public async Task Load(Stream input)
    {
        var gpxNamespace = await GetXmlNamespace(input);
        _gpxNamespace = gpxNamespace;

        _gpxData = XDocument.Load(input);
    }

    public static async Task<bool> CanRead(Stream input)
    {
        var contentNamespace = await GetXmlNamespace(input);  
        return contentNamespace is "http://www.topografix.com/GPX/1/0" or "http://www.topografix.com/GPX/1/1";
    }

    private static async Task<string> GetXmlNamespace(Stream input)
    {
        var settings = new XmlReaderSettings
        {
            Async = true
        };
        var reader = XmlReader.Create(input, settings);
        await reader.MoveToContentAsync();

        var contentNamespace = reader.NamespaceURI;

        input.Position = 0;

        return contentNamespace;
    }
}
