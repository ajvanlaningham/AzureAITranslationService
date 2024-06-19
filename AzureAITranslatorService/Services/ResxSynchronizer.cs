using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using System.Xml.Linq;

public static class ResxSynchronizer
{
    private static readonly string ResxSchema;

    static ResxSynchronizer()
    {
        ResxSchema = File.ReadAllText(Constants.ResxSchemaPath);
    }

    public static void SynchronizeResxFiles(string sourceResxPath, string targetResxPath)
    {
        var sourceEntries = ReadResxFile(sourceResxPath);
        var targetEntries = ReadResxFile(targetResxPath);

        var allKeys = new HashSet<string>(sourceEntries.Keys.Concat(targetEntries.Keys));

        var updatedTargetEntries = new Dictionary<string, (string Value, string Comment)>();
        foreach (var key in allKeys)
        {
            if (targetEntries.ContainsKey(key))
            {
                updatedTargetEntries[key] = targetEntries[key];
            }
            else if (sourceEntries.ContainsKey(key))
            {
                updatedTargetEntries[key] = sourceEntries[key];
            }
            else
            {
                updatedTargetEntries[key] = (string.Empty, string.Empty); // Add empty value and comment for missing keys
            }
        }

        WriteResxFile(targetResxPath, updatedTargetEntries);
    }

    private static Dictionary<string, (string Value, string Comment)> ReadResxFile(string resxPath)
    {
        var entries = new Dictionary<string, (string Value, string Comment)>();
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

    private static void WriteResxFile(string resxPath, Dictionary<string, (string Value, string Comment)> entries)
    {
        var doc = new XDocument(
            new XElement("root",
                new XComment(ResxSchema),
                new XElement("resheader",
                    new XAttribute("name", "resmimetype"),
                    new XElement("value", "text/microsoft-resx")),
                new XElement("resheader",
                    new XAttribute("name", "version"),
                    new XElement("value", "2.0")),
                new XElement("resheader",
                    new XAttribute("name", "reader"),
                    new XElement("value", "System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")),
                new XElement("resheader",
                    new XAttribute("name", "writer"),
                    new XElement("value", "System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"))
            )
        );

        foreach (var entry in entries)
        {
            var dataElement = new XElement("data",
                new XAttribute("name", entry.Key),
                new XAttribute(XNamespace.Xml + "space", "preserve"),
                new XElement("value", entry.Value.Value));

            if (!string.IsNullOrEmpty(entry.Value.Comment))
            {
                dataElement.Add(new XElement("comment", entry.Value.Comment));
            }

            doc.Root.Add(dataElement);
        }

        doc.Save(resxPath);
    }
}
