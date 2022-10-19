using MusicX.Server.Models;
using MusicX.Shared.ListenTogether;

namespace MusicX.Server.Managers
{
    public class SessionManager
    {
        private readonly Dictionary<string, ListenTogetherSession> Sessions;

        public SessionManager()
        {
            Sessions = new Dictionary<string, ListenTogetherSession>();
        }

        public ListenTogetherSession AddSession(string owner)
        {
            var session = new ListenTogetherSession();

            session.Owner = owner;
            session.Listeners = new List<string>();

            Sessions.Add(session.Owner, session);

            return session;
        }

        public void RemoveSession(string owner)
        {
            Sessions.Remove(owner);
        }

        public void AddListenerToSesstion(string owner, string listener)
        {
            if(Sessions.TryGetValue(owner, out var session))
            {
                session.Listeners.Add(listener);
            }
        }

        public void RemoveListener(string owner, string listener)
        {
            if (Sessions.TryGetValue(owner, out var session))
            {
                session.Listeners.Remove(listener);
            }
        }

        public ListenTogetherSession? GetSessionByOwner(string owner)
        {
            if (Sessions.TryGetValue(owner, out var session))
            {
                return session;
            }

            return null;
        }

        public ListenTogetherSession? GetSessionByListener(string listener)
        {
            //todo: блять надеюсь не ебанет
            try
            {
                var session = Sessions.First(s => s.Value.Listeners.Any(l => l == listener));

                return session.Value;
            }catch(Exception)
            {
                return null;
            }
        }

        public bool ChangeTrackInSession(Track track, string owner)
        {
            if(Sessions.TryGetValue(owner, out var session))
            {
                session.CurrentTrack = track;

                return true;
            }

            return false;
        }
    }
}
