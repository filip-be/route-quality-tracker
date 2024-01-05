using RouteQualityTracker.Core.Models;
using System.Xml.Linq;

namespace RouteQualityTracker.Core.Gpx;

public class GpxTrack : IGpxObject
{
    private readonly XElement _gpxElement;
    private readonly string _gpxNamespace;

    public IList<GpxWaypoint> WayPoints
    {
        get => _gpxElement.XPathSelectElements<GpxWaypoint>("//trkpt", _gpxNamespace) ??
               new List<GpxWaypoint>();
        set
        {
            var oldWayPoints = _gpxElement.XPathSelectElements<GpxWaypoint>("//trkpt", _gpxNamespace);
            oldWayPoints?.ForEach(w => w.RemoveFromParent());

            value.ToList().ForEach(t => _gpxElement.Add(t.ToXElement()));
        }
    } 

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

    public GpxTrack CloneEmptyTrack()
    {
        var gpxClone = XElement.Parse(_gpxElement.ToString());
        
        return new GpxTrack(gpxClone, _gpxNamespace)
        {
            WayPoints = new List<GpxWaypoint>()
        };
    }
}
