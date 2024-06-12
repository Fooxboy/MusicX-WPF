using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicX.Core.Helpers;

namespace MusicX.Core.Models
{
    public class Placeholder : IIdentifiable
    {
        string IIdentifiable.Identifier => Id;

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("icons")]
        public List<Image> Icons { get; set; } = new List<Image>();

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("buttons")]
        public List<Button> Buttons { get; set; } = new List<Button>();
    }
}
