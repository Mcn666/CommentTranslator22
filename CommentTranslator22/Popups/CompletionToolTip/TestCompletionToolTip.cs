using CommentTranslator22.Translate;
using CommentTranslator22.Translate.TranslateData;
using EnvDTE;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Concurrent;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CommentTranslator22.Popups.CompletionToolTip
{
    /// <summary>
    /// 测试用补全工具提示控件
    /// </summary>
    internal class TestCompletionToolTip : TextBlock
    {
        // 静态缓存，记录正在翻译或已翻译的短语，避免重复请求
        private static readonly ConcurrentDictionary<string, bool> _translationRequestCache =
            new ConcurrentDictionary<string, bool>();

        // 用于标识当前短语翻译是否进行中
        private readonly string _currentPhrase;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="completion">补全项</param>
        public TestCompletionToolTip(Completion completion)
        {
            // 记录当前短语
            var unfolded = TranslationClient.Instance.HumpUnfold(completion.DisplayText).Trim();
            _currentPhrase = unfolded;

            InitializeToolTip(completion);
        }

        /// <summary>
        /// 初始化工具提示内容
        /// </summary>
        private void InitializeToolTip(Completion completion)
        {
            // 如果补全项没有描述信息，则进行翻译处理
            if (string.IsNullOrEmpty(completion.Description))
            {
                var resultText = string.Empty;
                var unfolded = _currentPhrase;
                var split = unfolded.Split(' ');

                // 处理单个单词
                if (split.Length == 1)
                {
                    var dictionaryResult = Dictionary.Dictionary.Instance.IndexOf(unfolded);
                    if (dictionaryResult != null)
                    {
                        resultText = $"{unfolded}  {dictionaryResult.zh}";
                    }
                    // 未找到翻译时，resultText保持为空
                }
                // 处理多个单词（短语）
                else
                {
                    var phraseResult = PhraseTranslationData.Instance.GetTranslationEntry(unfolded);
                    if (phraseResult != null && phraseResult.TargetText.Trim() != string.Empty)
                    {
                        resultText = $"{unfolded}  {phraseResult.TargetText}";
                    }
                    else
                    {
                        // 临时显示单词级翻译结果
                        foreach (var word in split)
                        {
                            var wordResult = Dictionary.Dictionary.Instance.IndexOf(word);
                            resultText += $"{word}  {(wordResult == null ? "??" : wordResult.zh)}\n";
                        }

                        // 异步翻译短语（后台执行，不阻塞UI）
                        StartBackgroundTranslation(unfolded);
                    }
                }

                this.Text = resultText.TrimEnd('\n');
                this.FontSize = 12;
                this.Foreground = Brushes.LightGray;
            }
        }

        /// <summary>
        /// 启动后台翻译任务
        /// </summary>
        private void StartBackgroundTranslation(string phrase)
        {
            // 跳过空值或已经在翻译的短语
            if (string.IsNullOrWhiteSpace(phrase) || _translationRequestCache.ContainsKey(phrase))
                return;

            // 标记该短语正在翻译中
            if (!_translationRequestCache.TryAdd(phrase, true))
                return; // 添加失败，说明已经在翻译中

            // 使用后台任务处理翻译，避免阻塞UI线程
            _ = System.Threading.Tasks.Task.Run(async () =>
            {
                try
                {
                    // 在后台线程执行翻译
                    var translationResult = await TranslationClient.Instance.TranslateAsync(phrase);

                    // 切换到主线程更新缓存（如果需要线程安全访问共享资源）
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                    // 缓存翻译结果
                    PhraseTranslationData.Instance.AddTranslationEntry(phrase, translationResult.TargetText);

                    // 可选：如果希望工具提示能自动更新，可以在这里触发UI更新事件
                    // 但由于VS工具提示通常不会动态更新，这里只缓存结果供下次使用
                }
                catch (Exception ex)
                {
                    // 从缓存中移除，允许重试
                    _translationRequestCache.TryRemove(phrase, out _);

                    // 记录错误但不影响UI
                    System.Diagnostics.Debug.WriteLine($"翻译失败 '{phrase}': {ex.Message}");

                    // 可以在这里实现重试逻辑，例如延迟后重试
                    // 但当前实现简单起见，只记录错误
                }
                finally
                {
                    // 翻译完成后，保留缓存标记一段时间（比如5分钟），防止频繁请求
                    // 可以在这里添加定时器来清理过期缓存
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// 清理翻译缓存（可选方法，可用于内存管理）
        /// </summary>
        /// <param name="olderThanMinutes">清理指定分钟前的缓存</param>
        public static void ClearTranslationCache(int olderThanMinutes = 10)
        {
            // 在实际应用中，可以实现更复杂的缓存清理策略
            // 这里简单清理所有缓存
            _translationRequestCache.Clear();
        }
    }

    /// <summary>
    /// 测试用补全工具提示提供程序
    /// </summary>
    /// <remarks>
    /// 当前选择的补全项描述信息提示接口。
    /// 在最新的VS2022版本中，C#不再使用此接口，主要用于XML/XAML内容
    /// </remarks>
    [Export(typeof(IUIElementProvider<Completion, ICompletionSession>))]
    [Name(nameof(TestCompletionToolTipProvider))]
    [Order(Before = "RoslynToolTipProvider")] // 覆盖默认的Roslyn提示提供程序
    [ContentType("XML")]
    [ContentType("XAML")]
    internal class TestCompletionToolTipProvider : IUIElementProvider<Completion, ICompletionSession>
    {
        /// <summary>
        /// 获取UI元素
        /// </summary>
        /// <param name="itemToRender">要渲染的补全项</param>
        /// <param name="context">补全会话上下文</param>
        /// <param name="elementType">UI元素类型</param>
        /// <returns>UI元素或null</returns>
        public UIElement GetUIElement(Completion itemToRender, ICompletionSession context, UIElementType elementType)
        {
            if (elementType == UIElementType.Tooltip)
            {
                return new TestCompletionToolTip(itemToRender);
            }

            return null;
        }
    }

    // 可选：定时清理翻译缓存的辅助类
    // 可以在应用程序启动或关闭时调用清理方法
    internal static class TranslationCacheManager
    {
        private static System.Threading.Timer _cacheCleanupTimer;

        /// <summary>
        /// 启动定时清理缓存
        /// </summary>
        /// <param name="cleanupIntervalMinutes">清理间隔（分钟）</param>
        public static void StartCacheCleanup(int cleanupIntervalMinutes = 30)
        {
            // 每30分钟清理一次缓存
            _cacheCleanupTimer = new System.Threading.Timer(
                _ => TestCompletionToolTip.ClearTranslationCache(),
                null,
                TimeSpan.FromMinutes(cleanupIntervalMinutes),
                TimeSpan.FromMinutes(cleanupIntervalMinutes)
            );
        }

        /// <summary>
        /// 停止定时清理
        /// </summary>
        public static void StopCacheCleanup()
        {
            _cacheCleanupTimer?.Dispose();
            _cacheCleanupTimer = null;
        }
    }
}