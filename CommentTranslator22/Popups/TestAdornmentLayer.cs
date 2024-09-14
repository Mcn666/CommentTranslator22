using CommentTranslator22.Popups.Command;
using CommentTranslator22.Popups.CompletionToolTip;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Collections.Generic;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace CommentTranslator22.Popups
{
    internal class TestAdornmentLayer
    {
        class WindowState
        {
            public bool IsLimit { get; set; }

            public bool IsShow { get; set; }

            public WindowState(bool limit, bool display)
            {
                IsLimit = limit;
                IsShow = display;
            }
        }

        private readonly IWpfTextView view;
        private readonly IAdornmentLayer layer;
        private readonly Dictionary<object, object> views = new Dictionary<object, object>()
        {
            [typeof(Command1View)] = new Command1View(),
            [typeof(TestCompletionItemView)] = new TestCompletionItemView(),
        };
        private readonly Dictionary<object, WindowState> state = new Dictionary<object, WindowState>()
        {
            [typeof(Command1View)] = new WindowState(false, false),
            [typeof(TestCompletionItemView)] = new WindowState(false, false),
        };

        public TestAdornmentLayer(IWpfTextView view)
        {
            this.view = view;
            this.view.Selection.SelectionChanged += Selection_SelectionChanged;
            this.view.Closed += View_Closed;

            layer = view.GetAdornmentLayer(nameof(TestAdornmentLayer));
            if (CommentTranslator22Package.Config.UseUiLimit == false)
            {
                (layer as UIElement).LayoutUpdated += TestAdornmentLayer_LayoutUpdated;
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
            foreach (var w in views)
            {
                if (state[w.Key].IsShow == true)
                {
                    AddAdornment(w.Value as UIElement, view.Selection.SelectedSpans[0]);
                }
                else if (layer.Elements.Count > 0)
                {
                    RemoveAdornment(w.Value as UIElement);
                }
            }
        }

        private void View_Closed(object sender, EventArgs e)
        {
            foreach (var i in views)
            {
                i.Value.GetType().GetMethod("Close")?.Invoke(i.Value, null);
            }
        }

        private void Selection_SelectionChanged(object sender, EventArgs e)
        {
            foreach (var i in views)
            {
                i.Value.GetType().GetMethod("Close")?.Invoke(i.Value, null);
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
            if (views.TryGetValue(typeof(T), out var value))
            {
                EventHandler handler = null;
                handler = (sender, e) =>
                {
                    sender.GetType().GetEvent("OnClosed")?.RemoveEventHandler(sender, handler);
                    RemoveAdornment(sender as UIElement);
                };

                value.GetType().GetEvent("OnClosed")?.AddEventHandler(value, handler);
                PopupPosition(value as UIElement, span);
                AddAdornment(value as UIElement, span);
                return (T)value;
            }
            return default;
        }

        private void PopupPosition(UIElement window, SnapshotSpan span)
        {
            var bounds = view.TextViewLines.GetCharacterBounds(span.Start);
            if (bounds != null)
            {
                if (state.TryGetValue(window.GetType(), out var value))
                {
                    if (value.IsLimit == false)
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
            state[window.GetType()].IsShow = true;
        }

        private void RemoveAdornment(UIElement window)
        {
            layer.RemoveAdornment(window);
            state[window.GetType()].IsShow = false;
        }

        public static void HideView<T>(ITextView view)
        {
            GetAdornment(view)?.HideView<T>();
        }

        private void HideView<T>()
        {
            if (views.TryGetValue(typeof(T), out var value))
            {
                value.GetType().GetMethod("Close")?.Invoke(value, null);
                RemoveAdornment(value as UIElement);
            }
        }

        public static void AdjustViewPosition<T>(ITextView view, SnapshotSpan span)
        {
            GetAdornment(view)?.AdjustViewPosition<T>(span);
        }

        private void AdjustViewPosition<T>(SnapshotSpan span)
        {
            if (views.TryGetValue(typeof(T), out var value))
            {
                var bounds = view.TextViewLines.GetCharacterBounds(span.Start);
                var ui = value as UserControl;
                Canvas.SetLeft(ui, bounds.Left);
                Canvas.SetTop(ui, bounds.Top - ui.ActualHeight);
                state[typeof(T)].IsLimit = true;
            }
        }

        public static void AdjustViewPositionEnd<T>(ITextView view)
        {
            GetAdornment(view)?.AdjustViewPositionEnd<T>();
        }

        private void AdjustViewPositionEnd<T>()
        {
            state[typeof(T)].IsLimit = false;
        }
    }
}
