using MusicX.Shared.ListenTogether;
using MusicX.Shared.Player;

namespace MusicX.Server.Models
{
    public class ListenTogetherSession
    {
        public string Owner { get; set; }

        public List<string> Listeners { get; set; }

        public PlaylistTrack CurrentTrack { get; set; }
    }
}
