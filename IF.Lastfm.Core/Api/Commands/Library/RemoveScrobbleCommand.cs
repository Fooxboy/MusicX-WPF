using System;
using System.Net.Http;
using System.Threading.Tasks;
using IF.Lastfm.Core.Api.Helpers;

namespace IF.Lastfm.Core.Api.Commands.Library {
    [ApiMethodName("library.removeScrobble")]
    internal class RemoveScrobbleCommand : PostAsyncCommandBase<LastResponse> {
        public string Artist { get; set; }

        public string Track { get; set; }
        public DateTimeOffset Timestamp { get; set; }

        public RemoveScrobbleCommand( ILastAuth auth, string artist, string track, DateTimeOffset timestamp ) : base( auth ) {
            Artist = artist;
            Track = track;
            Timestamp = timestamp;
        }


        public override void SetParameters() {
            Parameters.Add( "artist", Artist );
            Parameters.Add( "track", Track );
            Parameters.Add( "timestamp", Timestamp.AsUnixTime().ToString() );
        }

        public override async Task<LastResponse> HandleResponse( HttpResponseMessage response ) {
            return await LastResponse.HandleResponse( response );
        }
    }
}
