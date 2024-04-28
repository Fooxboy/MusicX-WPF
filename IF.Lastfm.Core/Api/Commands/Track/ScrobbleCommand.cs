﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using IF.Lastfm.Core.Api.Enums;
using IF.Lastfm.Core.Api.Helpers;
using IF.Lastfm.Core.Objects;
using IF.Lastfm.Core.Scrobblers;

namespace IF.Lastfm.Core.Api.Commands.Track
{
    [ApiMethodName("track.scrobble")]
    internal class ScrobbleCommand : PostAsyncCommandBase<ScrobbleResponse>
    {
        public IList<Scrobble> Scrobbles { get; private set; }

        public ScrobbleCommand(ILastAuth auth, IList<Scrobble> scrobbles)
            : base(auth)
        {
            if (scrobbles.Count > 50)
            {
                throw new ArgumentOutOfRangeException("scrobbles", "Only 50 scrobbles can be sent at a time");
            }

            Scrobbles = scrobbles;
        }

        protected override Uri BuildRequestUrl()
        {
            return new Uri(LastFm.ApiRootSsl, UriKind.Absolute);
        }

        public ScrobbleCommand(ILastAuth auth, Scrobble scrobble)
            : this(auth, new []{scrobble})
        {
        }

        public override void SetParameters()
        {
            for(int i = 0; i < Scrobbles.Count; i++)
            {
                var scrobble = Scrobbles[i];

                Parameters.Add(String.Format("artist[{0}]", i), scrobble.Artist);
                Parameters.Add(String.Format("album[{0}]", i), scrobble.Album);
                Parameters.Add(String.Format("track[{0}]", i), scrobble.Track);
                Parameters.Add(String.Format("albumArtist[{0}]", i), scrobble.AlbumArtist);
                Parameters.Add(String.Format("chosenByUser[{0}]", i), Convert.ToInt32(scrobble.ChosenByUser).ToString());
                Parameters.Add(String.Format("timestamp[{0}]", i), scrobble.TimePlayed.AsUnixTime().ToString());
            }
        }

        public override async Task<ScrobbleResponse> HandleResponse(HttpResponseMessage response)
        {
            var json = await response.Content.ReadAsStringAsync();

            LastResponseStatus status;
            if (LastFm.IsResponseValid(json, out status) && response.IsSuccessStatusCode)
            {
                return await ScrobbleResponse.CreateSuccessResponse(json);
            }
            else
            {
                return LastResponse.CreateErrorResponse<ScrobbleResponse>(status);
            }
        }
    }
}