using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureAITranslatorService.Models
{
    public class TranslationEntry
    {
        public string Name { get; set; }
        public string SourceLanguage { get; set; }
        public string TargetLanguage { get; set; }
        public string Comment { get; set; }
    }
}
