using System;
using Microsoft.Extensions.DependencyInjection;
using MusicX.Helpers;
using MusicX.Services;
using MusicX.ViewModels.Login;
using NLog;
using Wpf.Ui;
using NavigationService = MusicX.Services.NavigationService;

namespace MusicX.Views.Login;

public partial class AccountsWindow
{
    private readonly Logger _logger;
    private readonly ISnackbarService _snackbarService;
    private readonly NavigationService _navigationService;

    public AccountsWindow(Logger logger, LoginViewModel loginViewModel, ISnackbarService snackbarService, NavigationService navigationService, WindowThemeService themeService) : base(snackbarService, navigationService,
        themeService)
    {
        _logger = logger;
        _snackbarService = snackbarService;
        _navigationService = navigationService;
        InitializeComponent();
        navigationService.ExternalPageOpened += NavigationServiceOnExternalPageOpened;
        LoginAsync(loginViewModel);
    }

    private async void LoginAsync(LoginViewModel viewModel)
    {
        try
        {
            _navigationService.OpenExternalPage(new LoginPage(viewModel));
            if (await viewModel.LoggedIn.Task)
            {
                var rootWindow = ActivatorUtilities.CreateInstance<RootWindow>(StaticService.Container);
                rootWindow.Show();
            }
        }
        catch (Exception e)
        {
            _snackbarService.ShowException("Не удалось авторизоваться!", e);
            _logger.Error(e);
        }
        await CloseAsync();
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        _navigationService.ExternalPageOpened -= NavigationServiceOnExternalPageOpened;
    }

    private void NavigationServiceOnExternalPageOpened(object? sender, object e)
    {
        Content = e;
    }
}