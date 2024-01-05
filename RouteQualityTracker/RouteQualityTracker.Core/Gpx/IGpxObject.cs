using System.Xml.Linq;

namespace RouteQualityTracker.Core.Gpx;

public interface IGpxObject
{
    void RemoveFromParent();

    XElement ToXElement();
}
