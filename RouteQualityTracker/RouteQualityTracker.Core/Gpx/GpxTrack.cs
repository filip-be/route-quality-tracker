using RouteQualityTracker.Core.Models;
using System.Xml.Linq;

namespace RouteQualityTracker.Core.Gpx;

public class GpxTrack : IGpxObject
{
    private readonly XElement _gpxElement;
    private readonly XNamespace _gpxNamespace;
    private readonly XNamespace _styleNamespace = "http://www.topografix.com/GPX/gpx_style/0/2";

    public IList<GpxWaypoint> WayPoints
    {
        get => _gpxElement.XPathSelectElements<GpxWaypoint>("//trkpt", _gpxNamespace);
        set
        {
            var oldWayPoints = _gpxElement.XPathSelectElements<GpxWaypoint>("//trkpt", _gpxNamespace);
            oldWayPoints?.ForEach(w => w.RemoveFromParent());

            var trackSegment = _gpxElement.XPathSelectElement("trkseg", _gpxNamespace);
            if (trackSegment is null)
            {
                trackSegment = new XElement(_gpxNamespace + "trkseg");
                _gpxElement.Add(trackSegment);
            }

            value.ToList().ForEach(t => trackSegment.Add(t.ToXElement()));
        }
    } 

    public RouteQualityEnum? TrackQuality
    {
        get
        {
            var colorElement = GetColorElement();
            var colorValue = colorElement.Value;

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
            var colorElement = GetColorElement();

            if (value is null)
            {
                colorElement.Remove();
                return;
            }

            colorElement.Value = value switch
            {
                RouteQualityEnum.Bad => TrackColor.Bad,
                RouteQualityEnum.Standard => TrackColor.Standard,
                RouteQualityEnum.Good => TrackColor.Good,
                RouteQualityEnum.Unknown => TrackColor.Unknown,
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unsupported track quality")
            };
        }
    }

    public string? Name
    {
        get
        {
            var nameElement = _gpxElement.Element("name", _gpxNamespace);
            return nameElement?.Value;
        }
        set
        {
            var nameElement = _gpxElement.Element("name", _gpxNamespace);

            if (value is null)
            {
                nameElement?.Remove();
                return;
            }

            if (nameElement is null)
            {
                nameElement = new XElement(_gpxNamespace + "name");
                _gpxElement.Add(nameElement);
            }

            nameElement.Value = value;
        }
    }

    private XElement GetExtensionsElement()
    {
        var extensionsElement = _gpxElement.Element("extensions", _gpxNamespace);
        if (extensionsElement is null)
        {
            extensionsElement = new XElement(_gpxNamespace + "extensions");
            _gpxElement.Add(extensionsElement);
        }

        return extensionsElement;
    }

    private XElement GetColorElement()
    {
        var extensionsElement = GetExtensionsElement();

        var lineElement = extensionsElement.Element("line", _styleNamespace);
        if (lineElement is null)
        {
            lineElement = new XElement(_styleNamespace + "line");
            extensionsElement.Add(lineElement);
        }

        var colorElement = lineElement.Element("color", _styleNamespace);
        if (colorElement is null)
        {
            colorElement = new XElement(_styleNamespace + "color");
            lineElement.Add(colorElement);
        }

        return colorElement;
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
