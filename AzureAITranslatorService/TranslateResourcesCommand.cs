using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AzureAITranslatorService.Services;

namespace AzureAITranslatorService
{
    internal sealed class TranslateResourcesCommand
    {
        public const int CommandId = 0x4147; // Matches TranslateResourceCommandId
        public static readonly Guid CommandSet = new Guid("8BBB2D0C-6A4F-405D-984B-39DACE1B8D33");

        private readonly AsyncPackage package;
        private readonly ITranslationService translationService;

        private TranslateResourcesCommand(AsyncPackage package, OleMenuCommandService commandService, ITranslationService translationService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            this.translationService = translationService ?? throw new ArgumentNullException(nameof(translationService));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new OleMenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        public static TranslateResourcesCommand Instance { get; private set; }

        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider => this.package;

        public static async Task InitializeAsync(AsyncPackage package, ITranslationService translationService)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new TranslateResourcesCommand(package, commandService, translationService);
            Console.WriteLine(Instance.ToString());
        }

        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var selectedItem = GetSelectedItem();
            if (selectedItem == null) return;

            string resxFilePath = selectedItem;
            string directory = Path.GetDirectoryName(resxFilePath);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(resxFilePath);
            string pattern = $"^{Regex.Escape(fileNameWithoutExtension)}{Constants.regexPathPattern}\\.resx$";

            try
            {
                var regex = new Regex(pattern, RegexOptions.IgnoreCase);
                var files = Directory.GetFiles(directory, "*.resx")
                                     .Where(file => regex.IsMatch(Path.GetFileName(file)))
                                     .ToArray();

                if (files.Length == 0)
                {
                    VsShellUtilities.ShowMessageBox(
                        this.package,
                        $"No matching files found for pattern: {pattern}",
                        "No Files Found",
                        OLEMSGICON.OLEMSGICON_WARNING,
                        OLEMSGBUTTON.OLEMSGBUTTON_OK,
                        OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                    return;
                }

                foreach (var targetLanguageFilePath in files)
                {
                    string targetLanguageCode = ExtractLanguageCodeFromFileName(targetLanguageFilePath);
                    translationService.TranslateFileAsync(resxFilePath, targetLanguageFilePath, targetLanguageCode)
                        .GetAwaiter()
                        .GetResult();
                }

                VsShellUtilities.ShowMessageBox(
                    this.package,
                    $"Successfully translated resx files in directory: {directory}",
                    "Translation Complete",
                    OLEMSGICON.OLEMSGICON_INFO,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            }
            catch (Exception ex)
            {
                VsShellUtilities.ShowMessageBox(
                    this.package,
                    $"Failed to translate resx files. Error: {ex.Message}",
                    "Error",
                    OLEMSGICON.OLEMSGICON_CRITICAL,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            }
        }

        private string ExtractLanguageCodeFromFileName(string filePath)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            var match = Regex.Match(fileName, Constants.regexPathPattern);
            return match.Success ? match.Value.TrimStart('.') : throw new InvalidOperationException("Target language code not found.");
        }

        private string GetSelectedItem()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var monitorSelection = ServiceProvider.GetServiceAsync(typeof(SVsShellMonitorSelection)).Result as IVsMonitorSelection;
            if (monitorSelection == null) return null;

            IVsMultiItemSelect multiItemSelect;
            IntPtr hierarchyPtr, selectionContainerPtr;
            uint itemid;
            monitorSelection.GetCurrentSelection(out hierarchyPtr, out itemid, out multiItemSelect, out selectionContainerPtr);

            if (itemid == VSConstants.VSITEMID_NIL) return null;

            var hierarchy = Marshal.GetObjectForIUnknown(hierarchyPtr) as IVsHierarchy;
            if (hierarchy == null) return null;

            return GetItemFullPath(hierarchy, itemid);
        }

        private string GetItemFullPath(IVsHierarchy hierarchy, uint itemid)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            hierarchy.GetCanonicalName(itemid, out var fullPathObject);
            if (fullPathObject == null)
            {
                VsShellUtilities.ShowMessageBox(
                    this.package,
                    "Failed to get the full path of the selected item.",
                    "Error",
                    OLEMSGICON.OLEMSGICON_CRITICAL,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                return null;
            }

            return fullPathObject;
        }
    }
}
