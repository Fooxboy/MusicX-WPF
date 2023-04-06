using System.Reactive;
using System.Reactive.Linq;
using DynamicData.Binding;
using MusicX.Avalonia.Core.Extensions;
using MusicX.Avalonia.Core.Models;
using MusicX.Avalonia.Core.Services;
using ReactiveUI;
using VkApi;
using VkApi.Core;
using VkApi.Core.Requests;

namespace MusicX.Avalonia.Core.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly IServiceProvider _provider;

    public IObservableCollection<MenuTabViewModel> MenuTabs { get; } =
        new ObservableCollectionExtended<MenuTabViewModel>();

    public MenuTabViewModel? CurrentTab { get; set; } = null;

    public ModalViewModel? CurrentModal { get; set; } = null;

    public MainWindowViewModel(ConfigurationService configurationService, IServiceProvider provider)
    {
        _provider = provider;

        MessageBus.Current.Listen<LoginState?>()
                  .Log(this, stringifier: state => state is null ? "Logged out" : $"Logged in as {state.UserId}")
                  .SelectMany(OnLoginStateChanged)
                  .Subscribe();
        
        MessageBus.Current.SendMessage(configurationService.LoginState);
    }

    private async Task<Unit> OnLoginStateChanged(LoginState? state)
    {
        if (state is null)
        {
            CurrentTab = null;
            MenuTabs.Clear();
            CurrentModal = (ModalViewModel?)_provider.GetService(typeof(LoginModalViewModel));
            return default;
        }

        var api = (Api)_provider.GetService(typeof(Api))!;

        var mainCatalog = await api.GetCatalogAudioAsync(new(null, true, null, null));
        
        foreach (var (id, title, _) in mainCatalog.Catalog.Sections)
        {
            var viewModel = (SectionTabViewModel)_provider.GetService(typeof(SectionTabViewModel))!;

            viewModel.Init(id, title);
            
            MenuTabs.Add(viewModel);
            if (mainCatalog.Catalog.DefaultSection == id)
                CurrentTab = viewModel;
        }

        return default;
    }
}