using Microsoft.AspNetCore.SignalR;
using MusicX.Server.Hubs;
using MusicX.Server.Managers;
using MusicX.Server.Models;
using MusicX.Shared.ListenTogether;
using MusicX.Shared.Player;

namespace MusicX.Server.Services
{
    public class ListenTogetherService
    {
        private readonly IHubContext<ListenTogetherHub> _hub;
        private readonly ILogger<ListenTogetherService> _logger;
        private readonly SessionManager _sessionManager;

        public ListenTogetherService(IHubContext<ListenTogetherHub> hub, SessionManager manager, ILogger<ListenTogetherService> logger)
        {
            _hub = hub;
            _sessionManager = manager;
            _logger = logger;
        }

        public async Task<string?> StartSessionAsync(string ownerConnectionId, long ownerVkId)
        {
            _logger.LogInformation($"Пользователь {owner} создал сессию совместного прослушивания");

            var session = _sessionManager.GetSessionByOwner(ownerConnectionId);

            if (session is not null) return null;

            var owner = new User(ownerConnectionId, ownerVkId);

            session = _sessionManager.AddSession(owner);

            //todo: и другая магия, которой пока нет.

            _logger.LogInformation($"Пользователь {ownerConnectionId} создал сессию совместного прослушивания");

            return session.Owner.ConnectionId;
        }

        public async Task<bool> JoinToSessionAsync(string listenerConnectionId, long listenerVkId,  string ownerConnectionId)
        {
            _logger.LogInformation($"Пользователь {connectionId} подключился к сессии совместного прослушивания {sessionId}");

            //todo: на будущее можем сделать у сессий разные id, сейчас их id равен owner. Поэтому в названиях противоречие

            var currentSession = _sessionManager.GetSessionByListener(listenerVkId);

            if (currentSession != null) return false;

            var session = _sessionManager.GetSessionByOwner(ownerConnectionId);

            if(session is null) return false;

            await _hub.Groups.AddToGroupAsync(listenerConnectionId, session.Owner.ConnectionId);

            var listener = new User(listenerConnectionId, listenerVkId);

            _sessionManager.AddListenerToSesstion(session.Owner.ConnectionId, listener);

            await _hub.Clients.User(session.Owner.ConnectionId).SendAsync(Callbacks.ListenerConnected, listener);

            _logger.LogInformation($"Пользователь {listenerConnectionId} подключился к сессии совместного прослушивания {ownerConnectionId}");

            return true;
        }

        public async Task<bool> LeaveSessionAsync(string connectionId, long vkId)
        {
            _logger.LogInformation($"Пользователь {connectionId} покинул сессию");

            var session = _sessionManager.GetSessionByListener(vkId);

            if (session is null) return true;

            await _hub.Groups.RemoveFromGroupAsync(connectionId, session.Owner.ConnectionId);

            var listener = session.Listeners.Where(l => l.VkId == vkId).FirstOrDefault();

            if (listener is null) return true;

            _sessionManager.RemoveListener(session.Owner.ConnectionId, listener);

            await _hub.Clients.User(session.Owner.ConnectionId).SendAsync(Callbacks.ListenerDisconnected, listener);

            _logger.LogInformation($"Пользователь {connectionId} покинул сессию");

            return true;
        }

        public async Task<bool> StopSessionAsync(string ownerConnectionId)
        {
            var session = _sessionManager.GetSessionByOwner(ownerConnectionId);

            var session = _sessionManager.GetSessionByOwner(owner);

            if (session is null) return true;

            await _hub.Clients.Group(ownerConnectionId).SendAsync(Callbacks.SessionStoped);

            foreach (var listener in session.Listeners)
            {
                await _hub.Groups.RemoveFromGroupAsync(listener.ConnectionId, session.Owner.ConnectionId);
            }

            _sessionManager.RemoveSession(ownerConnectionId);

            _logger.LogInformation($"Пользователь {ownerConnectionId} остановил сессию");


            return true;
        }

        public async Task<bool> ChangeTrackAsync(PlaylistTrack track, string ownerConnectionId)
        {
            var session = _sessionManager.GetSessionByOwner(ownerConnectionId);

            if (session is null) return false;

            await _hub.Clients.Group(ownerConnectionId).SendAsync(Callbacks.TrackChanged, track);

            _logger.LogInformation($"Пользователь {ownerConnectionId} сменил трек.");

            return _sessionManager.ChangeTrackInSession(track, ownerConnectionId);
        }

        public async Task<bool> ChangePlayStateAsync(string ownerConnectionId, TimeSpan position, bool pause)
        {
            var session = _sessionManager.GetSessionByOwner(ownerConnectionId);

            if (session is null) return false;

            await _hub.Clients.Group(ownerConnectionId).SendAsync(Callbacks.PlayStateChanged, new PlayState(position, pause));

            return true;
        }

        public async Task<PlaylistTrack> GetCurrentSessionTrack(long vkId)
        {
            var session = _sessionManager.GetSessionByListener(vkId);

            if (session is null) return null;

            return session.CurrentTrack;
        }

        public async Task<User> GetOwnerSessionInfoAsync(long listenerVkId)
        {
            var session = _sessionManager.GetSessionByListener(listenerVkId);

            if (session is null) return null;

            return session.Owner;
        }

        public async Task<UsersList> GetListenersInSessionAsync(string ownerConnectionId)
        {
            var session = _sessionManager.GetSessionByOwner(ownerConnectionId);

            if (session is null) return null;

            return new(session.Listeners);
        }
    }
}
