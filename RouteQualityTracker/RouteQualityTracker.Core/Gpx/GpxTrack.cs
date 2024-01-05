using RouteQualityTracker.Core.Models;
using System.Xml.Linq;

namespace RouteQualityTracker.Core.Gpx;

public class GpxTrack
{
    private readonly XElement _gpxElement;
    private readonly string _gpxNamespace;

    public TrackColor? Color
    {
        get
        {
            var colorElement = GpxExtensions.XPathSelectElement(_gpxElement, $"extensions/color", _gpxNamespace);
            var colorValue = colorElement?.Value;
            
            if (Enum.TryParse<TrackColor>(colorValue, true, out var color))
            {
                return color;
            }
            return null;
        }
    }

    public DateTimeOffset? StartTime 
    {   get
        {
            return null;
        }
    }

    public GpxTrack(XElement node, string gpxNamespace)
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
