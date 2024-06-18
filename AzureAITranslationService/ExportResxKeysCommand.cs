using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace ResxExporter
{
    internal sealed class ExportResxKeysCommand
    {
        public const int CommandId = 0x0100;
        public static readonly Guid CommandSet = new Guid("your-guid-here");

        private readonly AsyncPackage package;

        private ExportResxKeysCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));

            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        public static ExportResxKeysCommand Instance { get; private set; }

        private IAsyncServiceProvider ServiceProvider => this.package;

        public static async Task InitializeAsync(AsyncPackage package)
        {
            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new ExportResxKeysCommand(package, commandService);
        }

        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            // Do nothing for now
        }
    }
}