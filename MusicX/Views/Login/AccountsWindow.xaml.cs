﻿using System;
using Microsoft.Extensions.DependencyInjection;
using MusicX.Services;
using MusicX.ViewModels.Login;
using NLog;
using Wpf.Ui;
using NavigationService = MusicX.Services.NavigationService;

namespace MusicX.Views.Login;

public partial class AccountsWindow
{
    private readonly NavigationService _navigationService;

    public AccountsWindow(ISnackbarService snackbarService, NavigationService navigationService, AccountsWindowViewModel viewModel, Logger logger) : base(snackbarService, navigationService, logger)
    {
        _navigationService = navigationService;
        navigationService.ExternalPageOpened += NavigationServiceOnExternalPageOpened;
        InitializeComponent();
        DataContext = viewModel;
        viewModel.OpenPage(AccountsWindowPage.EnterLogin);
        viewModel.LoggedIn += AccountsWindowOnLoggedIn;
    }

    private void AccountsWindowOnLoggedIn(object? sender, EventArgs e)
    {
        var rootWindow = ActivatorUtilities.CreateInstance<RootWindow>(StaticService.Container);
        rootWindow.Show();
        Close();
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        _navigationService .ExternalPageOpened -= NavigationServiceOnExternalPageOpened;
    }

    private void NavigationServiceOnExternalPageOpened(object? sender, object e)
    {
        Content = e;
    }
}