using System.Xml;
using System.Xml.Linq;

namespace RouteQualityTracker.Core.Gpx;

public class GpxWaypoint
{
    private readonly XElement _gpxElement;
    private readonly XNamespace _gpxNamespace;

    public DateTimeOffset? TimeUtc
    {
        get
        {
            var timeElement = _gpxElement.Element(_gpxNamespace + "time");

            if (DateTimeOffset.TryParse(timeElement?.Value ?? string.Empty, out var time))
            {
                return time;
            }
            return null;
        }
    }

    public GpxWaypoint(XElement node, XNamespace gpxNamespace)
    {
        _gpxElement = node;
        _gpxNamespace = gpxNamespace;
    }
}
