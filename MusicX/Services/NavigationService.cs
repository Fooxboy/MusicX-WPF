using System;
using System.Windows;
using System.Windows.Controls;
using AsyncAwaitBestPractices;
using Microsoft.Extensions.DependencyInjection;
using MusicX.ViewModels;
namespace MusicX.Services;

public enum SectionType
{
    None,
    Artist,
    Search
}
public class NavigationService
{
    public event EventHandler<SectionViewModel>? ExternalSectionOpened;
    public event EventHandler<object>? ExternalPageOpened;
    public event EventHandler<string>? MenuSectionOpened;
    public event EventHandler? BackRequested;
    public event EventHandler<string>? ReplaceBlocksRequested;
    public event EventHandler<object>? ModalOpenRequested;
    public event EventHandler? ModalCloseRequested;

    private SectionViewModel MakeViewModel(string value, SectionType sectionType)
    {
        var viewModel = ActivatorUtilities.CreateInstance<SectionViewModel>(StaticService.Container);
        
        switch (sectionType)
        {
            case SectionType.None:
                viewModel.LoadSection(value).SafeFireAndForget();
                break;
            case SectionType.Artist:
                viewModel.LoadArtistSection(value).SafeFireAndForget();
                break;
            case SectionType.Search:
                viewModel.LoadSearchSection(value).SafeFireAndForget();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(sectionType), sectionType, null);
        }
        
        return viewModel;
    }

    public void OpenSection(string value, SectionType sectionType = SectionType.None)
    {
        var viewModel = MakeViewModel(value, sectionType);
        
        if (Application.Current.Dispatcher.CheckAccess())
            ExternalSectionOpened?.Invoke(this, viewModel);
        else
            Application.Current.Dispatcher.BeginInvoke(() => ExternalSectionOpened?.Invoke(this, viewModel));
    }
    
    public void OpenMenuSection(string value)
    {
        if (Application.Current.Dispatcher.CheckAccess())
            MenuSectionOpened?.Invoke(this, value);
        else
            Application.Current.Dispatcher.BeginInvoke(() => MenuSectionOpened?.Invoke(this, value));
    }

    public void OpenExternalPage(object content)
    {
        if (Application.Current.Dispatcher.CheckAccess())
            ExternalPageOpened?.Invoke(this, content);
        else
            Application.Current.Dispatcher.BeginInvoke(() => ExternalPageOpened?.Invoke(this, content));
    }

    public void ReplaceBlocks(string id)
    {
        if (Application.Current.Dispatcher.CheckAccess())
            ReplaceBlocksRequested?.Invoke(this, id);
        else
            Application.Current.Dispatcher.BeginInvoke(() => ReplaceBlocksRequested?.Invoke(this, id));
    }

    public void GoBack()
    {
        if (Application.Current.Dispatcher.CheckAccess())
            BackRequested?.Invoke(this, EventArgs.Empty);
        else
            Application.Current.Dispatcher.BeginInvoke(() => BackRequested?.Invoke(this, EventArgs.Empty));
    }

    public void OpenModal<TView>(object? dataContext = null) where TView : Page, new()
    {
        TView Create()
        {
            return new()
            {
                DataContext = dataContext
            };
        }
        
        if (Application.Current.Dispatcher.CheckAccess())
            ModalOpenRequested?.Invoke(this, Create());
        else
            Application.Current.Dispatcher.BeginInvoke(() => ModalOpenRequested?.Invoke(this, Create()));
    }

    public void CloseModal()
    {
        if (Application.Current.Dispatcher.CheckAccess())
            ModalCloseRequested?.Invoke(this, EventArgs.Empty);
        else
            Application.Current.Dispatcher.BeginInvoke(() => ModalCloseRequested?.Invoke(this, EventArgs.Empty));
    }
}
