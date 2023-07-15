using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicX.Shared.ListenTogether.Radio
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic, SkipConstructor = true)]
    public class Station
    {
        public string SessionId { get; set; }

        public string Title { get; set; }

        public string Cover { get; set; }

        public StationOwner Owner { get; set; }
    }
}
