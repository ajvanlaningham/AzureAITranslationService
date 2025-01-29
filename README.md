# ResX Translator VSIX Extension

A **Visual Studio extension** designed to streamline the translation of `.resx` resource files by leveraging **Azure AI translation services**. This extension helps developers efficiently manage and translate localization resources within Visual Studio.

---

## Features
✅ **Export Resx Data** – Extracts **keys, values, and comments** from `.resx` files into an Excel file for easy review and translation.  
✅ **Azure AI Integration** – Connects to an **Azure AI translation service** to automate translations.  
✅ **Customizable Translation Source** – Users can configure their own **Azure AI instance** for improved flexibility.  
✅ **Batch Processing** – Enables bulk translations of multiple `.resx` files.  
✅ **Seamless Integration** – Works directly within **Visual Studio** for a smooth developer experience.  

---

## Installation
1. Clone this repository:  
   ```sh
   git clone https://github.com/ajvanlaningham/AzureAITranslationService.git
   cd AzureAITranslationService
   ```
2. Open the project in **Visual Studio**.  
3. Build the solution to generate the `.vsix` file.  
4. Install the extension by double-clicking the `.vsix` file.  
5. Restart **Visual Studio** to apply changes.  

---

## Usage
1. Open a Visual Studio project containing `.resx` files.  
2. Use the **"ResX Translator"** menu option to:  
   - Export `.resx` data to an Excel file.  
   - Translate `.resx` values using **Azure AI**.  
3. Review and modify translations in Excel if necessary.  
4. Re-import the translated `.resx` file back into the project.  

---

## Configuration
To use **Azure AI translations**, configure your Azure AI credentials in `settings.json` or through the extension's settings panel [settings panel WIP].  

```json
{
  "AzureAIEndpoint": "https://your-azure-ai-endpoint.cognitiveservices.azure.com/",
  "SubscriptionKey": "your-subscription-key"
}
```

---

## Dependencies
- **Visual Studio SDK**  
- **EPPlus** (for Excel file handling)  
- **Azure AI Translator SDK**  

---

## Roadmap
🚀 **Planned Features:**
- Setting panel UI
- Support for additional translation providers (Google Translate, DeepL).  
- Direct in-editor translation preview.  
- More advanced filtering for `.resx` keys.  

---

## Contributing
Probably don't! There are existing visual studio extensions that handle multi-lingual support that are done better. But I didn't like them and I have a problem with authority  

---

## License
📜 MIT License – Feel free to use, modify, and share this project.  
