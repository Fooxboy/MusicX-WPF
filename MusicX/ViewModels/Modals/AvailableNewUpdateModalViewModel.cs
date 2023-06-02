using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AsyncAwaitBestPractices.MVVM;
using Squirrel;
using Squirrel.Sources;

namespace MusicX.ViewModels.Modals;

public sealed class AvailableNewUpdateModalViewModel : BaseViewModel, IDisposable
{
    private readonly UpdateManager _updateManager;
    
    public UpdateInfo UpdateInfo { get; set; }
    
    public ICommand ApplyUpdatesCommand { get; }
    
    public bool IsUpdating { get; set; }
    
    public int Progress { get; set; }
    
    public string Changelog { get; }

    public AvailableNewUpdateModalViewModel(UpdateManager updateManager, UpdateInfo updateInfo,
        Dictionary<ReleaseEntry, string> releaseNotes)
    {
        _updateManager = updateManager;
        UpdateInfo = updateInfo;
        ApplyUpdatesCommand = new AsyncCommand(Execute);

        if (releaseNotes.Count > 1)
        {
            var sb = new StringBuilder();

            foreach (var (key, value) in releaseNotes)
            {
                sb.AppendLine($"- {key.Version}: ").AppendLine(value);
            }

            Changelog = sb.ToString();
        }
        else Changelog = releaseNotes.First().Value;
    }

    private async Task Execute()
    {
        IsUpdating = true;
        try
        {
            void ProgressHandler(int i) => Progress = i;
            
            await _updateManager.DownloadReleases(UpdateInfo.ReleasesToApply, ProgressHandler);
            await _updateManager.ApplyReleases(UpdateInfo, ProgressHandler);
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