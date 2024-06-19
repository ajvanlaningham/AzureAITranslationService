using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace AzureAITranslatorService
{
    internal sealed class SynchronizeLanguageFilesCommand
    {
        public const int CommandId = 0x0101;
        public static readonly Guid CommandSet = new Guid("7250f60b-f2f2-4bef-b4b3-b5ef7dbbc866");

        private readonly AsyncPackage package;

        private SynchronizeLanguageFilesCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new OleMenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        public static SynchronizeLanguageFilesCommand Instance { get; private set; }

        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider => this.package;

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new SynchronizeLanguageFilesCommand(package, commandService);
        }

        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var selectedItem = GetSelectedItem();
            if (selectedItem == null) return;

            string resxFilePath = selectedItem;
            string targetLanguageFilePath = Path.ChangeExtension(resxFilePath, null) + "-fr.resx"; // Change the language code as needed

            try
            {
                ResxSynchronizer.SynchronizeResxFiles(resxFilePath, targetLanguageFilePath);

                VsShellUtilities.ShowMessageBox(
                    this.package,
                    $"Successfully synchronized resx files: {resxFilePath} and {targetLanguageFilePath}",
                    "Synchronization Complete",
                    OLEMSGICON.OLEMSGICON_INFO,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            }
            catch (Exception ex)
            {
                VsShellUtilities.ShowMessageBox(
                    this.package,
                    $"Failed to synchronize resx files. Error: {ex.Message}",
                    "Error",
                    OLEMSGICON.OLEMSGICON_CRITICAL,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
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
