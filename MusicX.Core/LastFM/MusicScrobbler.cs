using Lpfm.LastFmScrobbler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace MusicX.Core.LastFM
{
    public class MusicScrobbler
    {
        private readonly Logger _logger;

        public bool IsAuth { get; private set; }

        private Scrobbler _scrobbler;

        public MusicScrobbler(Logger logger)
        {
            this._logger = logger;
        }
     
        public void Auth(string apiKey, string apiSecret, string session)
        {
            _scrobbler = new Scrobbler(apiKey, apiSecret, session);
            IsAuth = true;
        }

        public string GetUrlAuth()
        {
            if (!this.IsAuth) Auth();

            return _scrobbler.GetAuthorisationUri();
        }

        public string GetSession()
        {
            return _scrobbler?.GetSession();
        }

        public void Auth(string session)
        {
            Auth("2a649e9fc2e168da07c69fa32a51ed5b", "396367228492079ec1bf9194ebe8e480", session);
        }

        public void Auth()
        {
            Auth(null);
        }

        public void SetNowPlaying(Track track)
        {
            _scrobbler.NowPlaying(track);
        }

        public async Task SetNowPlayingAsync(Track track)
        {
            _ = Task.Run(() => { SetNowPlaying(track); }).ConfigureAwait(false);
        }

        public void Scrobble(Track track)
        {
            _scrobbler.Scrobble(track);
        }

        public async Task ScrobbleAsync(Track track)
        {
            await Task.Run(() => Scrobble(track));
        }
    }
}
