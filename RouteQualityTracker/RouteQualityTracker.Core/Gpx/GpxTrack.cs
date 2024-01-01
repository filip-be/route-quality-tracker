using RouteQualityTracker.Core.Models;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace RouteQualityTracker.Core.Gpx;

public class GpxTrack
{
    private readonly XElement _gpxElement;
    private readonly XNamespace _gpxNamespace;
    private readonly XmlNamespaceManager _gpxNamespaceManager;
    private const string NamespacePrefix = "gpx";

    public TrackColor? Color
    {
        get
        {
            var colorElement = _gpxElement.XPathSelectElement($"/{NamespacePrefix}:extensions/color", _gpxNamespaceManager);
            var colorValue = colorElement?.Value;
            
            if (Enum.TryParse<TrackColor>(colorValue, true, out var color))
            {
                return color;
            };
            return null;
        }
    }

    public GpxTrack(XElement node, XNamespace gpxNamespace)
    {
        _gpxElement = node;
        _gpxNamespace = gpxNamespace;

        _gpxNamespaceManager = new XmlNamespaceManager(new NameTable());
        _gpxNamespaceManager.AddNamespace(NamespacePrefix, gpxNamespace.ToString());
    }
}
