using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace AzureAITranslatorService.Services.Implementations
{
    internal class SecretsService : ISecretsService
    {
        private readonly IConfiguration configuration;

        public SecretsService(IConfiguration configuration)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public SecretsService()
        {
            Console.WriteLine($"Loading embedded configuration file...");

            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"{assembly.GetName().Name}.appsettings.local.json";

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                var json = reader.ReadToEnd();
                var builder = new ConfigurationBuilder()
                    .AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(json)));

                configuration = builder.Build();
            }
        }

        public string TranslatorEndpoint => GetRequiredConfigValue("TranslatorService:Endpoint");
        public string TranslatorKey => GetRequiredConfigValue("TranslatorService:Key");
        public string TranslatorRegion => GetRequiredConfigValue("TranslatorService:Region");

        private string GetRequiredConfigValue(string key)
        {
            var value = configuration[key];
            if (string.IsNullOrEmpty(value))
            {
                throw new InvalidOperationException($"Configuration value for '{key}' is missing or empty.");
            }
            return value;
        }
    }
}
