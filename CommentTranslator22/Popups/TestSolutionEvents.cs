using CommentTranslator22.Dictionary;
using CommentTranslator22.Translate.TranslateData;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace CommentTranslator22.Popups
{
    public class TestSolutionEvents : IVsSolutionEvents
    {
        public static TestSolutionEvents Instance
        {
            get
            {
                return Nested.instance;
            }
        }

        class Nested
        {
            internal static TestSolutionEvents instance = new TestSolutionEvents();

            static Nested() { }
        }

        private readonly IVsSolution solution;

        TestSolutionEvents()
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            solution = (IVsSolution)Package.GetGlobalService(typeof(SVsSolution));
            solution.AdviseSolutionEvents(this, out uint solutionCookie);

            GeneralAnnotationData.Instance.ReadAllData();
        }

        public void Initialize()
        {
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

        public int OnBeforeCloseSolution(object pUnkReserved)
        {
            // 通知侦听客户端解决方案关闭前
            DictionaryUseData.Instance.SaveAllData();
            MethodAnnotationData.Instance.SaveAllData();
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        public int OnAfterCloseSolution(object pUnkReserved)
        {
            // 通知侦听客户端解决方案已关闭
            GeneralAnnotationData.Instance.SaveAllData();
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }
    }
}
