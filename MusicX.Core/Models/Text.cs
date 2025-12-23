using Newtonsoft.Json;
using MusicX.Core.Helpers;

namespace MusicX.Core.Models
{
    public class Text : IIdentifiable
    {
        string IIdentifiable.Identifier => Id;
        
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("text")]
        public string Value { get; set; }

        [JsonProperty("collapsed_lines")]
        public int CollapsedLines { get; set; }
    }
}
