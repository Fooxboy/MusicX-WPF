using Microsoft.AspNetCore.Mvc;
using MusicX.Server.Services;
using MusicX.Shared.ListenTogether;

namespace MusicX.Server.Controllers;

public class ConnectController : Controller
{
    private readonly SessionService _sessionService;

    public ConnectController(SessionService sessionService)
    {
        _sessionService = sessionService;
    }

    // GET
    public IActionResult Index([FromQuery] string id)
    {
        // if (_sessionService.GetSessionByOwner(id) is null)
        //     return NotFound();
        
        return View(new SessionId(id));
    }
}