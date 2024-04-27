using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using AsyncAwaitBestPractices.MVVM;
using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Api.Helpers;
using MusicX.Services;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;
using NavigationService = MusicX.Services.NavigationService;

namespace MusicX.ViewModels.Modals;

public class LastFmAuthModalViewModel : BaseViewModel
{
    private readonly ILastAuth _auth;
    private readonly NavigationService _navigationService;
    private readonly ISnackbarService _snackbarService;
    private readonly ConfigService _configService;
    private string? _token;
    
    public ICommand ConfirmCommand { get; }

    public LastFmAuthModalViewModel(ILastAuth auth, NavigationService navigationService, ISnackbarService snackbarService, ConfigService configService)
    {
        _auth = auth;
        _navigationService = navigationService;
        _snackbarService = snackbarService;
        _configService = configService;

        ConfirmCommand = new AsyncCommand(ConfirmAsync);
    }

    private async Task ConfirmAsync()
    {
        if (_token is null)
            return;

        var response = await _auth.GetSessionTokenAsync(_token);

        if (!response.Success)
        {
            _snackbarService.Show("Ошибка авторизации", $"Сервис вернул: {response.Status}", ControlAppearance.Danger);
            return;
        }
        
        _navigationService.CloseModal();

        _configService.Config.LastFmSession = _auth.UserSession;
        await _configService.SetConfig(_configService.Config);
    }

    public async Task OpenAuthPageAsync()
    {
        _token = ((LastResponse<string>)await _auth.GetAuthTokenAsync()).Content;
        
        Process.Start(new ProcessStartInfo
        {
            FileName = $"https://last.fm/api/auth/?api_key={_auth.ApiKey}&token={_token}",
            UseShellExecute = true
        });
    }
}