using Newtonsoft.Json;
using MusicX.Core.Helpers;

namespace MusicX.Core.Models
{
    public class PodcastSliderItem : IIdentifiable
    {
        string IIdentifiable.Identifier => ItemId;

        [JsonProperty("item_id")]
        public string ItemId { get; set; }

        [JsonProperty("slider_type")]
        public string SliderType { get; set; }

        [JsonProperty("episode")]
        public PodcastEpisode Episode { get; set; }
    }
}
