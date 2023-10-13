using AsyncAwaitBestPractices;
using MusicX.Shared.Player;
using MusicX.ViewModels.Modals;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;


namespace MusicX.Views.Modals
{
    /// <summary>
    /// Логика взаимодействия для LyricsModal.xaml
    /// </summary>
    public partial class LyricsModal : Page
    {
        private LyricsViewModel _viewModel;
        private bool _syncText = true;

        public LyricsModal()
        {
            this.Loaded += LyricsModal_Loaded;
            
            InitializeComponent();
        }

        private void _viewModel_NextLineEvent(int msPoint)
        {
            LyricsControlView.ScrollToTime(msPoint);
        }

        private async void LyricsModal_Loaded(object sender, RoutedEventArgs e)
        {
            SyncButton.Content = "Не синхронизировать";
            _viewModel = DataContext as LyricsViewModel;

            _viewModel.NextLineEvent += _viewModel_NextLineEvent;
            _viewModel.NewTrack += _viewModel_NewTrack;

            await LoadTrack();
        }

        private async void _viewModel_NewTrack()
        {
            await LoadTrack();
        }

        private void SyncButton_Click(object sender, RoutedEventArgs e)
        {
            _syncText = !_syncText;

            SyncButton.Content = _syncText ? "Не синхронизировать" : "Синхронизировать";
            LyricsControlView.SetAutoScrollMode(_syncText);
        }

        private async Task LoadTrack()
        {
            if (_viewModel.Track.Data is not VkTrackData)
            {
                GeniusButton.IsEnabled = false;
                LyricFindButton.IsEnabled = false;
            }
            else if (!GeniusButton.IsEnabled && !LyricFindButton.IsEnabled)
            {
                GeniusButton.IsEnabled = true;
                LyricFindButton.IsEnabled = false;
            }
            LyricsControlView.SetLines(new List<string>());

            await _viewModel.LoadLyrics(LyricFindButton.IsEnabled);

            SyncButton.Visibility = _viewModel.Timestamps != null ? Visibility.Visible : Visibility.Collapsed;
            if (_viewModel.Timestamps != null)
            {
                LyricsControlView.SetLines(_viewModel.Timestamps);
            }else
            {
                LyricsControlView.SetLines(_viewModel.Texts);
            }
          
        }

        private void SourceButton_Click(object sender, RoutedEventArgs e)
        {
            if (LyricFindButton.IsEnabled)
            {
                LyricFindButton.IsEnabled = false;
                GeniusButton.IsEnabled = true;

                LoadTrack().SafeFireAndForget();
                return;
            }

            LyricFindButton.IsEnabled = true;
            GeniusButton.IsEnabled = false;

            LoadTrack().SafeFireAndForget();
        }
    }
}
