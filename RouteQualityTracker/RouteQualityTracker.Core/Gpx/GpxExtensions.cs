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

    public static List<T>? XPathSelectElements<T>(this XElement rootElement, string path, XNamespace xNamespace)
    {
        var gpxNamespaceManager = new XmlNamespaceManager(new NameTable());
        gpxNamespaceManager.AddNamespace(NamespacePrefix, xNamespace.ToString());

        Console.WriteLine(path);
        var xPath = new StringBuilder();
        path.Split('/').ToList().ForEach(p => 
        {
            Console.WriteLine(p);
            if (!string.IsNullOrEmpty(p))
            {
                xPath.Append($"{NamespacePrefix}:{p}");
            }
            xPath.Append('/');
        });
        var constructedPath = xPath.ToString();
        constructedPath = constructedPath.TrimEnd('/');

        Console.WriteLine($"constructed path: {constructedPath}");
        var nodes = rootElement.XPathSelectElements(constructedPath, gpxNamespaceManager);

        var elements = new List<T>();
        foreach (var node in nodes)
        {
            var element = (T)Activator.CreateInstance(typeof(T), node, xNamespace.ToString())!;
            elements.Add(element);
        }
        return elements;
    }

    public static XElement? XPathSelectElement(this XElement rootElement, string path, XNamespace xNamespace)
    {
        var gpxNamespaceManager = new XmlNamespaceManager(new NameTable());
        gpxNamespaceManager.AddNamespace(NamespacePrefix, xNamespace.ToString());

        return rootElement.XPathSelectElement($"/{NamespacePrefix}:{path}", gpxNamespaceManager);
    }
}
