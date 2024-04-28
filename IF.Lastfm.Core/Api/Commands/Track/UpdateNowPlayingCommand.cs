using System;
using System.Net.Http;
using System.Threading.Tasks;
using IF.Lastfm.Core.Api.Helpers;
using IF.Lastfm.Core.Objects;

namespace IF.Lastfm.Core.Api.Commands.Track
{
    [ApiMethodName("track.updateNowPlaying")]
    internal class UpdateNowPlayingCommand : PostAsyncCommandBase<LastResponse>
    {
        public string Artist { get; set; }

        public string Album { get; set; }

        public string Track { get; set; }

        public string AlbumArtist { get; set; }

        public bool ChosenByUser { get; set; }

        public TimeSpan? Duration { get; set; }

        public UpdateNowPlayingCommand(ILastAuth auth, string artist, string album, string track)
            : base(auth)
        {
            Artist = artist;
            Album = album;
            Track = track;
        }

        public UpdateNowPlayingCommand(ILastAuth auth, Scrobble scrobble)
            : this(auth, scrobble.Artist, scrobble.Album, scrobble.Track)
        {
            ChosenByUser = scrobble.ChosenByUser;
            Duration = scrobble.Duration;
        }

        public override void SetParameters()
        {
            Parameters.Add("artist", Artist);
            Parameters.Add("album", Album);
            Parameters.Add("track", Track);
            Parameters.Add("albumArtist", AlbumArtist);
            Parameters.Add("chosenByUser", Convert.ToInt32(ChosenByUser).ToString());

            if (Duration.HasValue)
            {
                Parameters.Add("duration",Math.Round(Duration.Value.TotalSeconds).ToString());
            }
        }

        public override Task<LastResponse> HandleResponse(HttpResponseMessage response)
        {
            return LastResponse.HandleResponse(response);
        }
    }
}