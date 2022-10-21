using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicX.Shared.ListenTogether
{
    public class Track
    {
        public string Title { get; set; }

        public string Subtitle { get; set; }

        public string? Cover { get; set; }

        public string Url { get; set; }

        public string Artist { get; set; }

        public TimeSpan Duration { get; set; }

        public bool IsExplicit { get; set; }
        
        public TrackType PlatformType { get; set; }
    }
}
