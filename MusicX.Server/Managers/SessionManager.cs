using MusicX.Server.Models;
using MusicX.Shared.ListenTogether;
using MusicX.Shared.Player;

namespace MusicX.Server.Managers
{
    public class SessionManager
    {
        private readonly Dictionary<string, ListenTogetherSession> Sessions;

        public SessionManager()
        {
            Sessions = new Dictionary<string, ListenTogetherSession>();
        }

        public ListenTogetherSession AddSession(User owner)
        {
            var session = new ListenTogetherSession();

            session.Owner = owner;
            session.Listeners = new List<User>();

            Sessions.Add(session.Owner.ConnectionId, session);

            return session;
        }

        public void RemoveSession(string owner)
        {
            Sessions.Remove(owner);
        }

        public void AddListenerToSesstion(string ownerConnectionId, User listener)
        {
            if(Sessions.TryGetValue(ownerConnectionId, out var session))
            {
                session.Listeners.Add(listener);
            }
        }

        public void RemoveListener(string ownerConnectionId, User listener)
        {
            if (Sessions.TryGetValue(ownerConnectionId, out var session))
            {
                session.Listeners.Remove(listener);
            }
        }

        public ListenTogetherSession? GetSessionByOwner(string ownerConnectionId)
        {
            if (Sessions.TryGetValue(ownerConnectionId, out var session))
            {
                return session;
            }

            return null;
        }

        public ListenTogetherSession? GetSessionByListener(long listenerVkId)
        {
            //todo: блять надеюсь не ебанет
            try
            {
                var session = Sessions.First(s => s.Value.Listeners.Any(l => l.VkId == listenerVkId));

                return session.Value;
            }catch(Exception)
            {
                return null;
            }
        }

        public bool ChangeTrackInSession(PlaylistTrack track, string ownerConnectionId)
        {
            if(Sessions.TryGetValue(ownerConnectionId, out var session))
            {
                session.CurrentTrack = track;

                return true;
            }

            return false;
        }
    }
}
