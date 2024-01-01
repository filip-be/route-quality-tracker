using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace RouteQualityTracker.Core.Gpx;

public static class GpxExtensions
{
    public const string NamespacePrefix = "gpx";

    public static XElement? GpxElement(this XElement rootElement, string name, string xNamespace)
    {
        XNamespace xmlNamespace = xNamespace;
        return rootElement.Element(xmlNamespace + name);
    }

    public static List<T>? SelectGpxElements<T>(this XElement rootElement, string path, string xNamespace)
    {
        var gpxNamespaceManager = new XmlNamespaceManager(new NameTable());
        gpxNamespaceManager.AddNamespace(NamespacePrefix, xNamespace);

        Console.WriteLine(path);
        var xPath = new StringBuilder();
        path.Split('/').ToList().ForEach(p => 
        {
            Console.WriteLine(p);
            xPath.Append('/');
            if (!string.IsNullOrEmpty(p))
            {
                xPath.Append($"{NamespacePrefix}:{p}");
            }
        });

        Console.WriteLine($"constructed path: {xPath}");
        var nodes = rootElement.XPathSelectElements(xPath.ToString(), gpxNamespaceManager);

        if (nodes is null) return null;

        var elements = new List<T>();
        foreach (var node in nodes)
        {
            var element = (T)Activator.CreateInstance(typeof(T), new object[] { node, xNamespace })!;
            elements.Add(element);
        }
        return elements;
    }

    public static XElement? SelectGpxElement(this XElement rootElement, string path, string xNamespace)
    {
        var gpxNamespaceManager = new XmlNamespaceManager(new NameTable());
        gpxNamespaceManager.AddNamespace(NamespacePrefix, xNamespace);

        return rootElement.XPathSelectElement($"/{NamespacePrefix}:{path}", gpxNamespaceManager);
    }
}
