//core V2.0.0
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using yuukaaigui.Core.Models;
using yuukaaigui.Memory;

namespace yuukaaigui.Core
{
    public interface IChatClient
    {
        Task<string> SendMessageAsync(string userInput);
        void ResetConversation();
    }

    public class Client : IChatClient
    {
        private readonly string _apiKey;
        private readonly string _apiUrl;
        private readonly List<Message> _conversationHistory = new List<Message>();
        private readonly HttpClient _httpClient;
        private readonly string _characterPrompt;
        private readonly MemoryManager? _memoryManager;
        
        public Client(string apiKey, string apiUrl, string characterPrompt)
        {
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _apiUrl = apiUrl ?? throw new ArgumentNullException(nameof(apiUrl));
            _characterPrompt = characterPrompt ?? throw new ArgumentNullException(nameof(characterPrompt));
            
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            
            _conversationHistory.Add(new Message 
            { 
                Role = "system", 
                Content = _characterPrompt 
            });

            if (MemoryConfig.EnableMemory)
            {
                _memoryManager = new MemoryManager();
                _ = _memoryManager.InitializeAsync();
            }
        }

        public async Task<string> SendMessageAsync(string userInput)
        {
            if (string.IsNullOrWhiteSpace(userInput))
                throw new ArgumentException("输入内容不能为空", nameof(userInput));

            try
            {
                _conversationHistory.Add(new Message { Role = "user", Content = userInput });

                var messagesToSend = new List<Message>();

                if (_memoryManager != null && MemoryConfig.EnableMemory)
                {
                    await _memoryManager.AddMessageAsync("user", userInput);
                    
                    // 构建记忆上下文
                    var memoryContext = _memoryManager.BuildContextForModel();
                    
                    // 添加系统提示词（只添加一次）
                    messagesToSend.Add(new Message { Role = "system", Content = _characterPrompt });
                    
                    // 添加历史摘要（如果有）
                    var summaryMsg = memoryContext.FirstOrDefault(m => m.Role == "system" && m.Content?.Contains("[历史摘要]") == true);
                    if (summaryMsg != null)
                    {
                        messagesToSend.Add(summaryMsg);
                    }
                    
                    // 添加相关长期记忆
                    var longTermMemory = await _memoryManager.SearchLongTermMemoryAsync(userInput);
                    if (!string.IsNullOrWhiteSpace(longTermMemory))
                    {
                        messagesToSend.Add(new Message
                        {
                            Role = "system",
                            Content = $"[相关记忆] {longTermMemory}"
                        });
                    }
                    
                    // 添加短期记忆（最近的对话）
                    foreach (var msg in memoryContext.Where(m => m.Role != "system" || m.Content?.Contains("[历史摘要]") != true))
                    {
                        messagesToSend.Add(msg);
                    }
                }
                else
                {
                    // 未启用记忆时，使用完整对话历史
                    messagesToSend = new List<Message>(_conversationHistory);
                }

                var requestBody = new
                {
                    model = "deepseek-v3.2",
                    messages = messagesToSend,
                    temperature = 1,
                    max_tokens = 5120,
                    enable_search = true
                };

                var jsonBody = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                
                var response = await _httpClient.PostAsync(_apiUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<ChatResponse>(responseContent);
                    var assistantReply = result?.Choices?[0]?.Message?.Content;
                    
                    if (assistantReply == null)
                        throw new InvalidOperationException("API返回格式异常");

                    _conversationHistory.Add(new Message { Role = "assistant", Content = assistantReply });

                    if (_memoryManager != null && MemoryConfig.EnableMemory)
                    {
                        await _memoryManager.AddMessageAsync("assistant", assistantReply);
                        
                        await ExtractAndStoreLongTermMemoryAsync(userInput, assistantReply);
                    }

                    return assistantReply;
                }
                else
                {
                    throw new Exception($"API请求失败，可能是过时的版本 也可能是自定义APIKEY错误 当前CORE版本V2.0.0 GUI版本 V2.1.0 : {response.StatusCode} - {responseContent}");
                }
            }
            catch (Exception)
            {
                if (_conversationHistory.Count > 0 && _conversationHistory[_conversationHistory.Count - 1].Role == "user")
                {
                    _conversationHistory.RemoveAt(_conversationHistory.Count - 1);
                }
                throw;
            }
        }

        private async Task ExtractAndStoreLongTermMemoryAsync(string userInput, string assistantReply)
        {
            if (_memoryManager == null) return;

            try
            {
                var apiKey = MemoryConfig.GetEffectiveDashScopeApiKey();
                
                var prompt = $@"请分析以下对话，提取所有需要长期记住的用户信息。用户说：{userInput}AI回复：{assistantReply}请提取以下类型的信息（如果有）：1. 用户姓名、昵称2. 生日、年龄3. 职业、工作4. 兴趣爱好5. 喜好偏好（喜欢的食物、颜色、音乐等）6. 不喜欢的内容7. 重要决定或计划8. 用户的观点、态度9. 用户的经历、背景10. 任何其他个性化信息输出格式要求：- 每条信息单独一行- 格式：用户[类型]：[具体内容]- 如果没有可提取的信息，只回复一个字：无示例输出：用户姓名：张三用户生日：1990年5月1日用户职业：软件工程师用户喜好：喜欢蓝色，喜欢喝咖啡用户不喜欢：不喜欢吃辣请提取：";

                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                var requestBody = new
                {
                    model = "qwen-flash",
                    messages = new[]
                    {
                        new { role = "system", content = "你是一个记忆提取助手。" },
                        new { role = "user", content = prompt }
                    },
                    temperature = 0.3,
                    max_tokens = 200
                };

                var jsonBody = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync("https://dashscope.aliyuncs.com/compatible-mode/v1/chat/completions", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<ChatResponse>(responseContent);
                    var extracted = result?.Choices?[0]?.Message?.Content;

                    if (!string.IsNullOrWhiteSpace(extracted) && extracted != "无" && extracted.Length > 2)
                    {
                        // 分行存储每条记忆，提高搜索精度
                        var lines = extracted.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                            .Select(l => l.Trim())
                            .Where(l => l.Length > 2 && !l.StartsWith("无"))
                            .ToList();
                        
                        foreach (var line in lines)
                        {
                            await _memoryManager.StoreLongTermMemoryAsync(line, "preference");
                        }
                    }
                }
            }
            catch { }
        }

        public void ResetConversation()
        {
            _conversationHistory.Clear();
            _conversationHistory.Add(new Message { Role = "system", Content = _characterPrompt });
        }
    }
}
