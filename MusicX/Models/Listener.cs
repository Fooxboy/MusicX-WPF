using MusicX.Shared.ListenTogether;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicX.Models
{
    public class Listener
    {
        public string Name { get; set; }
        public string Photo { get; set; }

        public User Ids { get; set; }
    }
}
