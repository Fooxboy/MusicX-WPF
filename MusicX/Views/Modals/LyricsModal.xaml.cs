using MusicX.ViewModels.Modals;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;


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
            await _viewModel.LoadLyrics();

            SyncButton.Visibility = _viewModel.Timestamps != null ? Visibility.Visible : Visibility.Collapsed;
            if (_viewModel.Timestamps != null)
            {
                LyricsControlView.SetLines(_viewModel.Timestamps);
            }else
            {
                LyricsControlView.SetLines(_viewModel.Texts);
            }
          
        }
    }
}
