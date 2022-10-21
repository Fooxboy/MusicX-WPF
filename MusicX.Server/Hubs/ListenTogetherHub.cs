using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using MusicX.Server.Services;
using MusicX.Shared.ListenTogether;

namespace MusicX.Server.Hubs;

public class ListenTogetherHub : Hub
{
    private readonly ILogger<ListenTogetherHub> _logger;
    private readonly ListenTogetherService _listenTogetherService;

    public ListenTogetherHub(ILogger<ListenTogetherHub> logger, ListenTogetherService listenTogetherService)
    {
        _logger = logger;
        _listenTogetherService = listenTogetherService;
    }

    /// <summary>
    /// Смена трека
    /// </summary>
    /// <param name="track">Трек</param>
    /// <returns>Успешность результата</returns>
    public async Task<bool> ChangeTrack(Track track)
    {
        try
        {
            var owner = Context.ConnectionId;
            return await _listenTogetherService.ChangeTrackAsync(track, Context.ConnectionId);
        }catch(Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw new HubException(ex.Message, ex);
        }
    }

    /// <summary>
    /// Изменилось состояние трека
    /// </summary>
    /// <param name="position">Позиция</param>
    /// <param name="pause">Флаг паузы</param>
    /// <returns>Успешность операции</returns>
    public async Task<bool> ChangePlayState(TimeSpan position, bool pause)
    {
        try
        {
            var owner = Context.ConnectionId;
            return await _listenTogetherService.ChangePlayStateAsync(owner, position, pause);
        }catch(Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw new HubException(ex.Message, ex);
        }
    }
    
    /// <summary>
    /// Создать сессию "Слушать вместе"
    /// </summary>
    /// <returns>Успешность операции</returns>
    public async Task<string?> StartPlaySession()
    {
        try
        {
            var owner = Context.ConnectionId;
            return await _listenTogetherService.StartSessionAsync(owner);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw new HubException(ex.Message, ex);
        }
    }

    /// <summary>
    /// Остановить сессию "Слушать вместе"
    /// </summary>
    /// <returns>Успешность операции</returns>
    public async Task<bool> StopPlaySession()
    {
        try
        {
            var owner = Context.ConnectionId;
            return await _listenTogetherService.StopSessionAsync(owner);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw new HubException(ex.Message, ex);
        }
    }
    
    /// <summary>
    /// Присоединиться к сессии
    /// </summary>
    /// <param name="sessionId">ИД сессии</param>
    /// <returns>Успешность операции</returns>
    public async Task<bool> JoinPlaySession(string sessionId)
    {
        try
        {
            var owner = Context.ConnectionId;
            return await _listenTogetherService.JoinToSessionAsync(owner, sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw new HubException(ex.Message, ex);
        }
    }

    /// <summary>
    /// Покинуть сессию
    /// </summary>
    public async Task<bool> LeavePlaySession(string sessionId)
    {
        try
        {
            return await _listenTogetherService.LeaveSessionAsync(sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw new HubException(ex.Message, ex);
        }
    }

    public async Task<Track> GetCurrentTrack()
    {
        try
        {
            var listener = Context.ConnectionId;
            return await _listenTogetherService.GetCurrentSessionTrack(listener);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw new HubException(ex.Message, ex);
        }
    }
}