using Newtonsoft.Json;
using MusicX.Core.Models.Abstractions;

namespace MusicX.Core.Models;

public class PodcastSliderItem : IBlockEntity<string>
{
    [JsonProperty("item_id")]
    public string Id { get; set; }

    [JsonProperty("slider_type")]
    public string SliderType { get; set; }

    [JsonProperty("episode")]
    public PodcastEpisode Episode { get; set; }
}