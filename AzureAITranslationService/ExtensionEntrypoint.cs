using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.Extensibility;
using ResxExporter; // Add this using directive to reference the ExportResxKeysCommand class

namespace AzureAITranslationService
{
    /// <summary>
    /// Extension entrypoint for the VisualStudio.Extensibility extension.
    /// </summary>
    [VisualStudioContribution]
    internal class ExtensionEntrypoint : Extension
    {
        /// <inheritdoc/>
        public override ExtensionConfiguration ExtensionConfiguration => new()
        {
            Metadata = new(
                    id: "AzureAITranslationService.03784632-69f8-4bdb-a54c-9e434e050a92",
                    version: this.ExtensionAssemblyVersion,
                    publisherName: "Publisher name",
                    displayName: "AzureAITranslationService",
                    description: "Extension description"),
        };

        /// <inheritdoc />
        protected override void InitializeServices(IServiceCollection serviceCollection)
        {
            base.InitializeServices(serviceCollection);

            // You can configure dependency injection here by adding services to the serviceCollection.

            // Initialize the ExportResxKeysCommand
            serviceCollection.AddSingleton<ExportResxKeysCommand>();
        }
    }
}