using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace yuukaaigui.Memory
{
    public class SummaryService
    {
        private readonly string _apiKey;
        private readonly string _apiUrl = "https://dashscope.aliyuncs.com/compatible-mode/v1/chat/completions";
        private readonly HttpClient _httpClient;

        public SummaryService(string apiKey)
        {
            _apiKey = apiKey;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        }

        public async Task<string> GenerateSummaryAsync(string conversationHistory)
        {
            if (string.IsNullOrWhiteSpace(conversationHistory))
                return "";

            var prompt = $@"请将以下对话历史精简为不超过200字的重点摘要，保留关键信息、用户偏好、重要事实和决定：{conversationHistory}请直接输出摘要，不需要任何前缀或解释。";

            var requestBody = new
            {
                model = "qwen-flash",
                messages = new[]
                {
                    new { role = "system", content = "你是一个对话摘要助手，请用简洁的语言总结对话要点。" },
                    new { role = "user", content = prompt }
                },
                temperature = 0.3,
                max_tokens = 500
            };

            var jsonBody = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync(_apiUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<JObject>(responseContent);
                    var summary = result?["choices"]?[0]?["message"]?["content"]?.ToString();
                    return summary ?? "";
                }
            }
            catch
            {
            }

            return "";
        }
    }
}
