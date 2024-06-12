using MusicX.Core.Helpers;
using Newtonsoft.Json;
using VkNet.Model;

namespace MusicX.Core.Models;

public class AudioFollowingsUpdateInfo : IIdentifiable
{
    string IIdentifiable.Identifier => Id;

    [JsonProperty("title")]
    public string Title { get; set; }
    
    [JsonProperty("id")]
    public string Id { get; set; }
    
    [JsonProperty("covers")]
    public List<AudioCover> Covers { get; set; }
}