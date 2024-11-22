using AzureAITranslatorService.Models;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell.Interop;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
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

            var sourceLanguageEntries = ResxFileReader.ReadResxFile(sourceLanguageFile);
            var targetLanguageEntriesDict = new Dictionary<string, OrderedDictionary>();

            foreach (var file in files)
            {
                if (file != sourceLanguageFile)
                {
                    var entries = ResxFileReader.ReadResxFile(file);
                    var language = ExtractLanguageFromFileName(file);
                    targetLanguageEntriesDict[language] = entries;
                }
            }

            var translationEntriesList = CreateTranslationEntries(sourceLanguageEntries, targetLanguageEntriesDict);

            WriteToExcel(translationEntriesList, targetDirectoryPath);
        }

        private static string ExtractLanguageFromFileName(string fileName)
        {
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            var match = Regex.Match(fileNameWithoutExtension, Constants.LanguageCodePattern);
            if (match.Success)
            {
                return match.Value.TrimStart('.');
            }
            else
            {
                return "other";
            }
        }

        private static List<TranslationEntry> CreateTranslationEntries(OrderedDictionary sourceLanguageEntries, Dictionary<string, OrderedDictionary> targetLanguageEntriesDict)
        {
            var translationEntriesList = new List<TranslationEntry>();

            foreach (DictionaryEntry sourceEntry in sourceLanguageEntries)
            {
                var name = (string)sourceEntry.Key;
                var (sourceValue, sourceComment) = ((string value, string comment))sourceEntry.Value;

                foreach (var targetLanguage in targetLanguageEntriesDict.Keys)
                {
                    var targetEntries = targetLanguageEntriesDict[targetLanguage];
                    if (targetEntries.Contains(name))
                    {
                        var (targetValue, targetComment) = ((string value, string comment))targetEntries[name];
                        translationEntriesList.Add(new TranslationEntry
                        {
                            Name = name,
                            SourceLanguage = sourceValue,
                            TargetLanguage = targetValue,
                            Comment = targetComment,
                            TargetLanguageCode = targetLanguage
                        });
                    }
                    else
                    {
                        translationEntriesList.Add(new TranslationEntry
                        {
                            Name = name,
                            SourceLanguage = sourceValue,
                            TargetLanguage = string.Empty, // No translation for this value in the files
                            Comment = "Missing from Sync",
                            TargetLanguageCode = targetLanguage
                        });
                    }
                }
            }

            return translationEntriesList;
        }

        private static void WriteToExcel(List<TranslationEntry> translationEntriesList, string targetDirectoryPath)
        {
            var filePath = Path.Combine(targetDirectoryPath, "Translations.xlsx");

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                // Group the entries by target language code
                var groupedEntries = translationEntriesList.GroupBy(te => te.TargetLanguageCode);

                foreach (var group in groupedEntries)
                {
                    var languageCode = group.Key;
                    var worksheet = package.Workbook.Worksheets.Add(languageCode);

                    // Write headers
                    worksheet.Cells[1, 1].Value = "Name";
                    worksheet.Cells[1, 2].Value = "SourceLanguage";
                    worksheet.Cells[1, 3].Value = "TargetLanguage";
                    worksheet.Cells[1, 4].Value = "Comment";

                    using (var range = worksheet.Cells[1, 1, 1, 4])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    }

                    // Write data
                    var entries = group.ToList();
                    for (int i = 0; i < entries.Count(); i++)
                    {
                        var entry = entries[i];
                        worksheet.Cells[i + 2, 1].Value = entry.Name;
                        worksheet.Cells[i + 2, 2].Value = entry.SourceLanguage;
                        worksheet.Cells[i + 2, 3].Value = entry.TargetLanguage;
                        worksheet.Cells[i + 2, 4].Value = entry.Comment;
                    }

                    worksheet.Cells.AutoFitColumns();
                }

                var file = new FileInfo(filePath);
                package.SaveAs(file);
            }
        }

        public static string FindSourceLanguageFile(string[] files)
        {
            foreach (var file in files)
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);
                string pattern = Constants.regexPathPattern;

                if (!Regex.IsMatch(fileNameWithoutExtension, pattern))
                {
                    return file;
                }
            }

            return null; // Return null if no source file is found
        }
    }
}
