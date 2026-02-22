using Newtonsoft.Json;
using yuukaai.Core.Models;

namespace yuukaai.Core.Models
{
    public class ChatChoice
    {
        [JsonProperty("message")]
        public Message? Message { get; set; }
    }
}