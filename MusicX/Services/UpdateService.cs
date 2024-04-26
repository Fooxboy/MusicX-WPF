using System.Threading.Tasks;
using MusicX.ViewModels.Modals;
using MusicX.Views.Modals;
using Velopack;
using Velopack.Sources;

namespace MusicX.Services;

public class UpdateService
{
    private readonly ConfigService _configService;
    private readonly NavigationService _navigationService;

    public UpdateService(ConfigService configService, NavigationService navigationService)
    {
        _configService = configService;
        _navigationService = navigationService;
    }

    public async Task<bool> CheckForUpdates()
    {
        var getBetaUpdates = _configService.Config.GetBetaUpdates.GetValueOrDefault();
        var manager = new UpdateManager(new GithubSource("https://github.com/Fooxboy/MusicX-WPF",
            string.Empty, getBetaUpdates, new HttpClientFileDownloader()), new()
        {
            ExplicitChannel = getBetaUpdates ? "win-beta" : "win"
        });

        var updateInfo = await manager.CheckForUpdatesAsync();
                
        if (updateInfo is null)
            return false;

        var viewModel = new AvailableNewUpdateModalViewModel(manager, updateInfo);

        _navigationService.OpenModal<AvailableNewUpdateModal>(viewModel);
        
        return true;
    }
}