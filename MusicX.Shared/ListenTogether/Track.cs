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

        public string Cover { get; set; }

        public string Url { get; set; }

        public string Artist { get; set; }

        public long Duration { get; set; }
    }
}
