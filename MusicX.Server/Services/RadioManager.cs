using Microsoft.Win32;
using MusicX.Shared.ListenTogether.Radio;
using System.Linq.Expressions;

namespace MusicX.Server.Services
{
    public class RadioManager
    {
        private readonly IConfiguration _configuration;

        public RadioManager(IConfiguration configuration)
        {
            _configuration = configuration;
            _stations = new Dictionary<string, Station>();

        }
        private readonly Dictionary<string, Station> _stations;

        public Station AddStation(string sessionId, string title, string cover, string description, long ownerId, string ownerName, string ownerPhoto)
        {
            if (_stations.ContainsKey(sessionId))
            {
                throw new Exception("Радиостанция с таким ID сессии уже существует");
            }

            var userCategory = _configuration["UsersCategories:UsersCategories:Developers"];

            var station = new Station()
            {
                Title = title,
                Cover = cover,
                SessionId = sessionId,
                Owner = new StationOwner()
                {
                    Name = ownerName,
                    Photo = ownerPhoto,
                    VkId = ownerId,
                    OwnerCategory = GetOwnerCategory(ownerId)
                }
            };

            if(station.Owner.OwnerCategory == OwnerCategory.Banned)
            {
                throw new Exception("Забаненный пользователь попытался создать сессию");
            }

            _stations.Add(sessionId, station);

            return station;
        }

        public void RemoveStation(string sessionId)
        {
            if(_stations.ContainsKey(sessionId))
            {
                _stations.Remove(sessionId);
            }
        }

        public List<Station> GetStations(Func<Station, bool> filter = null)
        {
            if(filter == null)
            {
                return _stations.Select(x=> x.Value).ToList();
            }

            return _stations.Select(x=> x.Value).Where(filter).ToList();
        }

        private OwnerCategory GetOwnerCategory(long vkId)
        {
            var developers = _configuration.GetSection("UsersCategories:Developers").Get<long[]>().ToList();
            var recoms = _configuration.GetSection("UsersCategories:Recoms").Get<long[]>().ToList();
            var banned = _configuration.GetSection("UsersCategories:Banned").Get<long[]>().ToList();

            if (developers.Contains(vkId)) return OwnerCategory.Developer;

            if (recoms.Contains(vkId)) return OwnerCategory.Recoms;

            if (banned.Contains(vkId)) return OwnerCategory.Banned;

            return OwnerCategory.User;
        }
    }
}
