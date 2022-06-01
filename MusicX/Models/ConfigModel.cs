using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicX.Models
{
    public class ConfigModel
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("user_id")]
        public long UserId { get; set; }

        [JsonProperty("user_name")]
        public string UserName { get; set; }

        [JsonProperty("show_rpc")]
        public bool? ShowRPC { get; set; }

        [JsonProperty("full_screen_monitor")]
        public int? FullScreenMonitor { get; set; }

        [JsonProperty("volume")]
        public int? Volume { get; set; }

        [JsonProperty("broadcast_vk")]
        public bool? BroadcastVK { get; set; }
    }
}
