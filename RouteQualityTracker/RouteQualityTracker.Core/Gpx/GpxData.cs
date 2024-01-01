using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace RouteQualityTracker.Core.Gpx;

public class GpxData
{
    private XDocument _gpxData;
    private XmlNamespaceManager _gpxNamespaceManager;
    private XNamespace _gpxNamespace;

    private const string NamespacePrefix = "gpx";

    public IList<GpxWaypoint>? Waypoints
    {
        get
        {
            return GetGpxElements<GpxWaypoint>("trkpt");
        }
    }
    public IList<GpxTrack>? Tracks
    {
        get
        {
            return GetGpxElements<GpxTrack>("trk");
        }
    }

    private IList<T>? GetGpxElements<T>(string path)
    {
        var nodes = _gpxData.XPathSelectElements($"//{NamespacePrefix}:{path}", _gpxNamespaceManager);

        if (nodes is null) return null;

        var elements = new List<T>();
        foreach (var node in nodes)
        {
            var element = (T) Activator.CreateInstance(typeof(T), new object[] { node, _gpxNamespace })!;
            elements.Add(element);
        }
        return elements;
    }

    public async Task Load(Stream input)
    {
        var gpxNamespace = await GetXmlNamespace(input);
        _gpxNamespace = gpxNamespace;

        _gpxData = XDocument.Load(input);

        _gpxNamespaceManager = new XmlNamespaceManager(new NameTable());
        _gpxNamespaceManager.AddNamespace(NamespacePrefix, gpxNamespace);
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
