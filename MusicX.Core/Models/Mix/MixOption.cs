using Newtonsoft.Json;
using System.IO.Compression;
using System.Text.Json.Nodes;

namespace MusicX.Core.Models.Mix
{
    public class MixOption
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }


        [JsonProperty("icon")]
        public string IconUri { get; set; }

        [JsonProperty("selected")]
        public bool Selected { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public byte[] ImageBytes
        {
            get
            {
               using(var httpClient = new HttpClient())
               {
                    httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");

                    var response = httpClient.GetAsync(IconUri).GetAwaiter().GetResult();

                    using var stream = response.Content.ReadAsStreamAsync().GetAwaiter().GetResult();
                    using var decompressedStream = new GZipStream(stream, CompressionMode.Decompress);
                    using var reader = new StreamReader(decompressedStream);
                    var lottieJson = reader.ReadToEndAsync().GetAwaiter().GetResult();


                    var jsonOnject = JsonObject.Parse(lottieJson);

                    var base64Image = jsonOnject["assets"][0]["p"].AsValue().ToString();

                    var imageBytes = Convert.FromBase64String(base64Image.Replace("data:image/png;base64,", string.Empty));

                    return imageBytes;
               }
            }
        }
    }
}
