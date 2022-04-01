﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicX.Core.Models
{
    public class PodcastInfo
    {
        [JsonProperty("cover")]
        public Cover Cover { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("is_favorite")]
        public bool IsFavorite { get; set; }

        [JsonProperty("plays")]
        public int Plays { get; set; }

        [JsonProperty("position")]
        public int Position { get; set; }

        [JsonProperty("post")]
        public string Post { get; set; }
    }
}
