using Newtonsoft.Json;
using System;

namespace MusicX.Models
{
    public class NotifyMessagesConfig
    {
        [JsonProperty("show_listen_together_modal")]
        public bool ShowListenTogetherModal { get; set; }

        [JsonProperty("last_showed_telegram_block")]
        public DateTime? LastShowedTelegramBlock { get; set; }
    }
}
