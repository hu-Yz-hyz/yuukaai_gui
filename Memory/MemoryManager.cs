using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using yuukaaigui.Core.Models;

namespace yuukaaigui.Memory
{
    public class MemoryManager
    {
        private readonly List<ConversationMessage> _conversationHistory = new List<ConversationMessage>();
        private SummaryRecord? _currentSummary;
        private readonly List<LongTermMemory> _longTermMemories = new List<LongTermMemory>();
        
        private int _totalRoundCount = 0;
        private bool _isInitialized = false;

        public event EventHandler<string>? OnSummaryUpdated;
        public event EventHandler<string>? OnLongTermMemoryStored;

        public async Task InitializeAsync()
        {
            if (_isInitialized) return;
            
            try
            {
                await LoadHistoryAsync();
                await LoadSummaryAsync();
                await LoadLongTermMemoriesAsync();
                
                // 从加载的历史记录恢复轮数计数
                _totalRoundCount = _conversationHistory.Count(m => m.Role == "user" || m.Role == "assistant");
                
                _isInitialized = true;
            }
            catch
            {
                _isInitialized = true;
            }
        }

        public async Task AddMessageAsync(string role, string content)
        {
            if (!MemoryConfig.EnableMemory || !MemoryConfig.EnableShortTerm) return;

            _conversationHistory.Add(new ConversationMessage
            {
                Role = role,
                Content = content,
                Timestamp = DateTime.Now
            });

            if (role == "user" || role == "assistant")
            {
                _totalRoundCount++;
            }

            await SaveHistoryAsync();

            if (MemoryConfig.EnableShortTerm && _conversationHistory.Count > MemoryConfig.ShortTermCount * 2)
            {
                TrimShortTermHistory();
            }

            if (MemoryConfig.EnableSummary && _totalRoundCount > 0 && _totalRoundCount % MemoryConfig.SummaryInterval == 0)
            {
                _ = TriggerSummaryAsync();
            }
        }

        private void TrimShortTermHistory()
        {
            if (_conversationHistory.Count <= MemoryConfig.ShortTermCount * 2) return;

            var keepCount = MemoryConfig.ShortTermCount * 2;
            var toRemove = _conversationHistory.Take(_conversationHistory.Count - keepCount).ToList();
            _conversationHistory.RemoveRange(0, _conversationHistory.Count - keepCount);
        }

        public List<Message> BuildContextForModel()
        {
            var messages = new List<Message>();

            if (!MemoryConfig.EnableMemory)
            {
                return messages;
            }

            // 如果启用了短期记忆，直接返回所有历史记录（其中已包含摘要消息）
            if (MemoryConfig.EnableShortTerm)
            {
                foreach (var msg in _conversationHistory)
                {
                    messages.Add(new Message
                    {
                        Role = msg.Role,
                        Content = msg.Content
                    });
                }
            }
            // 如果只启用了摘要但没有短期记忆，添加摘要
            else if (MemoryConfig.EnableSummary && _currentSummary != null)
            {
                messages.Add(new Message
                {
                    Role = "system",
                    Content = $"[历史摘要] {_currentSummary.Summary}"
                });
            }

            return messages;
        }

        public Task<string?> SearchLongTermMemoryAsync(string query)
        {
            if (!MemoryConfig.EnableMemory || !MemoryConfig.EnableVectorStore) 
                return Task.FromResult<string?>(null);

            if (string.IsNullOrWhiteSpace(query)) return Task.FromResult<string?>(null);

            // 如果没有长期记忆，直接返回
            if (_longTermMemories.Count == 0) return Task.FromResult<string?>(null);

            // 提取查询中的关键词
            var queryKeywords = ExtractKeywords(query);
            
            // 根据关键词匹配度、时间衰减和类别优先级排序
            var scoredMemories = _longTermMemories
                .Select(m => new 
                { 
                    Memory = m, 
                    Score = CalculateRelevanceScore(m, queryKeywords, query)
                })
                .OrderByDescending(x => x.Score)
                .Take(3)
                .ToList();

            // 返回评分最高的记忆，即使没有关键词匹配也返回（基础分数保底）
            if (scoredMemories.Count == 0 || scoredMemories[0].Score <= 0) 
                return Task.FromResult<string?>(null);

            var context = string.Join("\n", scoredMemories.Where(x => x.Score > 0).Select(m => $"- {m.Memory.Content}"));
            return Task.FromResult<string?>(context);
        }

        private List<string> ExtractKeywords(string text)
        {
            var keywords = new List<string>();
            if (string.IsNullOrWhiteSpace(text)) return keywords;
            
            // 清理文本
            var cleanText = text.ToLower();
            
            // 1. 提取中文词汇（2-6个字，更细粒度）
            // 2-3字词
            var chinesePattern2 = new System.Text.RegularExpressions.Regex(@"[\u4e00-\u9fa5]{2,3}");
            var matches2 = chinesePattern2.Matches(cleanText);
            foreach (System.Text.RegularExpressions.Match match in matches2)
            {
                keywords.Add(match.Value);
            }
            
            // 4-6字词（短语）
            var chinesePattern4 = new System.Text.RegularExpressions.Regex(@"[\u4e00-\u9fa5]{4,6}");
            var matches4 = chinesePattern4.Matches(cleanText);
            foreach (System.Text.RegularExpressions.Match match in matches4)
            {
                keywords.Add(match.Value);
            }

            // 2. 提取英文单词（2个字母以上）
            var englishPattern = new System.Text.RegularExpressions.Regex(@"[a-zA-Z]{2,}");
            var matches = englishPattern.Matches(cleanText);
            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                keywords.Add(match.Value.ToLower());
            }
            
            // 3. 提取数字（年龄、日期等）
            var numberPattern = new System.Text.RegularExpressions.Regex(@"\d+");
            var numberMatches = numberPattern.Matches(cleanText);
            foreach (System.Text.RegularExpressions.Match match in numberMatches)
            {
                keywords.Add(match.Value);
            }
            
            // 4. 提取特定类型的词（姓名、地点等）
            // 提取"用户"开头的词
            var userPattern = new System.Text.RegularExpressions.Regex(@"用户[\u4e00-\u9fa5]{2,4}");
            var userMatches = userPattern.Matches(text);
            foreach (System.Text.RegularExpressions.Match match in userMatches)
            {
                keywords.Add(match.Value);
            }
            
            // 5. 添加单字关键词（对于重要的单字）
            var importantSingleChars = new[] { "名", "姓", "岁", "年", "月", "日", "工", "爱", "喜", "厌", "怕" };
            foreach (var ch in importantSingleChars)
            {
                if (cleanText.Contains(ch))
                {
                    keywords.Add(ch);
                }
            }

            return keywords.Distinct().ToList();
        }

        private double CalculateRelevanceScore(LongTermMemory memory, List<string> queryKeywords, string originalQuery)
        {
            double score = 0;
            
            // 1. 内容关键词匹配分数
            var memoryContent = memory.Content.ToLower();
            var keywordMatches = queryKeywords.Count(k => memoryContent.Contains(k.ToLower()));
            score += keywordMatches * 10;
            
            // 2. 存储的关键词匹配（更高权重）
            if (memory.Keywords != null && memory.Keywords.Count > 0)
            {
                var storedKeywordMatches = queryKeywords.Count(k => 
                    memory.Keywords.Any(mk => mk.ToLower().Contains(k.ToLower())));
                score += storedKeywordMatches * 15; // 存储的关键词匹配权重更高
            }
            
            // 3. 完全匹配加分
            if (memoryContent.Contains(originalQuery.ToLower()))
            {
                score += 50;
            }
            
            // 4. 时间衰减（越新的记忆分数越高）
            var daysOld = (DateTime.Now - memory.CreatedAt).TotalDays;
            var timeDecay = Math.Max(0, 1 - (daysOld / 30)); // 30天后完全衰减
            score *= (0.5 + 0.5 * timeDecay); // 保留至少50%的分数
            
            // 5. 类别优先级
            if (memory.Category == "preference") score *= 1.2; // 用户偏好优先
            if (memory.Category == "important") score *= 1.5;  // 重要信息最高优先级
            
            return score;
        }

        public Task StoreLongTermMemoryAsync(string content, string category = "general")
        {
            if (!MemoryConfig.EnableMemory || !MemoryConfig.EnableVectorStore) return Task.CompletedTask;
            if (string.IsNullOrWhiteSpace(content)) return Task.CompletedTask;

            // 提取关键词用于后续搜索
            var keywords = ExtractKeywords(content);
            
            var memory = new LongTermMemory
            {
                Content = content,
                Category = category,
                CreatedAt = DateTime.Now,
                Keywords = keywords
            };

            _longTermMemories.Add(memory);
            _ = SaveLongTermMemoriesAsync();
            
            OnLongTermMemoryStored?.Invoke(this, content);
            return Task.CompletedTask;
        }

        private async Task TriggerSummaryAsync()
        {
            if (!MemoryConfig.EnableSummary) return;

            try
            {
                var apiKey = MemoryConfig.GetEffectiveDashScopeApiKey();
                var summaryService = new SummaryService(apiKey);
                
                // 获取需要摘要的对话（只取最近一个间隔的对话，排除系统消息和已有摘要）
                var userAssistantMessages = _conversationHistory
                    .Where(m => (m.Role == "user" || m.Role == "assistant") 
                        && !m.Content.StartsWith("[历史摘要]"))
                    .ToList();
                
                // 如果消息数量不足一个间隔，不生成摘要
                if (userAssistantMessages.Count < MemoryConfig.SummaryInterval) return;
                
                // 取最近一个间隔的对话进行摘要
                var messagesToSummarize = userAssistantMessages
                    .Skip(Math.Max(0, userAssistantMessages.Count - MemoryConfig.SummaryInterval))
                    .ToList();
                
                var historyText = string.Join("\n", messagesToSummarize.Select(m => 
                    $"{m.Role}: {m.Content}"));

                var summary = await summaryService.GenerateSummaryAsync(historyText);
                
                if (!string.IsNullOrWhiteSpace(summary))
                {
                    // 更新摘要记录
                    _currentSummary = new SummaryRecord
                    {
                        Summary = summary,
                        CreatedAt = DateTime.Now,
                        FromRound = _totalRoundCount - MemoryConfig.SummaryInterval,
                        ToRound = _totalRoundCount
                    };

                    await SaveSummaryAsync();
                    OnSummaryUpdated?.Invoke(this, summary);
                    
                    // 用摘要替换已摘要的对话
                    await ReplaceHistoryWithSummaryAsync(messagesToSummarize, summary);
                }
            }
            catch { }
        }

        private async Task ReplaceHistoryWithSummaryAsync(List<ConversationMessage> summarizedMessages, string summary)
        {
            if (summarizedMessages.Count == 0) return;
            
            // 检查是否已存在摘要消息
            var existingSummaryIndex = _conversationHistory.FindIndex(m => 
                m.Role == "system" && m.Content.StartsWith("[历史摘要]"));
            
            // 如果已存在摘要，更新它
            if (existingSummaryIndex != -1)
            {
                _conversationHistory[existingSummaryIndex].Content = $"[历史摘要] {summary}";
                _conversationHistory[existingSummaryIndex].Timestamp = DateTime.Now;
            }
            else
            {
                // 不存在摘要，添加新的摘要消息到开头
                _conversationHistory.Insert(0, new ConversationMessage
                {
                    Role = "system",
                    Content = $"[历史摘要] {summary}",
                    Timestamp = DateTime.Now
                });
            }
            
            // 移除已摘要的用户/助手消息
            foreach (var msg in summarizedMessages)
            {
                var index = _conversationHistory.FindIndex(m => 
                    m.Role == msg.Role && m.Content == msg.Content);
                if (index != -1)
                {
                    _conversationHistory.RemoveAt(index);
                }
            }
            
            // 保存更新后的历史
            await SaveHistoryAsync();
        }

        private async Task LoadHistoryAsync()
        {
            try
            {
                var path = MemoryConfig.GetHistoryPath();
                if (File.Exists(path))
                {
                    var json = await File.ReadAllTextAsync(path);
                    var history = JsonConvert.DeserializeObject<List<ConversationMessage>>(json);
                    if (history != null)
                    {
                        _conversationHistory.Clear();
                        _conversationHistory.AddRange(history);
                    }
                }
            }
            catch { }
        }

        private async Task SaveHistoryAsync()
        {
            try
            {
                var path = MemoryConfig.GetHistoryPath();
                var json = JsonConvert.SerializeObject(_conversationHistory, Formatting.Indented);
                await File.WriteAllTextAsync(path, json);
            }
            catch { }
        }

        private async Task LoadSummaryAsync()
        {
            try
            {
                var path = MemoryConfig.GetSummaryPath();
                if (File.Exists(path))
                {
                    var json = await File.ReadAllTextAsync(path);
                    _currentSummary = JsonConvert.DeserializeObject<SummaryRecord>(json);
                }
            }
            catch { }
        }

        private async Task SaveSummaryAsync()
        {
            try
            {
                var path = MemoryConfig.GetSummaryPath();
                var json = JsonConvert.SerializeObject(_currentSummary, Formatting.Indented);
                await File.WriteAllTextAsync(path, json);
            }
            catch { }
        }

        private async Task LoadLongTermMemoriesAsync()
        {
            try
            {
                var path = MemoryConfig.GetLongTermPath();
                if (File.Exists(path))
                {
                    var json = await File.ReadAllTextAsync(path);
                    var memories = JsonConvert.DeserializeObject<List<LongTermMemory>>(json);
                    if (memories != null)
                    {
                        _longTermMemories.Clear();
                        _longTermMemories.AddRange(memories);
                    }
                }
            }
            catch { }
        }

        private async Task SaveLongTermMemoriesAsync()
        {
            try
            {
                var path = MemoryConfig.GetLongTermPath();
                var json = JsonConvert.SerializeObject(_longTermMemories, Formatting.Indented);
                await File.WriteAllTextAsync(path, json);
            }
            catch { }
        }

        public Task ClearAllMemoryAsync()
        {
            _conversationHistory.Clear();
            _currentSummary = null;
            _longTermMemories.Clear();
            _totalRoundCount = 0;

            try
            {
                var historyPath = MemoryConfig.GetHistoryPath();
                var summaryPath = MemoryConfig.GetSummaryPath();
                var longTermPath = MemoryConfig.GetLongTermPath();

                if (File.Exists(historyPath)) File.Delete(historyPath);
                if (File.Exists(summaryPath)) File.Delete(summaryPath);
                if (File.Exists(longTermPath)) File.Delete(longTermPath);
            }
            catch { }

            return Task.CompletedTask;
        }

        public int GetTotalRoundCount() => _totalRoundCount;
        public int GetHistoryCount() => _conversationHistory.Count;
        public SummaryRecord? GetCurrentSummary() => _currentSummary;
        public int GetLongTermMemoryCount() => _longTermMemories.Count;
    }
}
