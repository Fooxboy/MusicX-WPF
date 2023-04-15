using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using MusicX.Avalonia.Pages;
using MusicX.Avalonia.ViewModels.ViewModels;

namespace MusicX.Avalonia.Extensions;

public class PageFactory : INavigationPageFactory
{
    public Control GetPage(Type srcType)
    {
        throw new NotSupportedException();
    }

    public Control GetPageFromObject(object target)
    {
        if (target is not ViewModelBase viewModel)
            throw new ArgumentException("Target must be ViewModelBase", nameof(target));

        UserControl control = viewModel switch
        {
            LoginModalViewModel => App.Provider.GetService<LoginPage>(),
            SectionTabViewModel => App.Provider.GetService<SectionPage>(),
            PlaylistViewModel => App.Provider.GetService<PlaylistPage>(),
            VideoModalViewModel => App.Provider.GetService<VideoPage>(),
            _ => throw new ArgumentOutOfRangeException(nameof(target), target,
                                                       "Target must be an implemented view model type in factory")
        };

        control.DataContext = viewModel;
        return control;
    }
}