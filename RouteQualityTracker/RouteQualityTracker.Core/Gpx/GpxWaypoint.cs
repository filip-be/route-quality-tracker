using System.Xml.Linq;

namespace RouteQualityTracker.Core.Gpx;

public class GpxWaypoint
{
    private XElement _gpxElement;

    public DateTimeOffset? TimeUtc
    {
        get
        {
            var timeElement = _gpxElement.Element("time");

            if (DateTimeOffset.TryParse(timeElement?.Value ?? string.Empty, out var time))
            {
                return time;
            }
            return null;
        }
    }

    public GpxWaypoint(XElement node)
    {
        _gpxElement = node;
    }
}
