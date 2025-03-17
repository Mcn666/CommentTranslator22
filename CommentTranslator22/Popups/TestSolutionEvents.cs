using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;

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

        public event EventHandler SolutionStart;

        public event EventHandler SolutionClose;

        TestSolutionEvents()
        {
            ThreadHelper.JoinableTaskFactory.Run(async delegate
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                var solution = (IVsSolution)Package.GetGlobalService(typeof(SVsSolution));
                solution.AdviseSolutionEvents(this, out var _);
            });
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
            SolutionStart?.Invoke(this, EventArgs.Empty); // 通知侦听客户端解决方案已打开
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }

        public int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel) =>
            Microsoft.VisualStudio.VSConstants.S_OK;

        public int OnBeforeCloseSolution(object pUnkReserved) =>
            Microsoft.VisualStudio.VSConstants.S_OK;

        public int OnAfterCloseSolution(object pUnkReserved)
        {
            SolutionClose?.Invoke(this, EventArgs.Empty); // 通知侦听客户端解决方案已关闭
            return Microsoft.VisualStudio.VSConstants.S_OK;
        }
    }
}
