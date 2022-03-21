using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MusicX.Core.Models.General
{
    public class ResponseVk
    {
        [JsonProperty("response")]
        public ResponseData Response { get; set; }

        [JsonProperty("error")]
        public ErrorVk Error { get; set; }
    }

    public class ResponseData
    {
        [JsonProperty("section")]
        public Section Section { get; set; }

        [JsonProperty("catalog")]
        public Catalog Catalog { get; set; }

        [JsonProperty("block")]
        public Block Block { get; set; }

        [JsonProperty("catalog_banners")]
        public List<CatalogBanner> CatalogBanners { get; set; }

        [JsonProperty("audios")]
        public List<Audio> Audios { get; set; }

        [JsonProperty("playlists")]
        public List<Playlist> Playlists { get; set; }

        [JsonProperty("playlist")]
        public Playlist Playlist { get; set; }

        [JsonProperty("links")]
        public List<Link> Links { get; set; }

        [JsonProperty("artists")]
        public List<Artist> Artists { get; set; }

        [JsonProperty("suggestions")]
        public List<Suggestion> Suggestions { get; set; }

        [JsonProperty("curators")]
        public List<Curator> Curators { get; set; }

        [JsonProperty("groups")]
        public List<Group> Groups { get; set; }

        [JsonProperty("texts")]
        public List<Text> Texts { get; set; }

        [JsonProperty("items")]
        public List<Audio> Items { get; set; }

        [JsonProperty("replacements")]
        public Replacements Replacements { get; set; }

    }
}
