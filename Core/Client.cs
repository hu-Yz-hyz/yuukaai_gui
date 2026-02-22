//core V1.3.0
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using yuukaai.Core.Models;

namespace yuukaai.Core
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
        }

        public async Task<string> SendMessageAsync(string userInput)
        {
            if (string.IsNullOrWhiteSpace(userInput))
                throw new ArgumentException("输入内容不能为空", nameof(userInput));

            try
            {
                _conversationHistory.Add(new Message { Role = "user", Content = userInput });

                //模型
                var requestBody = new
                {
                    model = "deepseek-v3.2",
                    messages = _conversationHistory,
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
                    return assistantReply;
                }
                else
                {
                    throw new Exception($"API请求失败，可能是过时的版本 也可能是自定义APIKEY错误 当前CORE版本V1.3.0 GUI版本 V2.0.0 : {response.StatusCode} - {responseContent}");
                }
            }
            catch (Exception)
            {
                _conversationHistory.RemoveAt(_conversationHistory.Count - 1);
                throw;
            }
        }

        public void ResetConversation()
        {
            _conversationHistory.Clear();
            _conversationHistory.Add(new Message { Role = "system", Content = _characterPrompt });
        }
    }
}