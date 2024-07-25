//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.CSharp;
//using Microsoft.CodeAnalysis.CSharp.Syntax;
//using Microsoft.CodeAnalysis.Text;
//using Microsoft.VisualStudio.Text;
//using System;
//using System.Threading;
//using System.Threading.Tasks;

//namespace CommentTranslator22.Popups
//{
//    internal class SemanticContext
//    {
//        SyntaxModel _Model = SyntaxModel.Empty;

//        public Document Document => _Model.Document;
//        public SemanticModel SemanticModel => _Model.SemanticModel;

//        public Task<bool> UpdateAsync(ITextBuffer buffer, CancellationToken token)
//        {
//            var textContainer = buffer.AsTextContainer();
//            if (Workspace.TryGetWorkspace(textContainer, out var workspace))
//            {
//                var id = workspace.GetDocumentIdInCurrentContext(textContainer);
//                if ((id is null) == false && workspace.CurrentSolution.ContainsDocument(id))
//                {
//                    var doc = workspace.CurrentSolution.WithDocumentText(id, textContainer.CurrentText, PreservationMode.PreserveIdentity).GetDocument(id);
//                    return UpdateAsync(buffer, doc, workspace, token);
//                }
//            }
//            return Task.FromResult(false);
//        }

//        async Task<bool> UpdateAsync(ITextBuffer buffer, Document doc, Workspace workspace, CancellationToken token)
//        {
//            try
//            {
//                SyntaxModel m = _Model;
//                var ver = await doc.GetSyntaxVersionAsync(token).ConfigureAwait(false);
//                if (doc == Document && ver == m.Version)
//                {
//                    return true;
//                }
//                SemanticModel model = await doc.GetSemanticModelAsync(token).ConfigureAwait(false);
//                _Model = new SyntaxModel()
//                {
//                    Workspace = workspace,
//                    Document = doc,
//                    SourceBuffer = buffer,
//                    SemanticModel = model,
//                    Compilation = model.SyntaxTree.GetCompilationUnitRoot(token),
//                    Version = ver
//                };
//                return true;
//            }
//            catch (NullReferenceException)
//            {
//                System.Diagnostics.Debug.WriteLine("Update semantic context failed.");
//            }
//            return false;
//        }

//        class SyntaxModel
//        {
//            internal static SyntaxModel Empty = new SyntaxModel();
//            public ITextBuffer SourceBuffer { get; set; }
//            public Document Document { get; set; }
//            public Workspace Workspace { get; set; }
//            public SemanticModel SemanticModel { get; set; }
//            public CompilationUnitSyntax Compilation { get; set; }
//            public VersionStamp Version { get; set; }
//        }
//    }
//}
