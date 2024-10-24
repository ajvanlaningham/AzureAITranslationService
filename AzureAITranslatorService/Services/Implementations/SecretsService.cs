using Microsoft.Extensions.Configuration;
using System.IO;

namespace AzureAITranslatorService.Services.Implementations
{
    internal class SecretsService : ISecretsService
    {
        private readonly IConfiguration configuration;

        public SecretsService()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true);
            configuration = builder.Build();
        }

        public string TranslatorEndpoint => configuration["TranslatorService:Endpoint"];
        public string TranslatorKey => configuration["TranslatorService:Key"];
        public string TranslatorRegion => configuration["TranslatorService:Region"];
    }
}
