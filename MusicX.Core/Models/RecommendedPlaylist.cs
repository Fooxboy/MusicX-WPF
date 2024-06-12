using MusicX.Core.Helpers;
using Newtonsoft.Json;


namespace MusicX.Core.Models
{
    public class RecommendedPlaylist : IIdentifiable
    {
        string IIdentifiable.Identifier => $"{OwnerId}_{Id}";

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("owner_id")]
        public long OwnerId { get; set; }

        [JsonProperty("percentage")]
        public string Percentage { get; set; }

        [JsonProperty("percentage_title")]
        public string PercentageTitle { get; set; }

        [JsonProperty("audios")]
        public List<string> AudiosIds { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; }

        [JsonProperty("cover")]
        public string Cover { get; set; }

        [JsonIgnore]
        public Playlist Playlist { get; set; }

        [JsonIgnore]
        public List<Audio> Audios { get; set; } = new List<Audio>();
    }
}
