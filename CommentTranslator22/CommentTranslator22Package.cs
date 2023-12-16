using CommentTranslator22.Config;
using CommentTranslator22.Translate;
using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace CommentTranslator22
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(CommentTranslator22Package.PackageGuidString)]
    [ProvideOptionPage(typeof(ConfigA), "CommentTranslator22", "常规", 0, 0, true)]
    [ProvideOptionPage(typeof(ConfigB), "CommentTranslator22", "高级", 0, 0, true)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
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
        }

        #endregion
    }
}
