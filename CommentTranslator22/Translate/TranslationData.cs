﻿using CommentTranslator22.Popups;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.IO;

namespace CommentTranslator22.Translate
{
    internal abstract class TranslationData
    {
        public class TranslationEntry
        {
            public int Count { get; set; }
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
                entry.Count++;
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
                Count = 1,
                SourceText = key,
                TargetText = result
            }, (k, existingEntry) =>
            {
                existingEntry.Count++;
                return existingEntry;
            });
        }

        protected abstract void SaveData();

        protected abstract void LoadData();

        protected void SaveData(string fileName)
        {
            var filePath = Path.Combine(MainFolder, $"{fileName}.json");
            var json = JsonConvert.SerializeObject(StorageData, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        protected void LoadData(string fileName)
        {
            var filePath = Path.Combine(MainFolder, $"{fileName}.json");
            if (!File.Exists(filePath))
            {
                return;
            }

            var json = File.ReadAllText(filePath);
            StorageData = JsonConvert.DeserializeObject<ConcurrentDictionary<ServerEnum, TranslationServerData>>(json);
        }
    }
}