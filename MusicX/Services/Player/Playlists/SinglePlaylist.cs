using MusicX.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicX.Services.Player.Playlists
{
    public class SinglePlaylist : PlaylistBase<Audio>
    {
        public override bool CanLoad => true;

        public override Audio Data { get; }

        public SinglePlaylist(Audio data)
        {
            Data = data;
        }

        public override IAsyncEnumerable<PlaylistTrack> LoadAsync()
        {
            return new List<PlaylistTrack>() { Data.ToTrack()}.ToAsyncEnumerable();
        }
    }
}
