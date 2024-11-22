using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AzureAITranslatorService.Services;

namespace AzureAITranslatorService
{
    internal sealed class TranslateResourcesCommand
    {
        public const int CommandId = 4147;
        public static readonly Guid CommandSet = new Guid("7250f60b-f2f2-4bef-b4b3-b5ef7dbbc866");

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
            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                var selectedItem = GetSelectedItem();
                if (selectedItem == null) return;

                string resxFilePath = selectedItem;
                string directory = Path.GetDirectoryName(resxFilePath);
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(resxFilePath);

                try
                {
                    string targetLanguageCode = ExtractLanguageCodeFromFileName(resxFilePath);

                    string sourceLanguageCode = "en"; 

                    string targetFilePath = GetTargetFilePath(resxFilePath, targetLanguageCode);

                    await translationService.TranslateFileAsync(resxFilePath, targetFilePath, targetLanguageCode, sourceLanguageCode);

                    VsShellUtilities.ShowMessageBox(
                        this.package,
                        $"Successfully translated {Path.GetFileName(resxFilePath)} to {targetLanguageCode}.",
                        "Translation Complete",
                        OLEMSGICON.OLEMSGICON_INFO,
                        OLEMSGBUTTON.OLEMSGBUTTON_OK,
                        OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                }
                catch (Exception ex)
                {
                    VsShellUtilities.ShowMessageBox(
                        this.package,
                        $"Failed to translate resx file. Error: {ex.Message}",
                        "Error",
                        OLEMSGICON.OLEMSGICON_CRITICAL,
                        OLEMSGBUTTON.OLEMSGBUTTON_OK,
                        OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
                }
            }).FileAndForget("AzureAITranslatorService.TranslateCommand");
        }

        private string GetTargetFilePath(string sourceFilePath, string targetLanguageCode)
        {
            string directory = Path.GetDirectoryName(sourceFilePath);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(sourceFilePath);
            string baseFileName = GetBaseFileName(fileNameWithoutExtension);

            string targetFileName = $"{baseFileName}.{targetLanguageCode}.resx";
            return Path.Combine(directory, targetFileName);
        }


        private string GetBaseFileName(string fileNameWithoutExtension)
        {
            var match = Regex.Match(fileNameWithoutExtension, Constants.BaseFileNamePattern);
            return match.Groups["baseName"].Value;
        }


        private string ExtractLanguageCodeFromFileName(string filePath)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            var match = Regex.Match(fileName, Constants.LanguageCodePattern);
            if (match.Success)
            {
                return match.Value.TrimStart('.');
            }
            else
            {
                throw new InvalidOperationException("Target language code not found in the file name.");
            }
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
