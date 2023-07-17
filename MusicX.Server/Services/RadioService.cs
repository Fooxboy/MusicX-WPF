using Microsoft.AspNetCore.SignalR;
using MusicX.Server.Hubs;
using MusicX.Shared.ListenTogether.Radio;

namespace MusicX.Server.Services
{
    public class RadioService
    {
        private readonly ILogger<RadioService> _logger;
        private readonly RadioManager _radioManager;
        private readonly ListenTogetherService _listenTogetherService;

        public RadioService(ILogger<RadioService> logger, RadioManager radioManager, ListenTogetherService listenTogetherService)
        {
            _logger = logger;
            _radioManager = radioManager;
            _listenTogetherService = listenTogetherService;
        }


        public List<Station> GetStations()
        {
            return _radioManager.GetStations();
        }

        public Station CreateStation(string sessionId, string title, string cover, string description, long ownerId, string ownerName, string ownerPhoto)
        {
            if(!_listenTogetherService.CheckExsistSession(sessionId))
            {
                throw new Exception("Такой сессии не существует");
            }

            var session = _radioManager.AddStation(sessionId, title, cover, description, ownerId, ownerName, ownerPhoto);

            _logger.LogInformation($"Создана новая радиостанция: '{title}', владелец: '{ownerName}'");

            return session;
        }

        public bool DeleteStation(string sessionId)
        {
            _radioManager.RemoveStation(sessionId);
            _logger.LogInformation($"Удалена сессия с id '{sessionId}'");
            return true;
        }
    }
}
