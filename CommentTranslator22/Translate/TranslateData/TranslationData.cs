// TranslationData.cs
using CommentTranslator22.Popups;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.IO;

namespace CommentTranslator22.Translate.TranslateData
{
    internal abstract class TranslationData
    {
        public class TranslationEntry
        {
            public string SourceText { get; set; }
            public string TargetText { get; set; }
        }

        public class TranslationLanguagePair
        {
            public LanguageEnum SourceLanguage { get; set; }
            public LanguageEnum TargetLanguage { get; set; }
            public ConcurrentDictionary<string, ConcurrentDictionary<string, TranslationEntry>> TranslationEntries { get; set; } = new ConcurrentDictionary<string, ConcurrentDictionary<string, TranslationEntry>>();
        }

        public class TranslationServerData
        {
            public ServerEnum Server { get; set; }
            public ConcurrentDictionary<string, TranslationLanguagePair> LanguagePairs { get; set; } = new ConcurrentDictionary<string, TranslationLanguagePair>();
        }

        protected ConcurrentDictionary<ServerEnum, TranslationServerData> StorageData { get; set; } = new ConcurrentDictionary<ServerEnum, TranslationServerData>();
        protected string MainFolder { get; }
        private static readonly object SaveLock = new object();

        // 版本管理相关
        private const string CURRENT_VERSION = "v2";
        private const string OLD_VERSION_FILE = "TranslationData.json";
        private const string VERSION_MARKER = "Version";

        protected TranslationData()
        {
            MainFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CommentTranslator22");
            EnsureDirectoryExists(MainFolder);
            LoadData();
            TestSolutionEvents.Instance.SolutionClose += (s, e) => SaveData();
        }

        private void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        internal TranslationEntry GetTranslationEntry(string key)
        {
            return GetTranslationEntry(
                key,
                CommentTranslator22Package.Config.SourceLanguage,
                CommentTranslator22Package.Config.TargetLanguage,
                CommentTranslator22Package.Config.TranslationServer
            );
        }

        internal TranslationEntry GetTranslationEntry(string key, LanguageEnum sourceLanguage, LanguageEnum targetLanguage, ServerEnum server)
        {
            if (string.IsNullOrEmpty(key) || key.Length < 2)
            {
                return null;
            }

            var languagePairKey = $"{sourceLanguage}{targetLanguage}";
            var prefix = key.Substring(0, 2);

            if (StorageData.TryGetValue(server, out var serverData) &&
                serverData.LanguagePairs.TryGetValue(languagePairKey, out var languagePair) &&
                languagePair.TranslationEntries.TryGetValue(prefix, out var entries) &&
                entries.TryGetValue(key, out var entry))
            {
                return entry;
            }

            return null;
        }

        internal void AddTranslationEntry(string key, string result)
        {
            AddTranslationEntry(
                key,
                result,
                CommentTranslator22Package.Config.SourceLanguage,
                CommentTranslator22Package.Config.TargetLanguage,
                CommentTranslator22Package.Config.TranslationServer
            );
        }

        internal void AddTranslationEntry(string key, string result, LanguageEnum sourceLanguage, LanguageEnum targetLanguage, ServerEnum server)
        {
            if (string.IsNullOrEmpty(key) || key.Length < 2)
            {
                return;
            }

            var languagePairKey = $"{sourceLanguage}{targetLanguage}";
            var prefix = key.Substring(0, 2);

            var serverData = StorageData.GetOrAdd(server, s => new TranslationServerData { Server = s });
            var languagePair = serverData.LanguagePairs.GetOrAdd(languagePairKey, lp => new TranslationLanguagePair
            {
                SourceLanguage = sourceLanguage,
                TargetLanguage = targetLanguage
            });

            var entries = languagePair.TranslationEntries.GetOrAdd(prefix, p => new ConcurrentDictionary<string, TranslationEntry>());
            entries.AddOrUpdate(key, k => new TranslationEntry
            {
                SourceText = key,
                TargetText = result
            }, (k, existingEntry) =>
            {
                // 如果已存在，更新翻译结果（可以选择保留旧的或使用新的）
                existingEntry.TargetText = result;
                return existingEntry;
            });
        }

        protected abstract void SaveData();
        protected abstract void LoadData();

        protected void SaveTranslationData()
        {
            lock (SaveLock)
            {
                var filePath = Path.Combine(MainFolder, $"{GetType().Name}.json");
                var dataWithVersion = new
                {
                    Version = CURRENT_VERSION,
                    Data = StorageData,
                    LastUpdate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };
                var json = JsonConvert.SerializeObject(dataWithVersion, Formatting.Indented);
                File.WriteAllText(filePath, json);
            }
        }

        protected void LoadTranslationData()
        {
            var filePath = Path.Combine(MainFolder, $"{GetType().Name}.json");
            if (!File.Exists(filePath))
            {
                // 尝试加载旧版本数据
                MigrateFromOldVersion();
                return;
            }

            var json = File.ReadAllText(filePath);
            try
            {
                // 尝试解析带版本信息的数据
                var versionedData = JsonConvert.DeserializeObject<VersionedDataWrapper>(json);
                if (versionedData != null && versionedData.Version != null)
                {
                    // 新版本数据
                    StorageData = versionedData.Data ?? new ConcurrentDictionary<ServerEnum, TranslationServerData>();

                    // 清理旧版本可能残留的Count属性
                    CleanupOldData();
                }
                else
                {
                    // 旧版本数据（没有版本信息）
                    StorageData = JsonConvert.DeserializeObject<ConcurrentDictionary<ServerEnum, TranslationServerData>>(json) ??
                                 new ConcurrentDictionary<ServerEnum, TranslationServerData>();

                    // 清理旧版本可能残留的Count属性
                    CleanupOldData();
                }
            }
            catch
            {
                // 解析失败，使用空数据
                StorageData = new ConcurrentDictionary<ServerEnum, TranslationServerData>();
            }
        }

        private void CleanupOldData()
        {
            // 清理旧版本可能残留的Count属性
            foreach (var serverData in StorageData.Values)
            {
                foreach (var languagePair in serverData.LanguagePairs.Values)
                {
                    foreach (var entries in languagePair.TranslationEntries.Values)
                    {
                        foreach (var entry in entries.Values)
                        {
                            // 确保没有Count属性残留
                            // 由于我们移除了Count属性，这里不需要做任何操作
                            // 反序列化时会自动忽略不存在的属性
                        }
                    }
                }
            }
        }

        private void MigrateFromOldVersion()
        {
            var oldFilePath = Path.Combine(MainFolder, OLD_VERSION_FILE);
            if (!File.Exists(oldFilePath))
            {
                return;
            }

            try
            {
                var json = File.ReadAllText(oldFilePath);
                var oldData = JsonConvert.DeserializeObject<ConcurrentDictionary<ServerEnum, TranslationServerData>>(json);

                if (oldData != null)
                {
                    // 清理旧数据中的Count属性
                    CleanupMigratedData(oldData);

                    if (ShouldMigrateOldData())
                    {
                        StorageData = oldData;
                        Console.WriteLine($"成功从旧版本迁移数据到 {GetType().Name}");
                    }
                }

                // 迁移后备份旧文件
                var backupPath = Path.Combine(MainFolder, $"{OLD_VERSION_FILE}.backup_{DateTime.Now:yyyyMMddHHmmss}");
                File.Move(oldFilePath, backupPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"迁移旧版本数据失败: {ex.Message}");
            }
        }

        private void CleanupMigratedData(ConcurrentDictionary<ServerEnum, TranslationServerData> oldData)
        {
            // 清理迁移数据中的Count属性
            // 由于Count属性已移除，反序列化时会被忽略，但这里确保数据干净
            foreach (var serverData in oldData.Values)
            {
                foreach (var languagePair in serverData.LanguagePairs.Values)
                {
                    foreach (var entries in languagePair.TranslationEntries.Values)
                    {
                        // 遍历所有条目，确保没有Count属性
                        // 由于我们移除了Count属性，反序列化后就不会有Count
                        // 这里主要是为了代码清晰
                    }
                }
            }
        }

        protected virtual bool ShouldMigrateOldData()
        {
            // 默认实现：通用翻译数据继承所有旧数据
            // 子类可以重写此方法以定制迁移策略
            return GetType().Name == "GeneralTranslationData";
        }

        private class VersionedDataWrapper
        {
            [JsonProperty("Version")]
            public string Version { get; set; }

            [JsonProperty("Data")]
            public ConcurrentDictionary<ServerEnum, TranslationServerData> Data { get; set; }

            [JsonProperty("LastUpdate")]
            public string LastUpdate { get; set; }
        }
    }
}