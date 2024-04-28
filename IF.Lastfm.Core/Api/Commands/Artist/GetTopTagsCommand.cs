using System;
using System.Net.Http;
using System.Threading.Tasks;
using IF.Lastfm.Core.Api.Enums;
using IF.Lastfm.Core.Api.Helpers;
using IF.Lastfm.Core.Objects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IF.Lastfm.Core.Api.Commands.Artist
{
    [ApiMethodName("artist.getTopTags")]
    internal class GetTopTagsCommand : GetAsyncCommandBase<PageResponse<LastTag>>
    {
        public string ArtistMbid { get; set; }

        public string ArtistName { get; set; }

        public bool Autocorrect { get; set; }

        public GetTopTagsCommand(ILastAuth auth) : base(auth)
        {
        }
        
        public override void SetParameters()
        {
            var hasMbid = !string.IsNullOrEmpty(ArtistMbid);
            var hasName = !string.IsNullOrEmpty(ArtistName);

            if (!hasMbid && !hasName)
            {
                throw new InvalidOperationException($"Either {nameof(ArtistMbid)} or {nameof(ArtistName)} must be set");
            }

            if (hasMbid && hasName)
            {
                throw new InvalidOperationException($"");
            }

            if (hasMbid)
            {
                Parameters.Add("mbid", ArtistMbid);
            }
            else
            {
                Parameters.Add("artist", ArtistName);
            }

            Parameters.Add("autocorrect", Convert.ToInt32(Autocorrect).ToString());
        }

        public override async Task<PageResponse<LastTag>> HandleResponse(HttpResponseMessage response)
        {
            var json = await response.Content.ReadAsStringAsync();

            LastResponseStatus status;
            if (LastFm.IsResponseValid(json, out status) && response.IsSuccessStatusCode)
            {
                var jtoken = JsonConvert.DeserializeObject<JToken>(json);
                var resultsToken = jtoken.SelectToken("toptags");
                var itemsToken = resultsToken.SelectToken("tag");

                return PageResponse<LastTag>.CreateSuccessResponse(itemsToken, LastTag.ParseJToken);
            }
            else
            {
                return PageResponse<LastTag>.CreateErrorResponse(status);
            }
        }
    }
}