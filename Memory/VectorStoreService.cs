using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace yuukaaigui.Memory
{
    public class VectorStoreService
    {
        private readonly string? _apiKey;
        private readonly string? _collectionName;
        private readonly HttpClient _httpClient;
        private readonly List<VectorEntry> _localVectors = new List<VectorEntry>();
        private bool _isInitialized = false;

        private const string DashScopeApiUrl = "https://dashscope.aliyuncs.com/api/v1/services/embeddings/text-embedding";

        public VectorStoreService(string? apiKey, string? collectionName)
        {
            _apiKey = apiKey;
            _collectionName = collectionName ?? "yuukai_memory";
            _httpClient = new HttpClient();
            
            if (!string.IsNullOrEmpty(_apiKey))
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            }
        }

        public async Task InitializeAsync()
        {
            if (_isInitialized) return;
            await LoadLocalVectorsAsync();
            _isInitialized = true;
        }

        public async Task<string> SearchAsync(string query, int topK = 3)
        {
            if (string.IsNullOrWhiteSpace(query)) return "";

            try
            {
                if (!string.IsNullOrEmpty(_apiKey))
                {
                    return await SearchWithDashScopeAsync(query, topK);
                }
            }
            catch { }

            return SearchLocalSimple(query, topK);
        }

        private async Task<string> SearchWithDashScopeAsync(string query, int topK)
        {
            try
            {
                // 获取查询的向量嵌入
                var queryVector = await GetEmbeddingAsync(query);
                
                if (queryVector.Count > 0)
                {
                    return SearchByVector(queryVector, topK);
                }
            }
            catch { }

            // 如果向量搜索失败，回退到本地关键词搜索
            return SearchLocalSimple(query, topK);
        }

        private string SearchByVector(List<double> queryVector, int topK)
        {
            if (_localVectors.Count == 0) return "";

            var scored = _localVectors
                .Select(v => new { Entry = v, Score = CosineSimilarity(queryVector, v.Vector) })
                .OrderByDescending(x => x.Score)
                .Take(topK)
                .ToList();

            if (scored.Count == 0 || scored[0].Score < 0.3) 
                return "";

            var results = scored.Where(x => x.Score > 0.3).Select(x => x.Entry.Content);
            return string.Join("\n", results);
        }

        private string SearchLocalSimple(string query, int topK)
        {
            if (_localVectors.Count == 0) return "";

            var keywords = ExtractKeywords(query);
            
            var scored = _localVectors
                .Select(v => new 
                { 
                    Entry = v, 
                    Score = CalculateKeywordScore(v.Content, keywords) 
                })
                .OrderByDescending(x => x.Score)
                .Take(topK)
                .ToList();

            if (scored.Count == 0 || scored[0].Score <= 0) 
                return "";

            var results = scored.Where(x => x.Score > 0).Select(x => x.Entry.Content);
            return string.Join("\n", results);
        }

        private double CalculateKeywordScore(string content, List<string> keywords)
        {
            if (keywords.Count == 0) return 0;

            var contentLower = content.ToLower();
            var matchCount = keywords.Count(k => contentLower.Contains(k.ToLower()));
            
            return (double)matchCount / keywords.Count;
        }

        private List<string> ExtractKeywords(string text)
        {
            var keywords = new List<string>();
            
            var namePattern = new Regex(@"[\u4e00-\u9fa5]{2,4}");
            var matches = namePattern.Matches(text);
            foreach (Match match in matches)
            {
                keywords.Add(match.Value);
            }

            var englishPattern = new Regex(@"[a-zA-Z]{3,}");
            matches = englishPattern.Matches(text);
            foreach (Match match in matches)
            {
                keywords.Add(match.Value);
            }

            return keywords.Distinct().Take(10).ToList();
        }

        private double CosineSimilarity(List<double> a, List<double> b)
        {
            if (a.Count != b.Count) return 0;
            if (a.Count == 0) return 0;

            var dotProduct = a.Zip(b, (x, y) => x * y).Sum();
            var magnitudeA = Math.Sqrt(a.Sum(x => x * x));
            var magnitudeB = Math.Sqrt(b.Sum(x => x * x));

            if (magnitudeA == 0 || magnitudeB == 0) return 0;

            return dotProduct / (magnitudeA * magnitudeB);
        }

        private async Task<List<double>> GetEmbeddingAsync(string text)
        {
            var requestBody = new
            {
                model = "text-embedding-v3",
                input = text
            };

            var jsonBody = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(DashScopeApiUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonConvert.DeserializeObject<JObject>(responseContent);
                var embedding = result?["data"]?[0]?["embedding"]?.ToObject<List<double>>();
                return embedding ?? new List<double>();
            }

            return new List<double>();
        }

        public async Task StoreAsync(string content, string category = "general")
        {
            if (string.IsNullOrWhiteSpace(content)) return;

            var entry = new VectorEntry
            {
                Id = Guid.NewGuid().ToString(),
                Content = content,
                Category = category,
                CreatedAt = DateTime.Now,
                Keywords = ExtractKeywords(content)
            };

            if (!string.IsNullOrEmpty(_apiKey))
            {
                try
                {
                    entry.Vector = await GetEmbeddingAsync(content);
                }
                catch
                {
                    entry.Vector = new List<double>();
                }
            }

            _localVectors.Add(entry);
            await SaveLocalVectorsAsync();
        }

        public Task ClearAsync()
        {
            _localVectors.Clear();
            
            try
            {
                var path = MemoryConfig.GetLongTermPath();
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch { }
            
            return Task.CompletedTask;
        }

        private async Task LoadLocalVectorsAsync()
        {
            try
            {
                var path = MemoryConfig.GetLongTermPath();
                if (File.Exists(path))
                {
                    var json = await File.ReadAllTextAsync(path);
                    var vectors = JsonConvert.DeserializeObject<List<VectorEntry>>(json);
                    if (vectors != null)
                    {
                        _localVectors.Clear();
                        _localVectors.AddRange(vectors);
                    }
                }
            }
            catch { }
        }

        private async Task SaveLocalVectorsAsync()
        {
            try
            {
                var path = MemoryConfig.GetLongTermPath();
                var json = JsonConvert.SerializeObject(_localVectors, Formatting.Indented);
                await File.WriteAllTextAsync(path, json);
            }
            catch { }
        }

        public int GetCount() => _localVectors.Count;
    }

    public class VectorEntry
    {
        public string Id { get; set; } = "";
        public string Content { get; set; } = "";
        public string Category { get; set; } = "general";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public List<double> Vector { get; set; } = new List<double>();
        public List<string> Keywords { get; set; } = new List<string>();
    }
}
