﻿using System.Net.Http;
using System.Threading.Tasks;
using IF.Lastfm.Core.Api.Helpers;

namespace IF.Lastfm.Core.Api.Commands.Track
{
    [ApiMethodName("track.shout")]
    internal class AddShoutCommand : PostAsyncCommandBase<LastResponse>
    {
        public string Track { get; set; }

        public string Artist { get; set; }

        public string Message { get; set; }


        public AddShoutCommand(ILastAuth auth, string track, string artist, string message) : base(auth)
        {
            Track = track;
            Artist = artist;
            Message = message;
        }

        public override void SetParameters()
        {
            Parameters.Add("track", Track);
            Parameters.Add("artist", Artist);
            Parameters.Add("message", Message);
        }

        public override async Task<LastResponse> HandleResponse(HttpResponseMessage response)
        {
            return await LastResponse.HandleResponse(response);
        }
    }
}
