using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MusicX.Core.Models
{
    public class Album
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("owner_id")]
        public long OwnerId { get; set; }

        [JsonProperty("access_key")]
        public string AccessKey { get; set; }

        [JsonProperty("thumb")]
        public Photo Thumb { get; set; }

        public Uri? Cover
        {
            get
            {
                var url = Thumb switch
                {
                    { Photo68: { } photo68 } => photo68,
                    { Photo135: { } photo135 } => photo135,
                    { Photo270: { } photo270 } => photo270,
                    { Photo300: { } photo300 } => photo300,
                    { Photo600: { } photo600 } => photo600,
                    { Photo1200: { } photo1200 } => photo1200,
                    _ => null
                };
                
                return url is null ? null : new(url);
            }
        }
    }
}
