using RouteQualityTracker.Core.Models;
using System.Xml.Linq;

namespace RouteQualityTracker.Core.Gpx;

public class GpxTrack : IGpxObject
{
    private readonly XElement _gpxElement;
    private readonly XNamespace _gpxNamespace;

    public IList<GpxWaypoint> WayPoints
    {
        get => _gpxElement.XPathSelectElements<GpxWaypoint>("//trkpt", _gpxNamespace);
        set
        {
            var oldWayPoints = _gpxElement.XPathSelectElements<GpxWaypoint>("//trkpt", _gpxNamespace);
            oldWayPoints?.ForEach(w => w.RemoveFromParent());

            //var trackSegment = _gpxElement.XPathSelectElement("trkseg", _gpxNamespace);
            //if (trackSegment is null)
            //{
            //}

            value.ToList().ForEach(t => _gpxElement.Add(t.ToXElement()));
        }
    } 

    public RouteQualityEnum? TrackQuality
    {
        get
        {
            var colorElement = _gpxElement.XPathSelectElement("extensions/color", _gpxNamespace);
            var colorValue = colorElement?.Value;

            if (colorValue is null)
            {
                return null;
            }

            return colorValue switch
            {
                TrackColor.Bad => RouteQualityEnum.Bad,
                TrackColor.Standard => RouteQualityEnum.Standard,
                TrackColor.Good => RouteQualityEnum.Good,
                _ => RouteQualityEnum.Unknown
            };
        }
        set
        {
            var colorElement = _gpxElement.XPathSelectElement("extensions/color", _gpxNamespace);

            if (value is null or RouteQualityEnum.Unknown)
            {
                colorElement?.Remove();
                return;
            }

            if (colorElement is null)
            {
                var extensionsElement = _gpxElement.Element("extensions", _gpxNamespace);
                if (extensionsElement is null)
                {
                    extensionsElement = new XElement(_gpxNamespace + "extensions");
                    _gpxElement.Add(extensionsElement);
                }

                colorElement = new XElement(_gpxNamespace + "color");
                extensionsElement.Add(colorElement);
            }

            colorElement.Value = value switch
            {
                RouteQualityEnum.Bad => TrackColor.Bad,
                RouteQualityEnum.Standard => TrackColor.Standard,
                RouteQualityEnum.Good => TrackColor.Good,
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
            };
        }
    }

    public GpxTrack(XElement node, XNamespace gpxNamespace)
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
        var gpxClone = new XElement(_gpxElement);
        
        return new GpxTrack(gpxClone, _gpxNamespace)
        {
            WayPoints = new List<GpxWaypoint>()
        };
    }
}
