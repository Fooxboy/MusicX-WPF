using MusicX.Shared.ListenTogether;
using MusicX.Shared.Player;

namespace MusicX.Server.Models
{
    public class ListenTogetherSession
    {
        public User Owner { get; set; }

        public List<User> Listeners { get; set; }

        public PlaylistTrack CurrentTrack { get; set; }
    }
}
