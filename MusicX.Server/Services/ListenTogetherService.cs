using Microsoft.AspNetCore.SignalR;
using MusicX.Server.Hubs;
using MusicX.Server.Models;
using MusicX.Shared.ListenTogether;
using MusicX.Shared.Player;

namespace MusicX.Server.Services
{
    public class ListenTogetherService
    {
        private readonly IHubContext<ListenTogetherHub> _hub;
        private readonly ILogger<ListenTogetherService> _logger;
        private readonly SessionService _sessionService;

        public ListenTogetherService(IHubContext<ListenTogetherHub> hub, SessionService service, ILogger<ListenTogetherService> logger)
        {
            _hub = hub;
            _sessionService = service;
            _logger = logger;
        }

        public async Task<string?> StartSessionAsync(string ownerConnectionId, long ownerVkId)
        {

            var session = _sessionService.GetSessionByOwner(ownerConnectionId);

            if (session is not null) throw new Exception("Уже существует сессия.");

            var owner = new User(ownerConnectionId, ownerVkId);

            session = _sessionService.AddSession(owner);

            //todo: и другая магия, которой пока нет.

            _logger.LogInformation($"Пользователь {ownerConnectionId} создал сессию совместного прослушивания");

            return session.Owner.ConnectionId;
        }

        public async Task<bool> JoinToSessionAsync(string listenerConnectionId, long listenerVkId,  string ownerConnectionId)
        {
            var session = _sessionService.GetSessionByOwner(ownerConnectionId);

            if(session is null) throw new Exception("Не найдена сессия.");

            await _hub.Groups.AddToGroupAsync(listenerConnectionId, session.Owner.ConnectionId);

            var listener = new User(listenerConnectionId, listenerVkId);

            _sessionService.AddListenerToSesstion(session.Owner.ConnectionId, listener);

            await _hub.Clients.Client(session.Owner.ConnectionId).SendAsync(Callbacks.ListenerConnected, listener);

            _logger.LogInformation($"Пользователь {listenerConnectionId} подключился к сессии совместного прослушивания {ownerConnectionId}");

            return true;
        }

        public async Task<bool> LeaveSessionAsync(string connectionId, long vkId)
        {

            var session = _sessionService.GetSessionByListener(vkId);

            if (session is null) return true;

            await _hub.Groups.RemoveFromGroupAsync(connectionId, session.Owner.ConnectionId);

            var listener = session.Listeners.Where(l => l.VkId == vkId).FirstOrDefault();

            if (listener is null) return true;

            _sessionService.RemoveListener(session.Owner.ConnectionId, listener);

            await _hub.Clients.Client(session.Owner.ConnectionId).SendAsync(Callbacks.ListenerDisconnected, listener);

            _logger.LogInformation($"Пользователь {connectionId} покинул сессию");

            return true;
        }

        public async Task<bool> LeaveSessionAsync(string connectionId)
        {

            var session = _sessionService.GetSessionByListener(connectionId);

            if (session is null) return true;

            await _hub.Groups.RemoveFromGroupAsync(connectionId, session.Owner.ConnectionId);

            var listener = session.Listeners.Where(l => l.ConnectionId == connectionId).FirstOrDefault();

            if (listener is null) return true;

            _sessionService.RemoveListener(session.Owner.ConnectionId, listener);

            await _hub.Clients.Client(session.Owner.ConnectionId).SendAsync(Callbacks.ListenerDisconnected, listener);

            _logger.LogInformation($"Пользователь {connectionId} покинул сессию");

            return true;
        }

        public async Task<bool> StopSessionAsync(string ownerConnectionId)
        {
            var session = _sessionService.GetSessionByOwner(ownerConnectionId);

            if (session is null) return true;

            await _hub.Clients.Group(ownerConnectionId).SendAsync(Callbacks.SessionStoped);

            foreach (var listener in session.Listeners)
            {
                await _hub.Groups.RemoveFromGroupAsync(listener.ConnectionId, session.Owner.ConnectionId);
            }

            _sessionService.RemoveSession(ownerConnectionId);

            _logger.LogInformation($"Пользователь {ownerConnectionId} остановил сессию");


            return true;
        }

        public async Task<bool> ChangeTrackAsync(PlaylistTrack track, string ownerConnectionId)
        {
            var session = _sessionService.GetSessionByOwner(ownerConnectionId);

            if (session is null) return false;

            await _hub.Clients.Group(ownerConnectionId).SendAsync(Callbacks.TrackChanged, track);

            _logger.LogInformation($"Пользователь {ownerConnectionId} сменил трек.");

            return _sessionService.ChangeTrackInSession(track, ownerConnectionId);
        }

        public async Task<bool> ChangePlayStateAsync(string ownerConnectionId, TimeSpan position, bool pause)
        {
            var session = _sessionService.GetSessionByOwner(ownerConnectionId);

            if (session is null) return false;

            await _hub.Clients.Group(ownerConnectionId).SendAsync(Callbacks.PlayStateChanged, new PlayState(position, pause));

            return true;
        }

        public async Task<PlaylistTrack> GetCurrentSessionTrack(long vkId)
        {
            var session = _sessionService.GetSessionByListener(vkId);

            if (session is null) throw new Exception("сессия не найдена");

            return session.CurrentTrack;
        }

        public async Task<User> GetOwnerSessionInfoAsync(long listenerVkId)
        {
            var session = _sessionService.GetSessionByListener(listenerVkId);

            if (session is null) throw new Exception("Сессия не найдена.") ;

            var owner = session.Owner;

            return owner;
        }

        public async Task<UsersList> GetListenersInSessionAsync(string ownerConnectionId)
        {
            var session = _sessionService.GetSessionByOwner(ownerConnectionId);

            if (session is null) return null;

            return new(session.Listeners);
        }

        public async Task ClientDisconnected(string connectionId)
        {
            var session = _sessionService.GetSessionByOwner(connectionId);

            if(session is not null)
            {
                await this.StopSessionAsync(connectionId);

                return;
            }

            var s = _sessionService.GetSessionByListener(connectionId);

            if(s is not null) await this.LeaveSessionAsync(connectionId);
        }
    }
}
