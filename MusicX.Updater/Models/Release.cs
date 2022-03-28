using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MusicX.Updater.Models
{
    public class Release
    {
        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("assets_url")]
        public string AssetsUrl { get; set; }

        [JsonPropertyName("upload_url")]
        public string UploadUrl { get; set; }

        [JsonPropertyName("html_url")]
        public string HtmlUrl { get; set; }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("node_id")]
        public string NodeId { get; set; }

        [JsonPropertyName("tag_name")]
        public string TagName { get; set; }

        [JsonPropertyName("target_commitish")]
        public string TargetCommitish { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("draft")]
        public bool Draft { get; set; }

        [JsonPropertyName("prerelease")]
        public bool Prerelease { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("published_at")]
        public DateTime PublishedAt { get; set; }

        [JsonPropertyName("assets")]
        public List<Asset> Assets { get; set; }

        [JsonPropertyName("tarball_url")]
        public string TarballUrl { get; set; }

        [JsonPropertyName("zipball_url")]
        public string ZipballUrl { get; set; }

        [JsonPropertyName("body")]
        public string Body { get; set; }
    }
}
