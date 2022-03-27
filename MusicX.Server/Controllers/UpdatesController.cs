using Microsoft.AspNetCore.Mvc;
using MusicX.Shared;
using System.IO;
using System.Text.Json;

namespace MusicX.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UpdatesController : Controller
    {
        [HttpGet()]
        public async Task<CheckModel> Check(string version)
        {
            var json = await System.IO.File.ReadAllTextAsync("versions.production.json");

            var model = JsonSerializer.Deserialize<List<Shared.Version>>(json);

            var result = new CheckModel();

            result.LastVersion = model.LastOrDefault();
            result.IsUpdated = result.LastVersion.Ver == version;
            result.Versions = model;

            return result;
        }
    }
}
