using System.Xml;
using System.Xml.Linq;

namespace RouteQualityTracker.Core.Gpx;

public class GpxWaypoint
{
    private readonly XElement _gpxElement;
    private readonly string _gpxNamespace;

    public DateTimeOffset? TimeUtc
    {
        get
        {
            var timeElement = _gpxElement.GpxElement("time", _gpxNamespace);

            if (DateTimeOffset.TryParse(timeElement?.Value ?? string.Empty, out var time))
            {
                return time;
            }
            return null;
        }
    }

    public GpxWaypoint(XElement node, string gpxNamespace)
    {
        _gpxElement = node;
        _gpxNamespace = gpxNamespace;
    }
}
