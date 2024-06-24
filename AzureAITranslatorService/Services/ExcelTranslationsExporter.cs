using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureAITranslatorService.Services
{
    public static class ExcelTranslationsExporter
    {
        static ExcelTranslationsExporter()
        {

        }

        public static void ExportTranslations(string targetDirectoryPath, string[] files)
        {
            var allEntries = new List<OrderedDictionary>();

            foreach (var file in files) //this aint right, but I gotta go
            {
                var entries = ResxFileReader.ReadResxFile(file);
                allEntries.Add(entries);
            }
        }
    }
}
