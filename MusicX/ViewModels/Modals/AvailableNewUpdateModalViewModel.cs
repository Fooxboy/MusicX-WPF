using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AsyncAwaitBestPractices;
using AsyncAwaitBestPractices.MVVM;
using Microsoft.AppCenter.Crashes;
using Microsoft.Extensions.DependencyInjection;
using MusicX.Core.Services;
using MusicX.Services;
using NLog;
using Squirrel;
using Wpf.Ui;

namespace MusicX.ViewModels.Modals;

public sealed class AvailableNewUpdateModalViewModel : BaseViewModel, IDisposable
{
    private readonly UpdateManager _updateManager;
    private readonly GithubService _githubService;

    public UpdateInfo UpdateInfo { get; set; }
    
    public ICommand ApplyUpdatesCommand { get; }
    
    public bool IsUpdating { get; set; }
    
    public int Progress { get; set; }

    public string Changelog { get; private set; } = "Загрузка...";

    public AvailableNewUpdateModalViewModel(UpdateManager updateManager, UpdateInfo updateInfo,
        GithubService githubService)
    {
        _updateManager = updateManager;
        _githubService = githubService;
        UpdateInfo = updateInfo;
        ApplyUpdatesCommand = new AsyncCommand(Execute);
        LoadChangelogAsync().SafeFireAndForget(ex =>
        {
            var snackbarService = StaticService.Container.GetRequiredService<ISnackbarService>();
            var logger = StaticService.Container.GetRequiredService<Logger>();
            
            var properties = new Dictionary<string, string>
            {
#if DEBUG
                { "IsDebug", "True" },
#endif
                {"Version", StaticService.Version }
            };
            Crashes.TrackError(ex, properties);
            logger.Error(ex, ex.Message);

            snackbarService.Show("Неудалось получить список изменений",
                $"Произошла ошибка при получении списка изменений: {ex.GetType().FullName}");
            Changelog = "Нет информации.";
        });
    }

    private async Task LoadChangelogAsync()
    {
        var sb = new StringBuilder();
        foreach (var entry in UpdateInfo.ReleasesToApply)
        {
            var release = await _githubService.GetReleaseByTag(entry.Version.ToFullString());

            sb.AppendLine($"- {entry.Version.ToFullString()}:").AppendLine(release.Body);
        }

        Changelog = sb.ToString();
    }

    private async Task Execute()
    {
        IsUpdating = true;
        try
        {
            void ProgressHandler(int i) => Progress = i;

            await _updateManager.DownloadReleases(UpdateInfo.ReleasesToApply, ProgressHandler);
            await _updateManager.ApplyReleases(UpdateInfo, ProgressHandler);

            UpdateManager.RestartApp();
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

            snackbarService.Show("Неудалось обновить приложение",
                $"Произошла ошибка при обновлении приложения: {ex.GetType().FullName}");
        }
        finally
        {
            IsUpdating = false;
        }
    }

    public void Dispose()
    {
        _updateManager.Dispose();
    }
}
