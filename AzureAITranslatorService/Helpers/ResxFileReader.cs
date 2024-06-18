using AzureAITranslatorService.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Resources;

public static class ResxFileReader
{
    public static List<ResxEntry> ReadResxFile(string resxFilePath)
    {
        var resxEntries = new List<ResxEntry>();

        using (ResXResourceReader resxReader = new ResXResourceReader(resxFilePath))
        {
            resxReader.UseResXDataNodes = true; // This makes the reader return ResXDataNode objects

            foreach (DictionaryEntry entry in resxReader)
            {
                var node = entry.Value as ResXDataNode;
                string comment = node != null ? node.Comment : string.Empty;

                var resxEntry = new ResxEntry
                {
                    Name = entry.Key.ToString(),
                    Value = node != null ? node.GetValue((ITypeResolutionService)null).ToString() : entry.Value.ToString(),
                    Comment = comment
                };

                resxEntries.Add(resxEntry);
            }
        }

        return resxEntries;
    }
}