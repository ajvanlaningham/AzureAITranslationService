using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureAITranslatorService.Services
{
    public interface ISecretsService
    {
        string TranslatorEndpoint { get; }
        string TranslatorKey { get; }
        string TranslatorRegion { get; }
    }
}
