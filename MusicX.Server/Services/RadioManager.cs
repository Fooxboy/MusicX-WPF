using MusicX.Shared.ListenTogether.Radio;
using System.Linq.Expressions;

namespace MusicX.Server.Services
{
    public class RadioManager
    {
        private readonly Dictionary<string, Station> _stations;

        public Station AddStation(string sessionId, string title, string cover, long ownerId, string ownerName, string ownerPhoto)
        {
            if (_stations.ContainsKey(sessionId))
            {
                throw new Exception("Радиостанция с таким ID сессии уже существует");
            }

            var station = new Station()
            {
                Title = title,
                Cover = cover,
                SessionId = sessionId,
                Owner = new StationOwner()
                {
                    Name = ownerName,
                    Photo = ownerPhoto,
                    VkId = ownerId
                }
            };

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
    }
}
