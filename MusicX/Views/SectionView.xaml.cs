using System;
using System.Runtime.Serialization;
using DryIoc;
using MusicX.Services;
using MusicX.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using AsyncAwaitBestPractices;
using Microsoft.Extensions.DependencyInjection;
using MusicX.Controls;
using NavigationService = System.Windows.Navigation.NavigationService;

namespace MusicX.Views;

/// <summary>
/// Логика взаимодействия для SectionView.xaml
/// </summary>
public partial class SectionView : Page, IProvideCustomContentState, IMenuPage
{
    public SectionView()
    {
        InitializeComponent();
    }
    public CustomContentState GetContentState()
    {
        return new SectionState((SectionViewModel)DataContext);
    }
    
    private async void SectionScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        if (_loading || Math.Abs(e.VerticalOffset - ((ScrollViewer)sender).ScrollableHeight) is > 200 or < 1)
            return;

        _loading = true;
        await ((SectionViewModel)DataContext).LoadMore();
        _loading = false;
    }
    
    [Serializable]
    private class SectionState : CustomContentState, ISerializable
    {
        private const string TypeKey = "SectionType";
        private const string IdKey = "SectionId";
        
        public override string JournalEntryName => _viewModel.SectionId;

        private SectionViewModel _viewModel;
        public SectionState(SectionViewModel viewModel)
        {
            _viewModel = viewModel;
        }
        
        public SectionState(SerializationInfo info, StreamingContext context)
        {
            _viewModel = ActivatorUtilities.CreateInstance<SectionViewModel>(StaticService.Container);
            var sectionId = info.GetString("IdKey") ?? throw new SerializationException($"{IdKey} is required value");
            
            _viewModel.SectionId = sectionId;
            _viewModel.SectionType = (SectionType)info.GetInt32(TypeKey);
            _viewModel.LoadAsync().SafeFireAndForget();
        }
        
        public override void Replay(NavigationService navigationService, NavigationMode mode)
        {
            if (navigationService.Content is SectionView section)
                section.DataContext = _viewModel;
            else
                navigationService.Navigate(new SectionView
                {
                    DataContext = _viewModel
                });
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(TypeKey, (int)_viewModel.SectionType);
            info.AddValue(IdKey, _viewModel.Section.Id);
        }
    }
    public override string ToString()
    {
        return MenuTag;
    }
    public string MenuTag
    {
        get => _menuTag ?? ((SectionViewModel) DataContext).SectionId;
        set => _menuTag = value;
    }
    private string? _menuTag;
    private bool _loading;
}