using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using MusicX.Avalonia.Pages;
using MusicX.Avalonia.ViewModels.ViewModels;

namespace MusicX.Avalonia;

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
            _ => throw new ArgumentOutOfRangeException(nameof(target), target,
                                                       "Target must be an implemented view model type in factory")
        };

        control.DataContext = viewModel;
        return control;
    }
}