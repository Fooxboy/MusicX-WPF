using Newtonsoft.Json;

namespace MusicX.Core.Models.Boom
{
    public class Radio
    {
        [JsonProperty("currentClusterId")]
        public string CurrentClusterId { get; set; }

        [JsonProperty("clusters")]
        public List<Cluster> Clusters { get; set; }

        [JsonProperty("tracks")]
        public List<Track> Tracks { get; set; }

        [JsonProperty("artist")]
        public Artist Artist { get; set; }

        [JsonProperty("tag")]
        public Tag Tag { get; set; }

        [JsonProperty("currentCluster")]
        public Cluster CurrentCluster { get; set; }

        [JsonProperty("apiId")]
        public string ApiId { get; set; }
    }
}
