﻿using System;
using System.Net.Http;
using System.Threading.Tasks;
using IF.Lastfm.Core.Api.Enums;
using IF.Lastfm.Core.Api.Helpers;
using IF.Lastfm.Core.Objects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IF.Lastfm.Core.Api.Commands.Artist
{
    [ApiMethodName("artist.getSimilar")]
    internal class GetSimilarCommand : GetAsyncCommandBase<PageResponse<LastArtist>>
    {
        public bool Autocorrect { get; set; }

        public string ArtistMbid { get; set; }

        public string ArtistName { get; set; }

        public int? Limit { get; set; }

        public GetSimilarCommand(ILastAuth auth)
            : base(auth){}


        public override void SetParameters()
        {

            if (ArtistMbid != null)
            {
                Parameters.Add("mbid", ArtistMbid);
            }
            else
            {
                Parameters.Add("artist", ArtistName);
            }

            Parameters.Add("autocorrect", Convert.ToInt32(Autocorrect).ToString());

            if (Limit != null)
            {
                Parameters.Add("limit", Limit.ToString());
            }

            DisableCaching();
        }

        public override async Task<PageResponse<LastArtist>> HandleResponse(HttpResponseMessage response)
        {
            var json = await response.Content.ReadAsStringAsync();

            LastResponseStatus status;
            if (LastFm.IsResponseValid(json, out status) && response.IsSuccessStatusCode)
            {
                var jtoken = JsonConvert.DeserializeObject<JToken>(json);
                var itemsToken = jtoken.SelectToken("similarartists").SelectToken("artist");

                return PageResponse<LastArtist>.CreateSuccessResponse(itemsToken, LastArtist.ParseJToken);
            }
            else
            {
                return LastResponse.CreateErrorResponse<PageResponse<LastArtist>>(status);
            }
        }
    }
}
