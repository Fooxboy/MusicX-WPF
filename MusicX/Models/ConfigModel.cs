using System;
using System.Collections.Generic;
using Newtonsoft.Json;

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
        
        [JsonProperty("is_muted")]
        public bool IsMuted { get; set; }

        [JsonProperty("broadcast_vk")]
        public bool? BroadcastVK { get; set; }
        
        [JsonProperty("download_directory")]
        public string? DownloadDirectory { get; set; }

        [JsonProperty("boom_token")]
        public string BoomToken { get; set; }
        
        [JsonProperty("boom_token_ttl")]
        public DateTimeOffset BoomTokenTtl { get; set; }

        [JsonProperty("boom_uuid")]
        public string BoomUuid { get; set; }

        [JsonProperty("notify_messages")]
        public NotifyMessagesConfig NotifyMessages { get; set; }

        [JsonProperty("ignored_artists")]
        public List<string>? IgnoredArtists { get; set; }
    }
}
