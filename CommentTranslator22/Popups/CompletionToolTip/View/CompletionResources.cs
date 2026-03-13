using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.Text.Adornments;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CommentTranslator22.Popups.CompletionToolTip.View
{
    public static class CompletionResources
    {
        // 静态画刷缓存
        private static readonly Dictionary<string, Brush> BrushCache = new Dictionary<string, Brush>
        {
            // 类/委托
            ["c"] = new SolidColorBrush(Color.FromRgb(80, 180, 120)),
            ["d"] = new SolidColorBrush(Color.FromRgb(80, 180, 120)),
            ["class name"] = new SolidColorBrush(Color.FromRgb(80, 180, 120)),
            ["delegate name"] = new SolidColorBrush(Color.FromRgb(80, 180, 120)),

            // 结构体
            ["result"] = new SolidColorBrush(Color.FromRgb(135, 200, 145)),
            ["struct name"] = new SolidColorBrush(Color.FromRgb(135, 200, 145)),

            // 接口/枚举/类型参数
            ["i"] = new SolidColorBrush(Color.FromRgb(185, 215, 165)),
            ["e"] = new SolidColorBrush(Color.FromRgb(185, 215, 165)),
            ["interface name"] = new SolidColorBrush(Color.FromRgb(185, 215, 165)),
            ["enum name"] = new SolidColorBrush(Color.FromRgb(185, 215, 165)),
            ["type parameter name"] = new SolidColorBrush(Color.FromRgb(185, 215, 165)),

            // 关键字
            ["k"] = new SolidColorBrush(Color.FromRgb(80, 155, 215)),
            ["keyword"] = new SolidColorBrush(Color.FromRgb(80, 155, 215)),

            // 方法
            ["m"] = new SolidColorBrush(Color.FromRgb(220, 220, 155)),
            ["method name"] = new SolidColorBrush(Color.FromRgb(220, 220, 155)),

            // 局部变量/参数
            ["l"] = new SolidColorBrush(Color.FromRgb(155, 220, 255)),
            ["parameter name"] = new SolidColorBrush(Color.FromRgb(155, 220, 255)),

            // 片段
            ["t"] = new SolidColorBrush(Color.FromRgb(215, 160, 220)),

            // 未导入命名空间项
            ["a"] = new SolidColorBrush(Color.FromRgb(125, 125, 125)),

            // 命名空间/空白/标点/文本等
            ["n"] = Brushes.LightGray,
            ["namespace name"] = Brushes.LightGray,
            ["whitespace"] = Brushes.LightGray,
            ["punctuation"] = Brushes.LightGray,
            ["text"] = Brushes.LightGray
        };

        public static Brush GetBrush(ImmutableArray<CompletionFilter> filters)
        {
            return filters.Length > 0 ? GetBrush(filters[0].AccessKey) : Brushes.LightGray;
        }

        public static Brush GetBrush(ClassifiedTextRun run)
        {
            return GetBrush(run.ClassificationTypeName);
        }

        private static Brush GetBrush(string key)
        {
            return BrushCache.TryGetValue(key, out var brush) ? brush : Brushes.LightGray;
        }

        public static BitmapImage GetCompletionImage(ImmutableArray<CompletionFilter> filters)
        {
            return filters.Length > 0 ? GetCompletionImage(filters[0].AccessKey) : null;
        }

        private static readonly Dictionary<string, string> imageNameMap = new Dictionary<string, string>()
        {
            { "n", "Namespace" },  // 命名空间
            { "c", "Class" },      // 类
            { "result", "Struct" },// 结构体
            { "i", "Interface" },  // 接口
            { "e", "Enum" },       // 枚举
            { "d", "Delegate" },   // 委托
            { "o", "Constant" },   // 常量
            { "f", "Field" },      // 字段
            { "v", "Event" },      // 事件
            { "p", "Property" },   // 属性
            { "m", "Method" },     // 方法
            { "l", "Local" },      // 局部变量和参数
            { "k", "Keyword" },    // 关键字
            { "t", "Snippet" }     // 片段
        };

        private static BitmapImage GetCompletionImage(string key)
        {
            if (imageNameMap.TryGetValue(key, out var name))
            {
                return GetResourceImage("CommentTranslator22.Resources.Images." + name + ".png");
            }
            return null;
        }

        private static readonly Dictionary<string, BitmapImage> _imageCache = new Dictionary<string, BitmapImage>();

        private static BitmapImage GetResourceImage(string name)
        {
            if (_imageCache.TryGetValue(name, out var cachedImage))
                return cachedImage;

            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                using (var stream = assembly.GetManifestResourceStream(name))
                {
                    if (stream == null)
                        return null;

                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = stream;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();

                    _imageCache[name] = bitmap;
                    return bitmap;
                }
            }
            catch
            {
                return null;
            }
        }
    }
}