using Lpfm.LastFmScrobbler;
using MusicX.Core.LastFM;
using MusicX.Core.Models;
using NLog;


namespace MusicX.Core.Services
{
    public class LastFMService
    {
        private readonly Logger _logger;
        private readonly MusicScrobbler _scrobbler;

        public LastFMService(Logger logger)
        {
            this._logger = logger;
            this._scrobbler = new MusicScrobbler(logger);
        }

        public async Task ScrobbleAsync(Audio audio)
        {
            var track = ToLastFMTrack(audio);
            await _scrobbler.ScrobbleAsync(track);
        }

        public string GetUrlAuth()
        {
            return this._scrobbler.GetUrlAuth();
        }

        public void Auth(string session)
        {
            this._scrobbler.Auth(session);
        }

        private Track ToLastFMTrack(Audio audio)
        {
            var model = new Track();
            model.AlbumArtist = audio.Artist;
            model.AlbumName = audio.Album is null ? audio.Title : audio.Album.Title;
            model.ArtistName = audio.Artist;
            model.Duration = TimeSpan.FromSeconds(audio.Duration);
            model.TrackName = audio.Title;
            model.WhenStartedPlaying = DateTime.Now - TimeSpan.FromSeconds(audio.Duration);

            return model;
        }


    }
}
