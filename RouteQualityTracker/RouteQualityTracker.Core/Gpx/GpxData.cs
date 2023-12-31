using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace RouteQualityTracker.Core.Gpx;

public class GpxData
{
    private XmlDocument _gpxData;

    public IEnumerable<GpxWaypoint>? Waypoints
    {
        get
        {
            var nodes = _gpxData.SelectNodes("/gpx/trk/trkseg/trkpt");

            if (nodes is null) return null;

            var waypoints = new List<GpxWaypoint>();
            foreach(XElement node in nodes)
            {
                waypoints.Add(new GpxWaypoint(node));
            }
            return waypoints;
        }
    }

    public GpxData(Stream input)
    {
        _gpxData = new XmlDocument();
        _gpxData.Load(input);
    }

    public static async Task<bool> CanRead(Stream input)
    {
        var settings = new XmlReaderSettings
        {
            Async = true
        };
        var reader = XmlReader.Create(input, settings);
        await reader.MoveToContentAsync();

        var contentNamespace = reader.NamespaceURI;

        input.Position = 0;
        return contentNamespace is "http://www.topografix.com/GPX/1/0" or "http://www.topografix.com/GPX/1/1";
    }
}
