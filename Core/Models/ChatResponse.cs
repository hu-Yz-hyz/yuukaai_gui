using Newtonsoft.Json;
using System.Collections.Generic;
using yuukaai.Core.Models;

namespace yuukaai.Core.Models
{
    public class ChatResponse
    {
        [JsonProperty("choices")]
        public List<ChatChoice>? Choices { get; set; }
    }
}