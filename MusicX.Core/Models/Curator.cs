using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicX.Core.Helpers;

namespace MusicX.Core.Models
{
    public class Curator : IIdentifiable
    {
        string IIdentifiable.Identifier => Id.ToString();
        
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
}
