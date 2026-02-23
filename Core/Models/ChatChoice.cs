using Newtonsoft.Json;
using yuukaaigui.Core.Models;

namespace yuukaaigui.Core.Models
{
    public class ChatChoice
    {
        [JsonProperty("message")]
        public Message? Message { get; set; }
    }
}