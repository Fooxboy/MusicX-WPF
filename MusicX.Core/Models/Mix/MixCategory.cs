using Newtonsoft.Json;

namespace MusicX.Core.Models.Mix
{
    public class MixCategory
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("options")]
        public List<MixOption> Options { get; set; }

    }
}
