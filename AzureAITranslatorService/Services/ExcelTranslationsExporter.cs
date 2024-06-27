using AzureAITranslatorService.Models;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
            string sourceLanguageFile = FindSourceLanguageFile(files);
            if (sourceLanguageFile == null)
            {
                throw new InvalidOperationException("Source Language file not found");
            }

            var sourceLanguageEntries  = ResxFileReader.ReadResxFile(sourceLanguageFile);
            var targetLanguageEntriesList = new List<OrderedDictionary>();

            foreach (var file in files)
            {
                if (file != sourceLanguageFile)
                {
                    var entries = ResxFileReader.ReadResxFile(file);
                    targetLanguageEntriesList.Add(entries);
                }
            }

            // flatten the targetLanguageEntriesList to a dictionary, or Imma be stuck in a mess of foreach loops
            var targetEntries = new Dictionary<string, (string language, string value, string comment)>();

            foreach (var targetEntriesDict in targetLanguageEntriesList)
            {
                foreach (DictionaryEntry entry in targetEntriesDict) //kinda like this
                {
                    var (value, comment) = ((string value, string comment))entry.Value;
                    var key = (string)entry.Key;
                    var language = Path.GetFileNameWithoutExtension(files.First(file => targetEntriesDict == ResxFileReader.ReadResxFile(file))).Split('-').Last(); //lol

                    if (!targetEntries.ContainsKey(key))
                    {
                        targetEntries[key] = (language, value, comment);
                    }
                }
            }

            var  translationEntries = new List<TranslationEntry>();

            foreach (DictionaryEntry sourceEntry in sourceLanguageEntries)
            {
                var name = (string)sourceEntry.Key;
                var (sourceValue, sourceComment) = ((string value, string comment))sourceEntry.Value;

                if (sourceComment.ToLower() != "no translation")
                {
                    if (targetEntries.TryGetValue(name, out var entries))
                    {
                        var (targetLanguage, targetValue, targetComment) = entries;

                        translationEntries.Add(new TranslationEntry
                        {
                            Name = name,
                            SourceLanguage = sourceValue,
                            TargetLanguage = targetValue,
                            Comment = sourceComment
                        });
                    }
                    else
                    {
                        translationEntries.Add(new TranslationEntry
                        {
                            Name = name,
                            SourceLanguage = sourceValue,
                            TargetLanguage = string.Empty, //there was no translation for this value in the files...
                            Comment = sourceComment
                        });
                    }
                }
            }

            WriteToExcel(translationEntries);
        }

        private static void WriteToExcel(List<TranslationEntry> translationEntries)
        {
            // TODO: Implement the logic to write translationEntries to an Excel file
        }

        public static string FindSourceLanguageFile(string[] files)
        {
            foreach (var file in files)
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);
                string pattern = @"-[a-zA-Z]{2}$";

                if (!Regex.IsMatch(fileNameWithoutExtension, pattern))
                {
                    return file;
                }
            }

            return null; // Return null if no source file is found
        }
    }
}
