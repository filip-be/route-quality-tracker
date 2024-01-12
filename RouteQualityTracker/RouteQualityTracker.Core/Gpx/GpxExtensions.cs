using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace RouteQualityTracker.Core.Gpx;

public static class GpxExtensions
{
    public const string NamespacePrefix = "gpx";

    public static XElement? Element(this XElement rootElement, string name, XNamespace xNamespace)
    {
        return rootElement.Element(xNamespace + name);
    }

    public static List<T> XPathSelectElements<T>(this XElement rootElement, string path, XNamespace xNamespace)
    {
        var gpxNamespaceManager = new XmlNamespaceManager(new NameTable());
        gpxNamespaceManager.AddNamespace(NamespacePrefix, xNamespace.ToString());

        var xPath = EnhancePathWithNamespace(path, xNamespace);
        
        var nodes = rootElement.XPathSelectElements(xPath, gpxNamespaceManager);

        var elements = new List<T>();
        foreach (var node in nodes)
        {
            var element = (T)Activator.CreateInstance(typeof(T), node, xNamespace)!;
            elements.Add(element);
        }
        return elements;
    }

    public static XElement? XPathSelectElement(this XElement rootElement, string path, XNamespace xNamespace)
    {
        var gpxNamespaceManager = new XmlNamespaceManager(new NameTable());
        gpxNamespaceManager.AddNamespace(NamespacePrefix, xNamespace.ToString());

        var xPath = EnhancePathWithNamespace(path, xNamespace);

        return rootElement.XPathSelectElement(xPath, gpxNamespaceManager);
    }

    public static string EnhancePathWithNamespace(string path, XNamespace xNamespace)
    {
        var xPath = new StringBuilder();
        path.Split('/').ToList().ForEach(p =>
        {
            if (!string.IsNullOrEmpty(p))
            {
                xPath.Append($"{NamespacePrefix}:{p}");
            }
            xPath.Append('/');
        });
        var constructedPath = xPath.ToString();
        constructedPath = constructedPath.TrimEnd('/');

        return constructedPath;
    }
}
