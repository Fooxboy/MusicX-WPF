using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AsyncAwaitBestPractices.MVVM;
using Microsoft.AppCenter.Analytics;
using Microsoft.Extensions.DependencyInjection;
using MusicX.Core.Services;
using MusicX.Helpers;
using MusicX.Services;
using MusicX.Services.Player;
using MusicX.Shared.ListenTogether;
using MusicX.Shared.Player;
using MusicX.ViewModels.Modals;
using MusicX.Views.Modals;
using NLog;
using VkNet.Abstractions;
using VkNet.Enums.Filters;
using Wpf.Ui.Contracts;

namespace MusicX.ViewModels.Controls;

public class ListenTogetherControlViewModel : BaseViewModel
{
    private readonly ListenTogetherService _service;
    private readonly IUsersCategory _vkUsers;
    private readonly ISnackbarService _snackbarService;
    private readonly Logger _logger;
    private readonly ConfigService _configService;
    private readonly PlayerService _playerService;
    private readonly NavigationService _navigationService;
    public bool IsSessionHost { get; set; }
    public bool IsConnected { get; set; }
    public ObservableRangeCollection<ListenTogetherSession> Sessions { get; } = new();
    
    public ICommand StopCommand { get; }
    public IAsyncCommand<string> ConnectCommand { get; }
    public ICommand StartSessionCommand { get; }
    public ICommand OpenModalLink { get; set; }

    public ICommand CreateNewUserStationCommand { get; set; }


    public bool IsLoading { get; set; } = false;

    public ListenTogetherControlViewModel(ListenTogetherService service, IUsersCategory vkUsers,
        ISnackbarService snackbarService,
                                          Logger logger, ConfigService configService, PlayerService playerSerivce, NavigationService navigationService)
    {
        _service = service;
        _vkUsers = vkUsers;
        _snackbarService = snackbarService;
        _logger = logger;
        _configService = configService;
        _playerService = playerSerivce;
        _navigationService = navigationService;

        StopCommand = new AsyncCommand(StopAsync);
        ConnectCommand = new AsyncCommand<string>(ConnectAsync);
        StartSessionCommand = new AsyncCommand(StartedSessionAsync);
        OpenModalLink = new AsyncCommand(OpenLinkModalAsync);
        CreateNewUserStationCommand = new AsyncCommand(CreateNewUserStation);

        service.LeaveSession += OnDisconnected;
        service.SessionStoped += OnDisconnected;
        service.SessionOwnerStoped += OnDisconnected;
        service.StartedSession += OnSessionStarted;
        service.ConnectedToSession += OnSessionConnected;
        service.ListenerConnected += OnListenerConnected;
        service.ListenerDisconnected += OnListenerDisconnected;
    }

    private async Task CreateNewUserStation()
    {
        var navigationService = StaticService.Container.GetRequiredService<NavigationService>();
        var userRadioService = StaticService.Container.GetRequiredService<UserRadioService>();
        var notificationsService = StaticService.Container.GetRequiredService<NotificationsService>();

        if(userRadioService.IsStarted)
        {
            notificationsService.Show("Стоп стоп стоп", "У Вас уже запущена радиостанция. Зачем создавать ещё одну?");
            return;
        }

        navigationService.OpenModal<CreateUserRadioModal>(new CreateUserRadioModalViewModel());
    }

    private async Task StartedSessionAsync()
    {
        try
        {
            var properties = new Dictionary<string, string>
            {
                {"Version", StaticService.Version }
            };
            Analytics.TrackEvent("Started session", properties);

            IsLoading = true;
            OnPropertyChanged("IsLoading");
            var sessionId = await _service.StartSessionAsync(_configService.Config.UserId);

            await _service.ChangeTrackAsync(_playerService.CurrentTrack);

            Clipboard.SetText(sessionId);

            _navigationService.OpenModal<ListenTogetherSessionStartedModal>(new ListenTogetherSessionStartedModalViewModel(_service));
            _snackbarService.Show("Успешно", "Сессия успешно создана. Id скопирован в буффер обмена");

            IsLoading = false;
            OnPropertyChanged("IsLoading");

        }
        catch (Exception e)
        {
            IsLoading = false;
            OnPropertyChanged("IsLoading");

            _snackbarService.Show("Ошибка", "Ошибка создания сессии");
            _logger.Error(e, e.Message);
        }
    }

    private async Task ConnectAsync(string? sessionId)
    {
        if (string.IsNullOrEmpty(sessionId))
        {
            _snackbarService.Show("Ошибка", "Введите id сессии");
            return;
        }

        IsLoading = true;
        OnPropertyChanged("IsLoading");
        try
        {

            var properties = new Dictionary<string, string>
            {
                {"Version", StaticService.Version }
            };
            Analytics.TrackEvent("Connect to session", properties);

            await _service.ConnectToServerAsync(_configService.Config.UserId);
            await _service.JoinToSesstionAsync(sessionId);
            _snackbarService.Show("Подключено", "Успешное подключение к сессии");

        }
        catch (Exception e)
        {
            _snackbarService.Show("Ошибка подключении к сессии", e.Message);
            _logger.Error(e, e.Message);
        }

        IsLoading = false;
        OnPropertyChanged("IsLoading");
    }

    private async Task OpenLinkModalAsync() 
    {
        _navigationService.OpenModal<ListenTogetherSessionStartedModal>(new ListenTogetherSessionStartedModalViewModel(_service));
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

    private Task OnSessionStarted(string sessionId)
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