using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using MusicX.Shared.Player;

namespace MusicX.Server.Hubs;

public class ListenTogetherHub : Hub
{
    private readonly ILogger<ListenTogetherHub> _logger;
    private readonly IMemoryCache _cache;

    public ListenTogetherHub(ILogger<ListenTogetherHub> logger, IMemoryCache cache)
    {
        _logger = logger;
        _cache = cache;
    }

    public Task ChangeTrack(PlaylistTrack track)
    {
        _cache.Set(Context.UserIdentifier!, track);
        return Clients.OthersInGroup(Context.UserIdentifier!).SendAsync("TrackChanged", track);
    }

    public Task ChangePlayState(TimeSpan position, bool pause)
    {
        return Clients.OthersInGroup(Context.UserIdentifier!).SendAsync("PlayStateChanged", position, pause);
    }

    public Task StartPlaySession()
    {
        _logger.LogDebug("Started play session with id {SessionId} for {User}", Context.UserIdentifier,
                         Context.User?.Identity?.Name ?? Context.UserIdentifier);
        
        return Groups.AddToGroupAsync(Context.ConnectionId, Context.UserIdentifier!);
    }

    public Task StopPlaySession()
    {
        _logger.LogDebug("Stopped play session with id {SessionId} for {User}", Context.UserIdentifier,
                         Context.User?.Identity?.Name ?? Context.UserIdentifier);
        
        return Groups.RemoveFromGroupAsync(Context.ConnectionId, Context.UserIdentifier!);
    }
    
    public async Task JoinPlaySession(string sessionId)
    {
        _logger.LogDebug("User {User} joined play session with id {SessionId}",
                         Context.User?.Identity?.Name ?? Context.UserIdentifier, sessionId);
        
        await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);

        if (_cache.TryGetValue(sessionId, out PlaylistTrack track))
            await Clients.Client(Context.ConnectionId).SendAsync("TrackChanged", track);
    }

    public Task LeavePlaySession(string sessionId)
    {
        _logger.LogDebug("User {User} left play session with id {SessionId}",
                         Context.User?.Identity?.Name ?? Context.UserIdentifier, sessionId);
        
        return Groups.RemoveFromGroupAsync(Context.ConnectionId, sessionId);
    }
}