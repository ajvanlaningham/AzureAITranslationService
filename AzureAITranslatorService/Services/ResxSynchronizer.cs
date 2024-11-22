using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Xml.Linq;

public static class ResxSynchronizer
{
    private static readonly string ResxSchema;

    static ResxSynchronizer()
    {
        try
        {
            string filePath = Constants.ResxSchemaPath;

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Schema file not found: {filePath}");
            }

            ResxSchema = File.ReadAllText(filePath);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to initialize ResxSynchronizer. Error reading schema file: {Constants.ResxSchemaPath}.", ex);
        }
    }

    public static void SynchronizeResxFiles(string sourceResxPath, string targetResxPath)
    {
        try
        {
            var sourceEntries = ResxFileReader.ReadResxFile(sourceResxPath);
            var targetEntries = ResxFileReader.ReadResxFile(targetResxPath);

            var updatedTargetEntries = new OrderedDictionary();

            // Add existing target entries first
            foreach (DictionaryEntry entry in targetEntries)
            {
                updatedTargetEntries[entry.Key] = entry.Value;
            }

            // Add new entries from source
            foreach (DictionaryEntry entry in sourceEntries)
            {
                if (!targetEntries.Contains(entry.Key))
                {
                    var valueTuple = ((string Value, string Comment))entry.Value;
                    string targetComment = valueTuple.Comment.ToLower() == "no translation" ? valueTuple.Comment : "New";
                    
                    updatedTargetEntries[entry.Key] = (valueTuple.Value, targetComment);
                }
            }

            WriteResxFile(targetResxPath, updatedTargetEntries);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to synchronize Resx files. Error: {ex.Message}", ex);
        }
    }


    public static void WriteResxFile(string resxPath, OrderedDictionary entries)
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
        
        foreach (DictionaryEntry entry in entries)
        {
            var dataElement = new XElement("data",
                new XAttribute("name", (string)entry.Key),
                new XAttribute(XNamespace.Xml + "space", "preserve"),
                new XElement("value", ((ValueTuple<string, string>)entry.Value).Item1));

            if (!string.IsNullOrEmpty(((ValueTuple<string, string>)entry.Value).Item2))
            {
                dataElement.Add(new XElement("comment", ((ValueTuple<string, string>)entry.Value).Item2));
            }

            doc.Root.Add(dataElement);
        }

        doc.Save(resxPath);
    }
}