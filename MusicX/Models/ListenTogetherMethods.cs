using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicX.Models
{
    public class ListenTogetherMethods
    {
        public const string ChangeTrack = "ChangeTrack";

        public const string ChangePlayState = "ChangePlayState";

        public const string StartPlaySession = "StartPlaySession";

        public const string StopPlaySession = "StopPlaySession";

        public const string JoinPlaySession = "JoinPlaySession";

        public const string LeavePlaySession = "LeavePlaySession";

        public const string GetCurrentTrack = "GetCurrentTrack";

        public const string GetOwnerSessionInfoAsync = "GetOwnerSessionInfoAsync";

        public const string GetListenersInSession = "GetListenersInSession";
    }
}
