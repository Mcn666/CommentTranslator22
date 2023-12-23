using CommentTranslator22.Config;
using CommentTranslator22.Translate;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace CommentTranslator22
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    [ProvideAutoLoad(UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(CommentTranslator22Package.PackageGuidString)]
    [ProvideOptionPage(typeof(ConfigA), "CommentTranslator22", "常规", 0, 0, true)]
    [ProvideOptionPage(typeof(ConfigB), "CommentTranslator22", "高级", 0, 0, true)]
    public sealed class CommentTranslator22Package : AsyncPackage
    {
        /// <summary>
        /// CommentTranslator22Package GUID string.
        /// </summary>
        public const string PackageGuidString = "3ce0a949-a32a-4108-9dd1-9103ff35e40c";

        public static ConfigA ConfigA { get; set; } = new ConfigA();
        public static ConfigB ConfigB { get; set; } = new ConfigB();

        public static TranslateClient TranslateClient { get; set; } = new TranslateClient();

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            // 加载配置信息
            ConfigA.ReloadSetting((ConfigA)GetDialogPage(typeof(ConfigA)));
            ConfigB.ReloadSetting((ConfigB)GetDialogPage(typeof(ConfigB)));

        }

        #endregion
    }
}
