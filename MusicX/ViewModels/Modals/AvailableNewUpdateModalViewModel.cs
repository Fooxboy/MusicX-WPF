using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AsyncAwaitBestPractices.MVVM;
using Microsoft.AppCenter.Crashes;
using Microsoft.Extensions.DependencyInjection;
using MusicX.Helpers;
using MusicX.Services;
using MusicX.Services.Player;
using NLog;
using NuGet.Versioning;
using Velopack;
using Wpf.Ui;

namespace MusicX.ViewModels.Modals;

public sealed class AvailableNewUpdateModalViewModel : BaseViewModel
{
    private readonly UpdateManager _updateManager;

    public SemanticVersion? CurrentVersion => _updateManager.CurrentVersion;

    public UpdateInfo UpdateInfo { get; set; }
    
    public ICommand ApplyUpdatesCommand { get; }
    
    public bool IsUpdating { get; set; }
    
    public int Progress { get; set; }

    public AvailableNewUpdateModalViewModel(UpdateManager updateManager, UpdateInfo updateInfo)
    {
        _updateManager = updateManager;
        UpdateInfo = updateInfo;
        ApplyUpdatesCommand = new AsyncCommand(Execute);
    }

    private async Task Execute()
    {
        IsUpdating = true;
        try
        {
            void ProgressHandler(int i) => Progress = i;

            await _updateManager.DownloadUpdatesAsync(UpdateInfo, ProgressHandler);

            var playerState = PlayerState.CreateOrNull(StaticService.Container.GetRequiredService<PlayerService>());
            
            _updateManager.WaitExitThenApplyUpdates(UpdateInfo, restartArgs: new []
            {
                "--play",
                playerState is null ? "null" : JsonSerializer.Serialize(playerState)
            });
            
            Application.Current.Shutdown();
        }
        catch (Exception ex)
        {
            var snackbarService = StaticService.Container.GetRequiredService<ISnackbarService>();
            var logger = StaticService.Container.GetRequiredService<Logger>();

            var properties = new Dictionary<string, string>
            {
#if DEBUG
                { "IsDebug", "True" },
#endif
                { "Version", StaticService.Version }
            };
            Crashes.TrackError(ex, properties);
            logger.Error(ex, ex.Message);

            snackbarService.ShowException("Неудалось обновить приложение",
                $"Произошла ошибка при обновлении приложения: {ex.Message}");
            IsUpdating = false;
        }
    }
}
