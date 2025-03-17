using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace CommentTranslator22.Popups.CompletionToolTip.View
{
    public static class ControlFinder
    {
        // 按名称查找控件
        public static T FindByName<T>(FrameworkElement parent, string name) where T : FrameworkElement
        {
            return FindControls<T>(parent, c => c.Name == name).FirstOrDefault();
        }

        // 按类型查找控件
        public static T FindByType<T>(FrameworkElement parent) where T : FrameworkElement
        {
            return FindControls<T>(parent, c => c.GetType() == typeof(T)).FirstOrDefault();
        }

        // 递归查找控件
        private static IEnumerable<T> FindControls<T>(DependencyObject parent, Func<T, bool> condition) where T : FrameworkElement
        {
            var controls = new List<T>();

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T t && condition(t))
                {
                    controls.Add(t);
                }

                controls.AddRange(FindControls(child, condition));
            }

            return controls;
        }
    }
}
