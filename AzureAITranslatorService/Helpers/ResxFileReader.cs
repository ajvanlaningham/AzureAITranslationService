using System.Collections.Specialized;
using System.Xml.Linq;

public class ResxFileReader
{
    public ResxFileReader()
    {
    }
    public static OrderedDictionary ReadResxFile(string resxPath)
    {
        var entries = new OrderedDictionary();
        var xdoc = XDocument.Load(resxPath);

        foreach (var data in xdoc.Root.Elements("data"))
        {
            var name = data.Attribute("name")?.Value;
            var value = data.Element("value")?.Value ?? string.Empty;
            var comment = data.Element("comment")?.Value ?? string.Empty;
            if (name != null)
            {
                entries[name] = (value, comment);
            }
        }

        return entries;
    }
}