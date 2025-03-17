using CommentTranslator22.Popups.CompletionToolTip.View;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace CommentTranslator22.Popups
{
    internal class TestAdornmentLayer
    {
        class ViewState
        {
            public bool IsAdjustPosition { get; set; } = false;
            public bool IsViewVisible { get; set; } = false;
            public object View { get; set; }
        }

        private readonly IWpfTextView view;
        private readonly IAdornmentLayer layer;
        private readonly Dictionary<Type, ViewState> viewStateDictionary = new Dictionary<Type, ViewState>()
        {
            [typeof(CompletionView)] = new ViewState() { View = new CompletionView() },
        };

        public TestAdornmentLayer(IWpfTextView view)
        {
            this.view = view ?? throw new ArgumentNullException(nameof(view));
            this.view.Selection.SelectionChanged += Selection_SelectionChanged;
            this.view.Closed += View_Closed;

            layer = view.GetAdornmentLayer(nameof(TestAdornmentLayer));
            if (layer is UIElement element)
            {
                element.LayoutUpdated += TestAdornmentLayer_LayoutUpdated;
            }
        }

        private void TestAdornmentLayer_LayoutUpdated(object sender, EventArgs e)
        {
            // 此事件用于缓解UI关闭的问题，在布局更新时，总是去检查控件的显示状态
            // 如果有更好的方法，以后会进行更改
            if (view == null)
            {
                return;
            }
            foreach (var vsp in viewStateDictionary)
            {
                if (vsp.Value.IsViewVisible)
                {
                    AddAdornment(vsp.Value.View as UIElement, view.Selection.SelectedSpans[0]);
                    vsp.Value.View.GetType().GetMethod("AdornmentLayerUpdate")?.Invoke(vsp.Value.View, null);
                }
                else if (layer.Elements.Count > 0)
                {
                    RemoveAdornment(vsp.Value.View as UIElement);
                }
            }
        }

        private void View_Closed(object sender, EventArgs e)
        {
            foreach (var vsp in viewStateDictionary)
            {
                vsp.Value.View.GetType().GetMethod("AdornmentLayerClose")?.Invoke(vsp.Value.View, null);
            }
        }

        private void Selection_SelectionChanged(object sender, EventArgs e)
        {
            foreach (var vsp in viewStateDictionary)
            {
                vsp.Value.View.GetType().GetMethod("AdornmentLayerClose")?.Invoke(vsp.Value.View, null);
            }
        }

        public static TestAdornmentLayer GetAdornment(ITextView view)
        {
            try
            {
                return view.Properties.GetProperty<TestAdornmentLayer>(typeof(TestAdornmentLayer));
            }
            catch
            {
                return null;
            }
        }

        public static T GetView<T>(ITextView view, SnapshotSpan span) where T : class
        {
            return GetAdornment(view)?.GetView<T>(span);
        }

        private T GetView<T>(SnapshotSpan span)
        {
            if (viewStateDictionary.TryGetValue(typeof(T), out var vsp))
            {
                EventHandler handler = null;
                handler = (sender, e) =>
                {
                    sender.GetType().GetEvent("OnClosed")?.RemoveEventHandler(sender, handler);
                    RemoveAdornment(sender as UIElement);
                };

                vsp.View.GetType().GetEvent("OnClosed")?.AddEventHandler(vsp.View, handler);
                PopupPosition(vsp.View as UIElement, span);
                AddAdornment(vsp.View as UIElement, span);
                return (T)vsp.View;
            }
            return default;
        }

        private void PopupPosition(UIElement window, SnapshotSpan span)
        {
            var bounds = view.TextViewLines.GetCharacterBounds(span.Start);
            if (bounds != null)
            {
                if (viewStateDictionary.TryGetValue(window.GetType(), out var vsp))
                {
                    if (!vsp.IsAdjustPosition)
                    {
                        Canvas.SetLeft(window, bounds.Left);
                        Canvas.SetTop(window, bounds.Bottom);
                    }
                }
            }
        }

        private void AddAdornment(UIElement window, SnapshotSpan span)
        {
            foreach (var i in layer.Elements)
            {
                if (i.Adornment.GetType() == window.GetType())
                {
                    return;
                }
            }
            layer.AddAdornment(span, null, window);
            viewStateDictionary[window.GetType()].IsViewVisible = true;
        }

        private void RemoveAdornment(UIElement window)
        {
            layer.RemoveAdornment(window);
            viewStateDictionary[window.GetType()].IsViewVisible = false;
        }

        public static void HideView<T>(ITextView view)
        {
            GetAdornment(view)?.HideView<T>();
        }

        private void HideView<T>()
        {
            if (viewStateDictionary.TryGetValue(typeof(T), out var vsp))
            {
                vsp.View.GetType().GetMethod("Close")?.Invoke(vsp.View, null);
                RemoveAdornment(vsp.View as UIElement);
            }
        }

        public static void AdjustViewPosition<T>(ITextView view, SnapshotSpan span)
        {
            GetAdornment(view)?.AdjustViewPosition<T>(span);
        }

        private void AdjustViewPosition<T>(SnapshotSpan span)
        {
            if (viewStateDictionary.TryGetValue(typeof(T), out var vsp))
            {
                var bounds = view.TextViewLines.GetCharacterBounds(span.Start);
                var ui = vsp.View as UserControl;
                Canvas.SetLeft(ui, bounds.Left);
                Canvas.SetTop(ui, bounds.Top - ui.ActualHeight);
                vsp.IsAdjustPosition = true;
            }
        }

        public static void AdjustViewPositionEnd<T>(ITextView view)
        {
            GetAdornment(view)?.AdjustViewPositionEnd<T>();
        }

        private void AdjustViewPositionEnd<T>()
        {
            if (viewStateDictionary.TryGetValue(typeof(T), out var vsp))
            {
                vsp.IsAdjustPosition = false;
            }
        }
    }
}