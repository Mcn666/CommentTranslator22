using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace CommentTranslator22.Popups.CompletionToolTip.View
{
    public static class ControlFinder
    {
        /// <summary>
        /// 按名称查找第一个匹配的控件（最大深度 10）
        /// </summary>
        public static T FindByName<T>(FrameworkElement parent, string name, int maxDepth = 10) where T : FrameworkElement
        {
            return FindFirst<T>(parent, element => element.Name == name, maxDepth);
        }

        /// <summary>
        /// 按类型查找第一个匹配的控件（最大深度 10）
        /// </summary>
        public static T FindByType<T>(FrameworkElement parent, int maxDepth = 10) where T : FrameworkElement
        {
            return FindFirst<T>(parent, element => element.GetType() == typeof(T), maxDepth);
        }

        /// <summary>
        /// 使用队列迭代查找第一个符合条件的控件，支持最大深度限制
        /// </summary>
        private static T FindFirst<T>(DependencyObject root, Func<T, bool> predicate, int maxDepth) where T : FrameworkElement
        {
            if (root == null)
                return null;

            // 队列元素：节点和当前深度
            var queue = new Queue<(DependencyObject node, int depth)>();
            queue.Enqueue((root, 0));

            while (queue.Count > 0)
            {
                var (current, depth) = queue.Dequeue();

                // 如果当前节点符合条件且是目标类型，直接返回
                if (current is T t && predicate(t))
                    return t;

                // 如果已达到最大深度，不再处理其子节点
                if (depth >= maxDepth)
                    continue;

                // 将子节点加入队列
                int childCount = VisualTreeHelper.GetChildrenCount(current);
                for (int i = 0; i < childCount; i++)
                {
                    var child = VisualTreeHelper.GetChild(current, i);
                    queue.Enqueue((child, depth + 1));
                }
            }

            return null;
        }

        /// <summary>
        /// 查找所有符合条件的控件（谨慎使用，可能遍历整个树）
        /// </summary>
        public static IEnumerable<T> FindAll<T>(DependencyObject root, Func<T, bool> predicate = null, int maxDepth = int.MaxValue) where T : DependencyObject
        {
            if (root == null)
                yield break;

            var queue = new Queue<(DependencyObject node, int depth)>();
            queue.Enqueue((root, 0));

            while (queue.Count > 0)
            {
                var (current, depth) = queue.Dequeue();

                if (current is T t && (predicate == null || predicate(t)))
                    yield return t;

                if (depth >= maxDepth)
                    continue;

                int childCount = VisualTreeHelper.GetChildrenCount(current);
                for (int i = 0; i < childCount; i++)
                {
                    var child = VisualTreeHelper.GetChild(current, i);
                    queue.Enqueue((child, depth + 1));
                }
            }
        }
    }
}