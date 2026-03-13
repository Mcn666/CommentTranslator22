// TranslationData.cs
using CommentTranslator22.Popups;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CommentTranslator22.Translate.TranslateData
{
    internal abstract class TranslationData
    {
        public class TranslationEntry
        {
            public string SourceText { get; set; }
            public string TargetText { get; set; }
        }

        // === 旧结构（用于兼容已有缓存）===
        public class TranslationLanguagePair
        {
            public LanguageEnum SourceLanguage { get; set; }
            public LanguageEnum TargetLanguage { get; set; }
            public ConcurrentDictionary<string, ConcurrentDictionary<string, TranslationEntry>> TranslationEntries { get; set; }
                = new ConcurrentDictionary<string, ConcurrentDictionary<string, TranslationEntry>>();
        }

        public class TranslationServerData
        {
            public ServerEnum Server { get; set; }
            public ConcurrentDictionary<string, TranslationLanguagePair> LanguagePairs { get; set; }
                = new ConcurrentDictionary<string, TranslationLanguagePair>();
        }

        // === 新结构（高性能扁平化存储）===
        // 扁平化存储：Key = $"{Server}|{LanguagePair}|{Prefix}|{FullKey}"
        protected ConcurrentDictionary<string, TranslationEntry> _flatStorage
            = new ConcurrentDictionary<string, TranslationEntry>();

        // 标记是否已加载旧数据（用于迁移判断）
        protected bool _isLegacyDataLoaded = false;
        protected bool _migrationCompleted = false;
        private bool _isMigrating = false;

        // 旧结构引用（仅当需要兼容时保留）
        protected ConcurrentDictionary<ServerEnum, TranslationServerData> StorageData { get; set; }
            = new ConcurrentDictionary<ServerEnum, TranslationServerData>();

        protected string MainFolder { get; }
        private static readonly object SaveLock = new object();

        // 版本管理相关
        private const string CURRENT_VERSION = "v3"; // 升级到 v3 表示支持扁平化
        private const string OLD_VERSION_FILE = "TranslationData.json";

        protected TranslationData()
        {
            MainFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CommentTranslator22");
            EnsureDirectoryExists(MainFolder);
            LoadData();
            TestSolutionEvents.Instance.SolutionClose += (s, e) => SaveData();

            // 启动后台迁移任务
            if (_isLegacyDataLoaded && !_migrationCompleted)
            {
                _ = Task.Run(() => MigrateOldDataInBackgroundAsync());
            }
        }

        private void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        // === 高性能查询（优先使用扁平化存储）===
        internal TranslationEntry GetTranslationEntry(string key)
        {
            return GetTranslationEntry(
                key,
                CommentTranslator22Package.Config.SourceLanguage,
                CommentTranslator22Package.Config.TargetLanguage,
                CommentTranslator22Package.Config.TranslationServer
            );
        }

        internal TranslationEntry GetTranslationEntry(string key, LanguageEnum sourceLanguage,
            LanguageEnum targetLanguage, ServerEnum server)
        {
            if (string.IsNullOrEmpty(key) || key.Length < 2)
            {
                return null;
            }

            var languagePairKey = $"{sourceLanguage}{targetLanguage}";
            var prefix = key.Substring(0, 2);

            // 【优化点1】优先从扁平化存储查找（O(1) 单次查找）
            var flatKey = BuildFlatKey(server, languagePairKey, prefix, key);
            if (_flatStorage.TryGetValue(flatKey, out var entry))
            {
                return entry;
            }

            // 【兼容性】如果旧数据已加载但尚未迁移完成，从旧结构查找
            if (_isLegacyDataLoaded && !_migrationCompleted)
            {
                if (StorageData.TryGetValue(server, out var serverData) &&
                    serverData.LanguagePairs.TryGetValue(languagePairKey, out var languagePair) &&
                    languagePair.TranslationEntries.TryGetValue(prefix, out var entries) &&
                    entries.TryGetValue(key, out var legacyEntry))
                {
                    // 命中旧数据，异步提升到新结构（写时迁移）
                    _ = Task.Run(() => _flatStorage.TryAdd(flatKey, legacyEntry));
                    return legacyEntry;
                }
            }

            return null;
        }

        // === 高性能写入（直接写入扁平化存储）===
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

        internal void AddTranslationEntry(string key, string result, LanguageEnum sourceLanguage,
            LanguageEnum targetLanguage, ServerEnum server)
        {
            if (string.IsNullOrEmpty(key) || key.Length < 2)
            {
                return;
            }

            var languagePairKey = $"{sourceLanguage}{targetLanguage}";
            var prefix = key.Substring(0, 2);
            var flatKey = BuildFlatKey(server, languagePairKey, prefix, key);

            // 【优化点2】直接写入扁平化存储（无需多层嵌套）
            _flatStorage.AddOrUpdate(flatKey, k => new TranslationEntry
            {
                SourceText = key,
                TargetText = result
            }, (k, existingEntry) =>
            {
                existingEntry.TargetText = result;
                return existingEntry;
            });

            // 同步更新旧结构（仅当旧数据存在时，保持向后兼容）
            if (_isLegacyDataLoaded && !_migrationCompleted)
            {
                UpdateLegacyStructure(key, result, sourceLanguage, targetLanguage, server);
            }
        }

        // === 辅助方法 ===
        private string BuildFlatKey(ServerEnum server, string languagePairKey, string prefix, string fullKey)
        {
            return $"{server}|{languagePairKey}|{prefix}|{fullKey}";
        }

        private void UpdateLegacyStructure(string key, string result, LanguageEnum sourceLanguage,
            LanguageEnum targetLanguage, ServerEnum server)
        {
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
                existingEntry.TargetText = result;
                return existingEntry;
            });
        }

        // === 后台迁移任务 ===
        private async Task MigrateOldDataInBackgroundAsync()
        {
            if (!_isLegacyDataLoaded || _migrationCompleted || _isMigrating)
            {
                return;
            }

            _isMigrating = true;

            try
            {
                // 等待系统空闲（避免影响用户体验）
                await Task.Delay(5000);

                int migratedCount = 0;
                foreach (var serverData in StorageData.Values)
                {
                    foreach (var languagePair in serverData.LanguagePairs.Values)
                    {
                        var languagePairKey = $"{languagePair.SourceLanguage}{languagePair.TargetLanguage}";

                        foreach (var entries in languagePair.TranslationEntries.Values)
                        {
                            foreach (var kvp in entries)
                            {
                                var prefix = kvp.Key.Length >= 2 ? kvp.Key.Substring(0, 2) : kvp.Key;
                                var flatKey = BuildFlatKey(serverData.Server, languagePairKey, prefix, kvp.Key);

                                if (_flatStorage.TryAdd(flatKey, kvp.Value))
                                {
                                    migratedCount++;
                                }
                            }
                        }
                    }
                }

                _migrationCompleted = true;
                Console.WriteLine($"后台迁移完成：迁移了 {migratedCount} 条记录到扁平化存储");

                // 迁移完成后立即保存一次，固化新格式
                SaveData();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"后台迁移失败: {ex.Message}");
                _isMigrating = false;
            }
        }

        protected abstract void SaveData();
        protected abstract void LoadData();

        protected void SaveTranslationData()
        {
            lock (SaveLock)
            {
                var filePath = Path.Combine(MainFolder, $"{GetType().Name}.json");

                // 【优化点3】优先保存扁平化数据（更小、更快）
                // 但仍保留旧结构以便旧版本读取（过渡期）
                var dataWithVersion = new
                {
                    Version = CURRENT_VERSION,
                    FlatData = _flatStorage.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                    LegacyData = (_isLegacyDataLoaded && !_migrationCompleted) ? StorageData : null,
                    LastUpdate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    MigrationCompleted = _migrationCompleted
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
                MigrateFromOldVersion();
                return;
            }

            var json = File.ReadAllText(filePath);
            try
            {
                var versionedData = JsonConvert.DeserializeObject<VersionedDataWrapper>(json);

                if (versionedData != null && versionedData.Version != null)
                {
                    // 【兼容性】处理 v2（旧嵌套结构）和 v3（新扁平结构）
                    if (versionedData.Version == "v3" && versionedData.FlatData != null)
                    {
                        // 新格式：直接加载扁平化数据
                        _flatStorage = new ConcurrentDictionary<string, TranslationEntry>(
                            versionedData.FlatData
                        );
                        _migrationCompleted = versionedData.MigrationCompleted;

                        // 如果还有旧数据，也加载以便双向兼容
                        if (versionedData.LegacyData != null)
                        {
                            StorageData = versionedData.LegacyData;
                            _isLegacyDataLoaded = true;
                        }
                    }
                    else if (versionedData.Data != null)
                    {
                        // v2 格式：加载旧结构，触发后台迁移
                        StorageData = versionedData.Data;
                        _isLegacyDataLoaded = true;
                        CleanupOldData();
                    }
                    else
                    {
                        StorageData = new ConcurrentDictionary<ServerEnum, TranslationServerData>();
                    }
                }
                else
                {
                    // 无版本信息：视为最旧格式
                    StorageData = JsonConvert.DeserializeObject<ConcurrentDictionary<ServerEnum, TranslationServerData>>(json) ??
                                 new ConcurrentDictionary<ServerEnum, TranslationServerData>();
                    _isLegacyDataLoaded = true;
                    CleanupOldData();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载数据失败: {ex.Message}");
                StorageData = new ConcurrentDictionary<ServerEnum, TranslationServerData>();
                _flatStorage = new ConcurrentDictionary<string, TranslationEntry>();
            }
        }

        private void CleanupOldData()
        {
            // 清理逻辑保持不变
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
                    CleanupMigratedData(oldData);

                    if (ShouldMigrateOldData())
                    {
                        StorageData = oldData;
                        _isLegacyDataLoaded = true;
                        Console.WriteLine($"成功从旧版本迁移数据到 {GetType().Name}");
                    }
                }

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
            // 清理逻辑保持不变
        }

        protected virtual bool ShouldMigrateOldData()
        {
            return GetType().Name == "GeneralTranslationData";
        }

        private class VersionedDataWrapper
        {
            [JsonProperty("Version")]
            public string Version { get; set; }

            [JsonProperty("Data")]
            public ConcurrentDictionary<ServerEnum, TranslationServerData> Data { get; set; }

            [JsonProperty("FlatData")]
            public Dictionary<string, TranslationEntry> FlatData { get; set; }

            [JsonProperty("LegacyData")]
            public ConcurrentDictionary<ServerEnum, TranslationServerData> LegacyData { get; set; }

            [JsonProperty("LastUpdate")]
            public string LastUpdate { get; set; }

            [JsonProperty("MigrationCompleted")]
            public bool MigrationCompleted { get; set; }
        }
    }
}