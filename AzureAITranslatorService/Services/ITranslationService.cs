using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureAITranslatorService.Services
{
    public interface ITranslationService
    {
        Task TranslateFileAsync(string sourceFilePath, string targetFilePath, string targetLanguageCode, string sourceLanguageCode = "en");
    }
}
