using DryIoc;
using MusicX.Core.Models;
using MusicX.Core.Services;
using MusicX.Services;
using MusicX.Views;
using MusicX.Views.Modals;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MusicX.Controls
{
    /// <summary>
    /// Логика взаимодействия для TrackControl.xaml
    /// </summary>
    public partial class TrackControl : UserControl
    {
        private readonly Logger logger;
        private readonly PlayerService player;


        public BitmapImage BitImage { get; set; }

        public TrackControl()
        {
            InitializeComponent();
            logger = StaticService.Container.Resolve<Logger>();
            player = StaticService.Container.Resolve<PlayerService>();


            player.TrackChangedEvent += Player_TrackChangedEvent;
            player.PlayStateChangedEvent += Player_PlayStateChangedEvent;

            this.Unloaded += TrackControl_Unloaded;

        }

        private void Player_PlayStateChangedEvent(object? sender, EventArgs e)
        {
            if(player.CurrentTrack.Id == this.Audio.Id)
            {
                if(player.IsPlaying)
                {
                    this.IconPlay.Glyph = WPFUI.Common.Icon.Pause24;

                }else
                {
                    this.IconPlay.Glyph = WPFUI.Common.Icon.Play24;

                }
            }
        }

        private void Player_TrackChangedEvent(object? sender, EventArgs e)
        {
            if(player.CurrentTrack.Id == this.Audio.Id)
            {
                this.IconPlay.Glyph = WPFUI.Common.Icon.Pause24;
            }else
            {
                this.IconPlay.Glyph = WPFUI.Common.Icon.Play24;
            }
        }

        private void TrackControl_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Cover.ImageSource = null;
        }

        public static readonly DependencyProperty ShowCardProperty =
            DependencyProperty.Register("ShowCard", typeof(bool), typeof(TrackControl), new PropertyMetadata(true));

        public bool ShowCard
        {
            get { return (bool)GetValue(ShowCardProperty); }
            set
            {
                SetValue(ShowCardProperty, value);
            }
        }

        public static readonly DependencyProperty ChartPositionProperty =
            DependencyProperty.Register("ChartPosition", typeof(int), typeof(TrackControl), new PropertyMetadata(0));

        public int ChartPosition
        {
            get { return (int)GetValue(ChartPositionProperty); }
            set
            {
                SetValue(ChartPositionProperty, value);
            }
        }

        public static readonly DependencyProperty AudioProperty =
          DependencyProperty.Register("Audio", typeof(Audio), typeof(TrackControl), new PropertyMetadata(new Audio()));

        public Audio Audio
        {
            get { return (Audio)GetValue(AudioProperty); }
            set
            {
                SetValue(AudioProperty, value);
            }
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {

              

                this.IconPlay.Glyph = WPFUI.Common.Icon.Play24;

                if (ShowCard) this.Card.Visibility = Visibility.Visible;
                else this.Card.Visibility = Visibility.Collapsed;

                Title.Text = Audio.Title;
                Subtitle.Text = Audio.Subtitle;
                if (ChartPosition != 0)
                {
                    ChartGrid.Visibility = Visibility.Visible;
                    ChartPositionText.Text = ChartPosition.ToString();
                }

                if(!Audio.IsAvailable || Audio.Url == String.Empty)
                {
                    Title.Text = Audio.Title;
                    Subtitle.Text = Audio.Subtitle;
                    Artists.Text = Audio.Artist;
                    this.Opacity = 0.3;
                    return;
                }

              
                if(BitImage != null)
                {
                    Cover.ImageSource = BitImage;
                }else
                {
                    if (Audio.Album != null)
                    {
                        if (Audio.Album.Cover != null) Cover.ImageSource = new BitmapImage(new Uri(Audio.Album.Cover)) { DecodePixelHeight = 45, DecodePixelWidth = 45, CacheOption = BitmapCacheOption.None };

                    }
                }

                var time = string.Empty;
                TimeSpan t = TimeSpan.FromSeconds(Audio.Duration);
                if (t.Hours > 0) time = t.ToString("h\\:mm\\:ss");
                time = t.ToString("m\\:ss");


                Time.Text = time;

                if (Audio.MainArtists?.Count > 0)
                {
                    string s = string.Empty;
                    foreach (var trackArtist in Audio.MainArtists)
                    {
                        s += trackArtist.Name + ", ";
                        var text = new TextBlock() { Text = trackArtist.Name, Tag = trackArtist.Id, Foreground = Brushes.White };
                        text.MouseLeftButtonDown += Text_MouseLeftButtonDown;
                        GoToArtistMenu.Items.Add(text);
                    }

                    var artists = s.Remove(s.Length - 2);

                    Artists.Text = artists;

                }
                else
                {
                    Artists.Text = Audio.Artist;
                }


                var configService = StaticService.Container.Resolve<Services.ConfigService>();


                var config = await configService.GetConfig();

                if (Audio.OwnerId == config.UserId)
                {
                    AddRemoveIcon.Glyph = WPFUI.Common.Icon.Delete20;
                    AddRemoveText.Text = "Удалить";
                }
                else
                {
                    AddRemoveIcon.Glyph = WPFUI.Common.Icon.Add24;
                    AddRemoveText.Text = "Добавить к себе";

                }


                if(Audio.IsExplicit)
                {
                    explicitBadge.Visibility = Visibility.Visible;
                }else
                {
                    explicitBadge.Visibility = Visibility.Collapsed;

                }

               
                if(this.ActualWidth > 110)
                {
                    NamePanel.MaxWidth = this.ActualWidth - 110;

                    if (Audio.IsExplicit)
                    {
                        Title.MaxWidth = (NamePanel.MaxWidth - 20);

                    }
                    else
                    {
                        Title.MaxWidth = (NamePanel.MaxWidth);
                    }

                    Artists.MaxWidth = this.ActualWidth;
                }
                

            }
            catch (Exception ex)
            {
                logger.Error("Failed load track control");
                logger.Error(ex, ex.Message);

                Title.Text = "Невозможно загрузить";
                Subtitle.Text = "это аудио";

                Artists.Text = "Попробуйте позже";
            }
            
        }

        private async void Text_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
           var textBlock = (TextBlock)sender;
            
            try
            {
                var navigationService = StaticService.Container.Resolve<Services.NavigationService>();

                if (Audio.MainArtists == null)
                {
                    await navigationService.OpenSearchSection(Audio.Artist);
                }
                else
                {
                    await navigationService.OpenArtistSection(Audio.MainArtists[0].Id);
                }
            }catch(Exception ex)
            {
                logger.Error(ex, ex.Message);
            }

        }


        double oldWidth = 0;
        double oldWidthArtists = 0;

        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            try
            {

                if (ShowCard)
                {
                    oldWidth = Title.ActualWidth;
                    oldWidthArtists = Artists.ActualWidth;
                    Title.MaxWidth = 120;
                    Subtitle.Visibility = Visibility.Collapsed;
                    Artists.MaxWidth = 120;

                }

                explicitBadge.Margin = new Thickness(7, 0, 0, 0);

                RecommendedAudio.Visibility = Visibility.Visible;
                PlayButtons.Visibility = Visibility.Visible;
                if (!ShowCard)
                {
                    Card.Visibility = Visibility.Visible;

                }


                Card.Opacity = 0.5;
            }catch(Exception ex)
            {
                logger.Error(ex, ex.Message);

            }

        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            try
            {

                if (ShowCard)
                {
                    Title.MaxWidth = oldWidth + 2;
                    Subtitle.Visibility = Visibility.Visible;

                    Artists.MaxWidth = oldWidthArtists + 2;
                }

                explicitBadge.Margin = new Thickness(0, 0, 0, 0);


                RecommendedAudio.Visibility = Visibility.Collapsed;
                if (Card == null) return;
                PlayButtons.Visibility = Visibility.Collapsed;

                if (!ShowCard)
                {
                    Card.Visibility = Visibility.Collapsed;
                }

                Card.Opacity = 1;
            }catch(Exception ex)
            {
                logger.Error(ex, ex.Message);

            }
        }

        private async void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                await Task.Delay(100);
                if (clickToArtist) return;
                if (Audio.Url == String.Empty)
                {
                    var navigationService = StaticService.Container.Resolve<Services.NavigationService>();
                    var vkService = StaticService.Container.Resolve<VkService>();

                    navigationService.OpenModal(new TrackNotAvalibleModal(vkService, navigationService, Audio.TrackCode, Audio.OwnerId + "_" + Audio.Id + "_" + Audio.AccessKey), 280, 550);

                    return;
                }

                await player.PlayTrack(Audio);
            }catch(Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
           
        }

        private void Artists_MouseEnter(object sender, MouseEventArgs e)
        {
            try
            {
                if (Artists == null) return;

                Artists.TextDecorations.Add(TextDecorations.Underline);
                this.Cursor = Cursors.Hand;
            }catch (Exception ex)
            {
                logger.Error(ex, ex.Message);

            }

        }

        private void Artists_MouseLeave(object sender, MouseEventArgs e)
        {
            try
            {
                if (Artists == null) return;
                foreach (var dec in TextDecorations.Underline)
                {
                    Artists.TextDecorations.Remove(dec);
                }
                this.Cursor = Cursors.Arrow;
            }catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }

        }

        bool clickToArtist = false;
        private async void Artists_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                clickToArtist = true;
                var navigationService = StaticService.Container.Resolve<Services.NavigationService>();

                if (Audio.MainArtists == null)
                {
                    await navigationService.OpenSearchSection(Audio.Artist);
                }
                else
                {
                    await navigationService.OpenArtistSection(Audio.MainArtists[0].Id);
                }

                clickToArtist = false;
            }
            catch (Exception ex)
            {
                clickToArtist = false;

                logger.Error(ex, ex.Message);
            }
        }

        private async void AddRemove_MouseDown(object sender, MouseButtonEventArgs e)
        {
           var configService = StaticService.Container.Resolve<Services.ConfigService>();
            var vkService = StaticService.Container.Resolve<VkService>();


            var config = await configService.GetConfig();

            if (Audio.OwnerId == config.UserId)
            {
                await vkService.AudioDeleteAsync(Audio.Id, Audio.OwnerId);
            }else
            {
                await vkService.AudioAddAsync(Audio.Id, Audio.OwnerId);

            }
        }

        private async void Download_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var downloader = StaticService.Container.Resolve<Services.DownloaderService>();

            try
            {
                await downloader.AddToQueueAsync(Audio);

            }catch(FileNotFoundException)
            {
                var navigation = StaticService.Container.Resolve<Services.NavigationService>();
                navigation.NavigateToPage(new DownloadsView());
            }
        }

        private void Title_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Audio.Album != null)
            {
                Title.TextDecorations.Add(TextDecorations.Underline);
                this.Cursor = Cursors.Hand;
            }
        }

        private void Title_MouseLeave(object sender, MouseEventArgs e)
        {
            foreach (var dec in TextDecorations.Underline)
            {
                Title.TextDecorations.Remove(dec);
            }
            this.Cursor = Cursors.Arrow;
        }

        private async void Title_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            clickToArtist = true;

            try
            {
                if (Audio.Album != null)
                {
                    var navigationService = StaticService.Container.Resolve<Services.NavigationService>();
                    navigationService.NavigateToPage(new PlaylistView(Audio.Album.Id, Audio.Album.OwnerId, Audio.Album.AccessKey));
                }

                await Task.Delay(100);

                clickToArtist = false;

            }
            catch (Exception ex)
            {
                clickToArtist = false;

                logger.Error(ex, ex.Message);
            }
            
        }

        private void RecommendedAudio_MouseEnter(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Hand;

          
            var amim = (Storyboard)(this.Resources["OpenAnimation"]);
            amim.Begin();
        }

        private void RecommendedAudio_MouseLeave(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Arrow;

          

            var amim = (Storyboard)(this.Resources["CloseAnimation"]);
            amim.Begin();
        }

        private async void RecommendedAudio_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var notifications = StaticService.Container.Resolve<Services.NotificationsService>();

                notifications.Show("Уже ищем", "Сейчас мы найдем похожие треки, подождите");

                clickToArtist = true;
                var vk = StaticService.Container.Resolve<VkService>();

                var items = await vk.GetRecommendationsAudio(Audio.OwnerId + "_" + Audio.Id);

                var navigation = StaticService.Container.Resolve<Services.NavigationService>();

                var ids = new List<string>();

                foreach (var audio in items.Response.Items)
                {
                    ids.Add(audio.OwnerId + "_" + audio.Id + "_" + audio.AccessKey);
                }

                var block = new MusicX.Core.Models.Block { Audios = items.Response.Items, AudiosIds = ids, DataType = "music_audios", Layout = new Layout() { Name = "list" } };
                var title = new MusicX.Core.Models.Block { DataType = "none", Layout = new Layout() { Name = "header", Title = $"Треки похожие на \"{Audio.Title}\"" } };

                var blocks = new List<Core.Models.Block>();
                blocks.Add(title);
                blocks.Add(block);

                await navigation.OpenSectionByBlocks(blocks);


                clickToArtist = false;
            }
            catch (Exception ex)
            {
                clickToArtist = false;

                logger.Error(ex, ex.Message);
                var notifications = StaticService.Container.Resolve<Services.NotificationsService>();

                notifications.Show("Ошибка", "Мы не смогли найти подходящие треки");

            }


        }
    }
}
