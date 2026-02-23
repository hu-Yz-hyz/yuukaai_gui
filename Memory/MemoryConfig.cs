using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace yuukaaigui.Memory
{
    public class MemoryConfigData
    {
        public bool EnableMemory { get; set; } = true;
        public bool EnableShortTerm { get; set; } = true;
        public bool EnableSummary { get; set; } = true;
        public bool EnableVectorStore { get; set; } = true;
        
        public int ShortTermCount { get; set; } = 20;
        public int SummaryInterval { get; set; } = 50;
        
        public string? DashScopeApiKey { get; set; }
        public string? VectorStoreApiKey { get; set; }
        
        public string? VectorCollectionName { get; set; } = "yuukai_memory";
    }

    public class ConversationMessage
    {
        public string Role { get; set; } = "";
        public string Content { get; set; } = "";
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

    public class SummaryRecord
    {
        public string Summary { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int FromRound { get; set; }
        public int ToRound { get; set; }
    }

    public class LongTermMemory
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Content { get; set; } = "";
        public string Category { get; set; } = "general";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public List<string> Keywords { get; set; } = new List<string>();
    }

    public static class MemoryConfig
    {
        private static readonly string ConfigPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "YuukaAI", "memory_config.json");

        public static bool EnableMemory { get; set; } = true;
        public static bool EnableShortTerm { get; set; } = true;
        public static bool EnableSummary { get; set; } = true;
        public static bool EnableVectorStore { get; set; } = true;
        
        public static int ShortTermCount { get; set; } = 20;
        public static int SummaryInterval { get; set; } = 50;
        
        public static string? DashScopeApiKey { get; set; }
        public static string? VectorStoreApiKey { get; set; }
        
        public static string? VectorCollectionName { get; set; } = "yuukai_memory";

        public static string GetEffectiveDashScopeApiKey()
        {
            if (!string.IsNullOrWhiteSpace(DashScopeApiKey))
                return DashScopeApiKey;
            if (!string.IsNullOrWhiteSpace(ThemeConfig.ApiKey))
                return ThemeConfig.ApiKey;
            return ThemeConfig.DefaultApiKey;
        }

        public static void Load()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    var json = File.ReadAllText(ConfigPath);
                    var data = JsonConvert.DeserializeObject<MemoryConfigData>(json);
                    if (data != null)
                    {
                        EnableMemory = data.EnableMemory;
                        EnableShortTerm = data.EnableShortTerm;
                        EnableSummary = data.EnableSummary;
                        EnableVectorStore = data.EnableVectorStore;
                        ShortTermCount = data.ShortTermCount;
                        SummaryInterval = data.SummaryInterval;
                        DashScopeApiKey = data.DashScopeApiKey;
                        VectorStoreApiKey = data.VectorStoreApiKey;
                        VectorCollectionName = data.VectorCollectionName;
                    }
                }
            }
            catch { }
        }

        public static void Save()
        {
            try
            {
                var dir = Path.GetDirectoryName(ConfigPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) 
                    Directory.CreateDirectory(dir);

                var data = new MemoryConfigData
                {
                    EnableMemory = EnableMemory,
                    EnableShortTerm = EnableShortTerm,
                    EnableSummary = EnableSummary,
                    EnableVectorStore = EnableVectorStore,
                    ShortTermCount = ShortTermCount,
                    SummaryInterval = SummaryInterval,
                    DashScopeApiKey = DashScopeApiKey,
                    VectorStoreApiKey = VectorStoreApiKey,
                    VectorCollectionName = VectorCollectionName
                };

                File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(data, Formatting.Indented));
            }
            catch { }
        }

        public static string GetMemoryStoragePath()
        {
            var path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "YuukaAI", "memory");
            
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            
            return path;
        }

        public static string GetHistoryPath() => Path.Combine(GetMemoryStoragePath(), "history.json");
        public static string GetSummaryPath() => Path.Combine(GetMemoryStoragePath(), "summary.json");
        public static string GetLongTermPath() => Path.Combine(GetMemoryStoragePath(), "longterm.json");
    }
}
