using MusicX.Shared.ListenTogether;

namespace MusicX.Server.Models
{
    public class ListenTogetherSession
    {
        public string Owner { get; set; }

        public List<string> Listeners { get; set; }

        public Track CurrentTrack { get; set; }
    }
}
