using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using AsyncAwaitBestPractices.MVVM;
using MusicX.Helpers;
using MusicX.Services;
using MusicX.Shared.ListenTogether;
using MusicX.Shared.Player;
using NLog;
using VkNet.Abstractions;
using VkNet.Enums.Filters;
using Wpf.Ui.Common;

namespace MusicX.ViewModels.Controls;

public class ListenTogetherControlViewModel : BaseViewModel
{
    private readonly ListenTogetherService _service;
    private readonly IUsersCategory _vkUsers;
    private readonly NotificationsService _notificationsService;
    private readonly Logger _logger;
    private readonly ConfigService _configService;
    public bool IsSessionHost { get; set; }
    public bool IsConnected { get; set; }
    public ObservableRangeCollection<ListenTogetherSession> Sessions { get; } = new();
    
    public ICommand StopCommand { get; }
    public IAsyncCommand<string> ConnectCommand { get; }
    public ICommand StartSessionCommand { get; }

    public ListenTogetherControlViewModel(ListenTogetherService service, IUsersCategory vkUsers, NotificationsService notificationsService,
                                          Logger logger, ConfigService configService)
    {
        _service = service;
        _vkUsers = vkUsers;
        _notificationsService = notificationsService;
        _logger = logger;
        _configService = configService;
        StopCommand = new AsyncCommand(StopAsync);
        ConnectCommand = new AsyncCommand<string>(ConnectAsync);
        StartSessionCommand = new AsyncCommand(StartedSessionAsync);

        service.LeaveSession += OnDisconnected;
        service.SessionStoped += OnDisconnected;
        service.SessionOwnerStoped += OnDisconnected;
        service.StartedSession += OnSessionStarted;
        service.ConnectedToSession += OnSessionConnected;
        service.ListenerConnected += OnListenerConnected;
        service.ListenerDisconnected += OnListenerDisconnected;
    }

    private async Task StartedSessionAsync()
    {
        try
        {
            var sessionId = await _service.StartSessionAsync(_configService.Config.UserId);

            Clipboard.SetText(sessionId);
            
            _notificationsService.Show("Успешно", "Сессия успешно создана. Id скопирован в буффер обмена");
        }
        catch (Exception e)
        {
            _notificationsService.Show("Ошибка", "Ошибка создания сессии");
            _logger.Error(e);
            throw;
        }
    }

    private async Task ConnectAsync(string? sessionId)
    {
        if (string.IsNullOrEmpty(sessionId))
        {
            _notificationsService.Show("Ошибка", "Введите id сессии");
            return;
        }

        try
        {
            await _service.ConnectToServerAsync(_configService.Config.UserId);
            await _service.JoinToSesstionAsync(sessionId);
        }
        catch (Exception e)
        {
            _notificationsService.Show("Ошибка", e.Message);
            _logger.Error(e);
            throw;
        }

        _notificationsService.Show("Подключено", "Успешное подключение к сессии");
    }

    private Task OnListenerDisconnected(User user)
    {
        if (Sessions.FirstOrDefault(b => b.VkId == user.VkId) is { } session)
            Sessions.Remove(session);
        
        return Task.CompletedTask;
    }

    private async Task OnListenerConnected(User user)
    {
        var users = await _vkUsers.GetAsync(new[] { user.VkId },
                                            ProfileFields.FirstName | ProfileFields.LastName | ProfileFields.Photo100);
        
        if (users.Any())
            Sessions.Add(new(user.VkId, $"{users[0].FirstName} {users[0].LastName}",
                             users[0].PhotoPreviews.Photo100.ToString()));
    }

    private Task OnSessionConnected(PlaylistTrack arg)
    {
        IsConnected = true;
        IsSessionHost = false;
        return Task.CompletedTask;
    }

    private Task OnSessionStarted()
    {
        IsConnected = true;
        IsSessionHost = true;
        return Task.CompletedTask;
    }

    private Task OnDisconnected()
    {
        IsConnected = false;
        IsSessionHost = false;
        Sessions.Clear();
        return Task.CompletedTask;
    }

    private Task StopAsync()
    {
        return IsSessionHost ? _service.StopPlaySessionAsync() : _service.LeavePlaySessionAsync();
    }
}

public record ListenTogetherSession(long VkId, string Name, string AvatarUrl);