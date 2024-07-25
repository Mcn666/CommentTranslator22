using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace CommentTranslator22.Popups
{
    [Export(typeof(IWpfTextViewCreationListener))]
    [Name(nameof(TestTextViewCreationListener))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal class TestTextViewCreationListener : IWpfTextViewCreationListener
    {
        // 加载的顺序很重要，否则UI控件会被遮挡
        [Export(typeof(AdornmentLayerDefinition))]
        [Name(nameof(TestAdornmentLayer))]
        [Order(After = PredefinedAdornmentLayers.BlockStructure)]
        [Order(After = PredefinedAdornmentLayers.BraceCompletion)]
        [Order(After = PredefinedAdornmentLayers.Caret)]
        [Order(After = PredefinedAdornmentLayers.CurrentLineHighlighter)]
        [Order(After = PredefinedAdornmentLayers.DifferenceChanges)]
        [Order(After = PredefinedAdornmentLayers.DifferenceSpace)]
        [Order(After = PredefinedAdornmentLayers.DifferenceWordChanges)]
        [Order(After = PredefinedAdornmentLayers.InterLine)]
        [Order(After = PredefinedAdornmentLayers.Outlining)]
        [Order(After = PredefinedAdornmentLayers.Selection)]
        [Order(After = PredefinedAdornmentLayers.Squiggle)]
        [Order(After = PredefinedAdornmentLayers.Text)]
        [Order(After = PredefinedAdornmentLayers.TextMarker)]
        [TextViewRole(PredefinedTextViewRoles.Document)]
        private readonly AdornmentLayerDefinition adornmentLayerDefinition;

        public void TextViewCreated(IWpfTextView view)
        {
            view.Properties.GetOrCreateSingletonProperty(() => new TestAdornmentLayer(view));
        }
    }
}
