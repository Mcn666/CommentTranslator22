using CommentTranslator22.Dictionary;
using CommentTranslator22.Translate.TranslateData;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace CommentTranslator22.Popups
{
    public class TestSolutionEvents : IVsSolutionEvents
    {
        private static TestSolutionEvents solutionEvents;
        private IVsSolution solution;

        public TestSolutionEvents()
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            this.solution = (IVsSolution)Package.GetGlobalService(typeof(SVsSolution));
            this.solution.AdviseSolutionEvents(this, out uint solutionCookie);

            // 现在是在解决方案打开后完成的实例化，需要执行一次OnAfterOpenSolution的代码
            OnAfterOpenSolution(null, 0);
        }

        public static void Create()
        {
            if (solutionEvents == null)
            {
                solutionEvents = new TestSolutionEvents();
            }
        }

        public int OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded) =>
            Microsoft.VisualStudio.VSConstants.S_OK;

        public int OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel) =>
            Microsoft.VisualStudio.VSConstants.S_OK;

        public int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved) =>
            Microsoft.VisualStudio.VSConstants.S_OK;

        public int OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy) =>
            Microsoft.VisualStudio.VSConstants.S_OK;

        public int OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel) =>
            Microsoft.VisualStudio.VSConstants.S_OK;

        public int OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy) =>
            Microsoft.VisualStudio.VSConstants.S_OK;

        public int OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
        {
            // 通知侦听客户端解决方案已打开
            GeneralAnnotationData.Instance.ReadAllData();
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        public int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel) =>
            Microsoft.VisualStudio.VSConstants.S_OK;

        public int OnBeforeCloseSolution(object pUnkReserved) =>
            Microsoft.VisualStudio.VSConstants.S_OK;

        public int OnAfterCloseSolution(object pUnkReserved)
        {
            // 通知侦听客户端解决方案已关闭
            DictionaryUseData.Instance.SaveAllData();
            MethodAnnotationData.Instance.SaveAllData();
            GeneralAnnotationData.Instance.SaveAllData();
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }
    }
}
