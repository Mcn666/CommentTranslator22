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
            switch (key)
            {
                case "c":   // 类
                case "d":   // 委托
                case "class name":      // 类名称
                case "delegate name":   // 委托名称
                    return new SolidColorBrush(Color.FromRgb(80, 180, 120));
                case "result":   // 结构体
                case "struct name":     // 结构体名称
                    return new SolidColorBrush(Color.FromRgb(135, 200, 145));
                case "i":   // 接口
                case "e":   // 枚举
                case "interface name":  // 接口名称
                case "enum name":       // 枚举名称
                case "type parameter name": // 类型参数名称 T
                    return new SolidColorBrush(Color.FromRgb(185, 215, 165));
                case "k":   // 关键字
                case "keyword":         // 关键字
                    return new SolidColorBrush(Color.FromRgb(80, 155, 215));
                case "m":   // 方法
                case "method name":     // 方法名称
                    return new SolidColorBrush(Color.FromRgb(220, 220, 155));
                case "l":   // 局部变量和参数
                case "parameter name":  // 参数名称
                    return new SolidColorBrush(Color.FromRgb(155, 220, 255));
                case "t":   // 片段
                    return new SolidColorBrush(Color.FromRgb(215, 160, 220));
                case "a":   // 未导入命名空间的项，在此项中有其所在的命名空间
                    return new SolidColorBrush(Color.FromRgb(125, 125, 125));
                case "n":   // 命名空间
                case "namespace name":  // 命名空间名称
                case "whitespace":      // 空白
                case "punctuation":     // 标点符号
                case "text":            // 文本
                default:
                    return Brushes.LightGray;
            }
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
            // 首先检查缓存中是否存在该图片
            if (_imageCache.TryGetValue(name, out var cachedImage))
            {
                return cachedImage;
            }

            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                using (var stream = assembly.GetManifestResourceStream(name))
                {
                    if (stream == null)
                    {
                        return null;
                    }

                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = stream;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad; // 避免流关闭后丢失图像数据
                    bitmap.EndInit();
                    bitmap.Freeze(); // 如果需要从多个线程访问，可以考虑冻结图像

                    // 将加载的图片缓存起来
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
