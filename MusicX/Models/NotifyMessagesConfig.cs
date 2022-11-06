using Newtonsoft.Json;

namespace MusicX.Models
{
    public class NotifyMessagesConfig
    {
        [JsonProperty("show_listen_together_modal")]
        public bool ShowListenTogetherModal { get; set; }

        [JsonProperty("show_telegram_block")]
        public bool ShowTelegramBlock { get; set; }
    }
}
