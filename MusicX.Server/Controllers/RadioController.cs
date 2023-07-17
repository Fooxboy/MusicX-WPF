using Microsoft.AspNetCore.Mvc;
using MusicX.Server.Services;
using MusicX.Shared.ListenTogether.Radio;

namespace MusicX.Server.Controllers
{
    [Route("radio")]
    public class RadioController : Controller
    {
        private readonly RadioService _radioService;

        public RadioController(RadioService radioService)
        {
            _radioService = radioService;
        }

        [HttpGet("getStations")]
        public List<Station> GetStations()
        {
            var stations = _radioService.GetStations();

            return stations;
        }

        [HttpGet("createStation")]
        public Station CreateStation(string sessionId, string title, string cover, string description, long ownerId, string ownerName, string ownerPhoto)
        {
            var station = _radioService.CreateStation(sessionId, title, cover, description, ownerId, ownerName, ownerPhoto);



            return station;
        }

        [HttpGet("deleteStation")]
        public bool DeleteStation(string connectId)
        {
            return _radioService.DeleteStation(connectId);
        }
    }
}
