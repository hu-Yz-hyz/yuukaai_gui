using Newtonsoft.Json;
using System.Collections.Generic;
using yuukaaigui.Core.Models;

namespace yuukaaigui.Core.Models
{
    public class ChatResponse
    {
        [JsonProperty("choices")]
        public List<ChatChoice>? Choices { get; set; }
    }
}