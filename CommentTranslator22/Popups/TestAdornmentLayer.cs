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

            public bool IsDisplay { get; set; }

            public WindowState(bool limit, bool display)
            {
                IsLimit = limit;
                IsDisplay = display;
            }
        }

        private readonly IWpfTextView view;
        private readonly IAdornmentLayer layer;
        private readonly Dictionary<object, object> window = new Dictionary<object, object>()
        {
            [typeof(Command1Window)] = new Command1Window(),
        };
        private readonly Dictionary<object, WindowState> state = new Dictionary<object, WindowState>()
        {
            [typeof(Command1Window)] = new WindowState(false, false),
            [typeof(TestCompletionDescriptionWindow)] = new WindowState(false, false),
            [typeof(TestCompletionItemWindow)] = new WindowState(false, false),
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

            var window1 = new TestCompletionDescriptionWindow();
            var window2 = new TestCompletionItemWindow(window1);
            window.Add(window1.GetType(), window1);
            window.Add(window2.GetType(), window2);
        }

        private void TestAdornmentLayer_LayoutUpdated(object sender, EventArgs e)
        {
            // 此事件用于缓解UI关闭的问题，在布局更新时，总是去检查控件的显示状态
            // 如果有更好的方法，以后会进行更改
            if (view == null)
            {
                return;
            }
            foreach (var w in window)
            {
                if (state[w.Key].IsDisplay == true)
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
            foreach (var i in window)
            {
                i.Value.GetType().GetMethod("Close")?.Invoke(i.Value, null);
            }
        }

        private void Selection_SelectionChanged(object sender, EventArgs e)
        {
            foreach (var i in window)
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

        public static object GetWindow(ITextView view, SnapshotSpan span, Type key, bool autoPos = true)
        {
            return GetAdornment(view)?.GetWindow(span, key, autoPos);
        }

        public static T GetWindow<T>(ITextView view, SnapshotSpan span, Type key, bool autoPos = true)
        {
            return (T)(GetAdornment(view)?.GetWindow(span, key, autoPos));
        }

        private object GetWindow(SnapshotSpan span, Type key, bool pos)
        {
            if (window.TryGetValue(key, out var value))
            {
                EventHandler handler = null;
                handler = (sender, e) =>
                {
                    sender.GetType().GetEvent("OnClosed")?.RemoveEventHandler(sender, handler);
                    RemoveAdornment(sender as UIElement);
                };

                value.GetType().GetEvent("OnClosed")?.AddEventHandler(value, handler);
                if (pos == true)
                {
                    PopupPosition(value as UIElement, span);
                }
                AddAdornment(value as UIElement, span);
                return value;
            }
            return null;
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
            state[window.GetType()].IsDisplay = true;
        }

        private void RemoveAdornment(UIElement window)
        {
            layer.RemoveAdornment(window);
            state[window.GetType()].IsDisplay = false;
        }

        public static void CloseWindow(ITextView view, Type type)
        {
            GetAdornment(view)?.CloseWindow(type);
        }

        private void CloseWindow(Type type)
        {
            if (window.TryGetValue(type, out var value))
            {
                value.GetType().GetMethod("Close")?.Invoke(value, null);
                RemoveAdornment(value as UIElement);
            }
        }

        public static void AdjustWindowPosition(ITextView view, SnapshotSpan span, Type type)
        {
            GetAdornment(view)?.AdjustWindowPosition(span, type);
        }

        private void AdjustWindowPosition(SnapshotSpan span, Type type)
        {
            if (window.TryGetValue(type, out var value))
            {
                var bounds = view.TextViewLines.GetCharacterBounds(span.Start);
                var ui = value as UserControl;
                Canvas.SetLeft(ui, bounds.Left);
                Canvas.SetTop(ui, bounds.Top - ui.ActualHeight);
                state[type].IsLimit = true;
            }
        }

        public static void AdjustWindowPositionEnd(ITextView view, Type type)
        {
            GetAdornment(view)?.AdjustWindowPositionEnd(type);
        }

        private void AdjustWindowPositionEnd(Type type)
        {
            state[type].IsLimit = false;
        }
    }
}
