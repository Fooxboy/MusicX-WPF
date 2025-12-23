using System.Text.Json.Serialization;

namespace MusicX.Core.Models.Mix
{
    public class MixSettingsRoot
    {
        [JsonPropertyName("settings")]
        public MixSettings Settings { get; set; }
    }
}
