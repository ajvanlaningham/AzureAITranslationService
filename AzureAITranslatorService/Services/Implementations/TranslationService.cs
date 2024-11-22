using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Azure;
using AzureAITranslatorService.Models;

namespace AzureAITranslatorService.Services.Implementations
{
    public class TranslationService : ITranslationService
    {
        private readonly ISecretsService secretsService;
        private static readonly HttpClient _client = new HttpClient();


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

            // Step 3: Translate the collected entries
            var translatedValues = await TranslateTextBatchAsync(entriesToTranslate.Select(e => e.Value).ToList(), sourceLanguageCode, targetLanguageCode);

            // Step 4: Map the translations back to the entries
            for (int i = 0; i < entriesToTranslate.Count; i++)
            {
                var (name, _) = entriesToTranslate[i];
                translatedEntries[name] = (translatedValues[i], "Need Review");
            }

            // Step 5: Write the translated entries to the target Resx file
            ResxSynchronizer.WriteResxFile(targetFilePath, translatedEntries);
        }



        private async Task<List<string>> TranslateTextBatchAsync(List<string> texts, string sourceLanguage, string targetLanguage)
        {
            string endpoint = secretsService.TranslatorEndpoint; // Should be 'https://api.cognitive.microsofttranslator.com'
            string route = $"/translate?api-version=3.0&from={sourceLanguage}&to={targetLanguage}";

            var requestBody = texts.Select(text => new { Text = text }).ToArray();
            var jsonRequestBody = JsonSerializer.Serialize(requestBody);

            using (var request = new HttpRequestMessage())
            {
                // Build the request
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(endpoint + route);
                request.Content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", secretsService.TranslatorKey);
                request.Headers.Add("Ocp-Apim-Subscription-Region", secretsService.TranslatorRegion);

                // Send the request and get response
                var response = await _client.SendAsync(request).ConfigureAwait(false);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    throw new InvalidOperationException($"Failed to translate text. Status Code: {response.StatusCode}, Error: {error}");
                }

                // Deserialize the response
                var translationResults = JsonSerializer.Deserialize<List<TranslationResult>>(responseBody);

                // Extract the translated texts
                return translationResults.Select(tr => tr.Translations.FirstOrDefault()?.Text ?? string.Empty).ToList();
            }
        }

        public class TranslationResult
        {
            [JsonPropertyName("translations")]
            public List<Translation> Translations { get; set; }
        }

        public class Translation
        {
            [JsonPropertyName("text")]
            public string Text { get; set; }

            [JsonPropertyName("to")]
            public string To { get; set; }
        }
    }
}
