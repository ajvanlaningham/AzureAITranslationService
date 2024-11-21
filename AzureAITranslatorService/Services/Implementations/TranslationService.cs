using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AzureAITranslatorService.Models;

namespace AzureAITranslatorService.Services.Implementations
{
    public class TranslationService : ITranslationService
    {
        private readonly ISecretsService secretsService;

        public TranslationService(ISecretsService secretsService)
        {
            this.secretsService = secretsService ?? throw new ArgumentNullException(nameof(secretsService));
        }

        public async Task TranslateFileAsync(string sourceFilePath, string targetFilePath, string targetLanguageCode, string sourceLanguageCode = "en")
        {
            // Step 1: Read source Resx file entries
            var sourceEntries = ResxFileReader.ReadResxFile(sourceFilePath);

            // Step 2: Prepare entries for translation
            var entriesToTranslate = new List<(string Name, string Value)>();
            var translatedEntries = new OrderedDictionary();

            foreach (DictionaryEntry entry in sourceEntries)
            {
                var name = (string)entry.Key;
                var (value, comment) = ((string Value, string Comment))entry.Value;

                // Skip translation if the comment indicates "final" or "no translation"
                if (!string.IsNullOrWhiteSpace(comment) &&
                    (comment.Equals("final", StringComparison.OrdinalIgnoreCase) ||
                     comment.Equals("no translation", StringComparison.OrdinalIgnoreCase)))
                {
                    translatedEntries[name] = (value, comment);
                    continue;
                }

                // Collect entries to translate
                if (!string.IsNullOrWhiteSpace(value))
                {
                    entriesToTranslate.Add((name, value));
                }
                else
                {
                    // Preserve empty or untranslated entries
                    translatedEntries[name] = (value, comment);
                }
            }

            // Step 3: Perform batch translation
            if (entriesToTranslate.Count > 0)
            {
                var translatedValues = await TranslateTextBatchAsync(entriesToTranslate.Select(e => e.Value).ToList(), sourceLanguageCode, targetLanguageCode);

                for (int i = 0; i < entriesToTranslate.Count; i++)
                {
                    var (name, _) = entriesToTranslate[i];
                    translatedEntries[name] = (translatedValues[i], "Need Review");
                }
            }

            // Step 4: Write translated entries to target Resx file
            ResxSynchronizer.WriteResxFile(targetFilePath, translatedEntries);
        }

        private async Task<List<string>> TranslateTextBatchAsync(List<string> texts, string sourceLanguage, string targetLanguage)
        {
            var client = new HttpClient();
            string endpoint = $"{secretsService.TranslatorEndpoint}/translate?api-version=3.0&from={sourceLanguage}&to={targetLanguage}";

            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", secretsService.TranslatorKey);
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Region", secretsService.TranslatorRegion);

            var requestBody = texts.Select(text => new { Text = text }).ToArray();
            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await client.PostAsync(endpoint, content);
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Failed to translate text. Error: {response.ReasonPhrase}");
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            var translationResults = JsonSerializer.Deserialize<List<TranslationResult>>(responseBody);

            return translationResults.Select(tr => tr.Translations.FirstOrDefault()?.Text ?? string.Empty).ToList();
        }


        private class TranslationResult
        {
            public List<TranslatedText> Translations { get; set; }
        }

        private class TranslatedText
        {
            public string Text { get; set; }
            public string To { get; set; }
        }
    }
}
