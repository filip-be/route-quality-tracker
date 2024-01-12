using System.Xml.Linq;

namespace RouteQualityTracker.Core.Gpx;

public class GpxWaypoint : IGpxObject
{
    private readonly XElement _gpxElement;
    private readonly XNamespace _gpxNamespace;

    public DateTimeOffset? TimeUtc
    {
        get
        {
            var timeElement = _gpxElement.Element("time", _gpxNamespace);

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

    public void RemoveFromParent()
    {
        _gpxElement.Remove();
    }

    public XElement ToXElement()
    {
        return _gpxElement;
    }
}
