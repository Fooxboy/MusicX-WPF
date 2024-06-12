using Newtonsoft.Json;
using MusicX.Core.Models.Abstractions;

namespace MusicX.Core.Models;

public class Curator : IBlockEntity<long>
{
    [JsonProperty("id")]
    public long Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("url")]
    public string Url { get; set; }

    [JsonProperty("photo")]
    public List<Image> Photo { get; set; }

    [JsonProperty("is_followed")]
    public bool IsFollowed { get; set; }

    [JsonProperty("can_follow")]
    public bool CanFollow { get; set; }
}