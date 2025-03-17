using CommentTranslator22.Popups.Config;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.ComponentModel.Design;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace CommentTranslator22.Popups.Command
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class Command1
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("6eacd00d-344f-45d8-81e6-7d484b6df6d3");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="Command1"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private Command1(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static Command1 Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in Command1's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new Command1(package, commandService);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            //_ = ThreadHelper.JoinableTaskFactory.RunAsync(async delegate
            //{
            //    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            //    var dte = await ServiceProvider.GetServiceAsync(typeof(DTE)) as DTE2;
            //    if (dte.ActiveDocument != null)
            //    {
            //        var select = dte.ActiveDocument.Selection as TextSelection;
            //        if (string.IsNullOrEmpty(select.Text))
            //        {
            //            //select.SelectLine(); // 如果没有选择的文本就选择行
            //            return;
            //        }

            //        // 在这里应该进行修剪行文本

            //        var text = select.Text;
            //        if (string.IsNullOrEmpty(text))
            //        {
            //            return; // 如果执行到这里，文本还是为空
            //        }
            //        var view = await GetActiveWpfTextViewAsync();
            //        var span = view.Selection.SelectedSpans[0];
            //        var cview = TestAdornmentLayer.GetView<Command1View>(view, span);
            //        if (cview != default)
            //        {
            //            cview.TranslateText(text);
            //        }
            //    }
            //});

            new ConfigWindow().Show();
        }

        async Task<IWpfTextView> GetActiveWpfTextViewAsync()
        {
            var t1 = await ServiceProvider.GetServiceAsync(typeof(SVsTextManager)) as IVsTextManager;
            var t2 = await ServiceProvider.GetServiceAsync(typeof(SComponentModel)) as IComponentModel;
            var t3 = t2.GetService<IVsEditorAdaptersFactoryService>();

            t1.GetActiveView(1, null, out var ppView);
            return t3.GetWpfTextView(ppView);
        }
    }
}
