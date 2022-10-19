using Microsoft.AspNetCore.SignalR;
using MusicX.Server.Hubs;
using MusicX.Server.Managers;
using MusicX.Server.Models;
using MusicX.Shared.ListenTogether;

namespace MusicX.Server.Services
{
    public class ListenTogetherService
    {
        private readonly IHubContext<ListenTogetherHub> _hub;
        private readonly ILogger<ListenTogetherService> _logger;
        private readonly SessionManager _sessionManager;

        public ListenTogetherService(IHubContext<ListenTogetherHub> hub, SessionManager manager)
        {
            _hub = hub;
            _sessionManager = manager;
        }

        public async Task<string?> StartSessionAsync(string owner)
        {
            _logger.LogInformation($"User {owner}");

            var session = _sessionManager.GetSessionByOwner(owner);

            if (session is not null) return null;

            session = _sessionManager.AddSession(owner);

            //todo: и другая магия, которой пока нет.

            return session.Owner;
        }

        public async Task<bool> JoinToSessionAsync(string connectionId, string sessionId)
        {
            //todo: на будущее можем сделать у сессий разные id, сейчас их id равен owner. Поэтому в названиях противоречие

            var currentSession = _sessionManager.GetSessionByListener(connectionId);

            if (currentSession != null) return false;

            var session = _sessionManager.GetSessionByOwner(sessionId);

            if(session is null) return false;

            await _hub.Groups.AddToGroupAsync(connectionId, session.Owner);

            _sessionManager.AddListenerToSesstion(session.Owner, connectionId);

            return true;
        }

        public async Task<bool> LeaveSessionAsync(string connectionId)
        {
            var session = _sessionManager.GetSessionByListener(connectionId);

            if (session is null) return true;

            await _hub.Groups.RemoveFromGroupAsync(connectionId, session.Owner);

            _sessionManager.RemoveListener(session.Owner, connectionId);

            return false;
        }

        public async Task<bool> StopSessionAsync(string owner)
        {
            var session = _sessionManager.GetSessionByOwner(owner);

            if (session is null) return true;

            foreach (var listener in session.Listeners)
            {
                await _hub.Groups.RemoveFromGroupAsync(listener, session.Owner);
            }

            _sessionManager.RemoveSession(owner);

            return true;
        }

        public async Task<bool> ChangeTrackAsync(Track track, string owner)
        {
            var session = _sessionManager.GetSessionByOwner(owner);

            if (session is null) return false;

            await _hub.Clients.Group(owner).SendAsync(Callbacks.TrackChanged, track);

            return _sessionManager.ChangeTrackInSession(track, owner);
        }

        public async Task<bool> ChangePlayStateAsync(string owner, TimeSpan position, bool pause)
        {
            var session = _sessionManager.GetSessionByOwner(owner);

            if (session is null) return false;

            await _hub.Clients.Group(owner).SendAsync(Callbacks.PlayStateChanged, position, pause);

            return true;
        }
    }
}
