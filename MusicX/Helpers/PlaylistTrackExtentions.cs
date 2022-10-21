using MusicX.Shared.ListenTogether;
using MusicX.Shared.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicX.Helpers
{
    public static class PlaylistTrackExtentions
    {
        public static Track ToRemoteTrack(this PlaylistTrack playlistTrack)
        {
            var track = new Track();

            track.PlatformType = TrackType.Vk;
            track.Title = playlistTrack.Title;
            track.Subtitle = playlistTrack.Subtitle;
            var artists = string.Empty;

            foreach(var artist in playlistTrack.MainArtists)
            {
                artists += $"{artist};";
            }

            track.Artist = artists;

            track.Duration = playlistTrack.Data.Duration;
            track.IsExplicit = playlistTrack.Data.IsExplicit;
            track.Cover = playlistTrack.AlbumId?.CoverUrl;
            track.Url = playlistTrack.Data.Url;

            return track;
        }

        public static PlaylistTrack ToPlaylistTrack(this Track track)
        {
            var album = new VkAlbumId(new Random().Next(0, 9000), new Random().Next(0, 9000), "remote", "remote", track.Cover);

            var artistsString = track.Artist.Split(";").ToList();

            var artists = new List<TrackArtist>();

            foreach(var artistString in artistsString)
            {
                var artist = new TrackArtist(artistString, null);
                artists.Add(artist);
            }

            var trackData = new VkTrackData(track.Url, false, track.IsExplicit, track.Duration, new IdInfo(new Random().Next(0,100), new Random().Next(0, 100), "remote"), "remote", null, null);

            var playlistTrack = new PlaylistTrack(track.Title, track.Subtitle, album, artists, new List<TrackArtist>(), trackData);

            return playlistTrack;
        }
    }
}
