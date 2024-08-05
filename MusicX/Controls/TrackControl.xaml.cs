using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net.Cache;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.Extensions.DependencyInjection;
using MusicX.Core.Models;
using MusicX.Core.Services;
using MusicX.Helpers;
using MusicX.Patches;
using MusicX.Services;
using MusicX.Services.Player;
using MusicX.Services.Player.Playlists;
using MusicX.Shared.Player;
using MusicX.ViewModels;
using MusicX.ViewModels.Modals;
using MusicX.Views;
using MusicX.Views.Modals;
using NLog;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;
using NavigationService = MusicX.Services.NavigationService;
using TextBlock = System.Windows.Controls.TextBlock;

namespace MusicX.Controls
{
    /// <summary>
    /// Логика взаимодействия для TrackControl.xaml
    /// </summary>
    public partial class TrackControl : UserControl
    {
        private readonly Logger logger;
        private readonly PlayerService player;

        public TrackControl()
        {
            InitializeComponent();
            logger = StaticService.Container.GetRequiredService<Logger>();
            player = StaticService.Container.GetRequiredService<PlayerService>();
        }

        private void Player_PlayStateChangedEvent(object? sender, EventArgs e)
        {
            if (player.CurrentTrack is { Data: VkTrackData data } && data.Info.Id == Audio.Id)
            {
                IconPlay.Symbol = player.IsPlaying ? SymbolRegular.Pause24 : SymbolRegular.Play24;
            }
        }

        private void Player_TrackChangedEvent(object? sender, EventArgs e)
        {
            if (player.CurrentTrack is { Data: VkTrackData data } && data.Info.Id == Audio.Id)
            {
                PlayButtons.Visibility = Visibility.Visible;
            }
            else
            {
                PlayButtons.Visibility = Visibility.Collapsed;

                IconPlay.Symbol = SymbolRegular.Play24;
            }
        }

        public static readonly DependencyProperty LoadOtherTracksProperty = DependencyProperty.Register(
            "LoadOtherTracks", typeof(bool), typeof(TrackControl), new PropertyMetadata(true));

        public bool LoadOtherTracks
        {
            get => (bool)GetValue(LoadOtherTracksProperty);
            set => SetValue(LoadOtherTracksProperty, value);
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

        public static readonly DependencyProperty InChartProperty = DependencyProperty.Register(
            nameof(InChart), typeof(bool), typeof(TrackControl), new PropertyMetadata(default(bool)));

        public bool InChart
        {
            get { return (bool)GetValue(InChartProperty); }
            set { SetValue(InChartProperty, value); }
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
        
        private int IndexInItemsControl =>
            ItemContainerGeneratorIndexHook.GetItemContainerIndex(((FrameworkElement?)TemplatedParent)?.TemplatedParent ?? TemplatedParent ?? this);
        
        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                player.TrackChangedEvent += Player_TrackChangedEvent;
                player.PlayStateChangedEvent += Player_PlayStateChangedEvent;

                IconPlay.Symbol = SymbolRegular.Play24;

                if(!Audio.IsAvailable || string.IsNullOrEmpty(Audio.Url))
                {
                    Artists.Text = Audio.Artist;
                    Opacity = 0.3;
                    return;
                }

                var t = TimeSpan.FromSeconds(Audio.Duration);
                var time = t.ToString(t.Hours > 0 ? @"h\:mm\:ss" : @"m\:ss");
                Time.Text = time;

                if (Audio.MainArtists is null or {Count: 0})
                {
                    Artists.Text = Audio.Artist;
                    Artists.MouseEnter += Artists_MouseEnter;
                    Artists.MouseLeave += Artists_MouseLeave;
                    Artists.MouseLeftButtonDown += Artists_MouseLeftButtonDown;

                    GoToArtistMenu.Items.Add(new MainArtist
                    {
                        Id = Audio.Artist,
                        Name = Audio.Artist
                    });
                }
                else
                {
                    Artists.Inlines.Clear();
                    AddArtists(Audio.MainArtists);
                }
                
                if (Audio.FeaturedArtists?.Count > 0)
                    Artists.Inlines.Add(" feat. ");
                if (Audio.FeaturedArtists is not null)
                    AddArtists(Audio.FeaturedArtists);
                
                var configService = StaticService.Container.GetRequiredService<ConfigService>();


                var config = await configService.GetConfig();

                if (Audio.OwnerId == config.UserId)
                {
                    AddRemoveIcon.Symbol = SymbolRegular.Delete20;
                    AddRemoveText.Text = "Удалить";
                }
                else
                {
                    AddRemoveIcon.Symbol = SymbolRegular.Add20;
                    AddRemoveText.Text = "Добавить к себе";

                }


                if(Audio.IsExplicit)
                {
                    explicitBadge.Visibility = Visibility.Visible;
                }else
                {
                    explicitBadge.Visibility = Visibility.Collapsed;

                }


                if (player.CurrentTrack is { Data: VkTrackData data } && data.Info.Id == Audio.Id)
                {
                    PlayButtons.Visibility = Visibility.Visible;
                    IconPlay.Symbol = SymbolRegular.Pause24;
                }
                
                ChartTextBlock.Text = (IndexInItemsControl + 1).ToString();
            }
            catch (Exception ex)
            {
                var properties = new Dictionary<string, string>
                {
#if DEBUG
                    { "IsDebug", "True" },
#endif
                    {"Version", StaticService.Version }
                };
                Crashes.TrackError(ex, properties);

                logger.Error("Failed load track control");
                logger.Error(ex, ex.Message);
                Opacity = 0.3;
            }
            
        }
        private void AddArtists(IEnumerable<MainArtist> artists)
        {
            var first = true;
            foreach (var artist in artists)
            {
                if (first)
                    first = false;
                else
                    Artists.Inlines.Add(", ");
                        
                var textBlock = new TextBlock
                {
                    Text = artist.Name,
                    DataContext = artist
                };
                    
                textBlock.MouseEnter += Artists_MouseEnter;
                textBlock.MouseLeave += Artists_MouseLeave;
                textBlock.MouseLeftButtonDown += Artists_MouseLeftButtonDown;
                    
                Artists.Inlines.Add(textBlock);

                GoToArtistMenu.Items.Add(artist);
            }
        }

        private async void ArtistContextMenu_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var navigationService = StaticService.Container.GetRequiredService<NavigationService>();

                if (Audio.MainArtists == null)
                {
                    navigationService.OpenSection(Audio.Artist, SectionType.Search);
                }
                else
                {
                    navigationService.OpenSection((string)((TextBlock)sender).Tag, SectionType.Artist);
                }
            }catch(Exception ex)
            {
                var properties = new Dictionary<string, string>
                {
#if DEBUG
                    { "IsDebug", "True" },
#endif
                    {"Version", StaticService.Version }
                };
                Crashes.TrackError(ex, properties);

                logger.Error(ex, ex.Message);
                StaticService.Container.GetRequiredService<ISnackbarService>()
                    .ShowException("Нам не удалось перейти на эту секцию", ex);
            }

        }


        double oldWidth = 0;
        double oldWidthArtists = 0;

        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            try
            {
                if (player.CurrentTrack is not { Data: VkTrackData data } || data.Info.Id != Audio.Id)
                {
                    PlayButtons.Visibility = Visibility.Visible;
                }
                
            }catch(Exception ex)
            {

                var properties = new Dictionary<string, string>
                {
#if DEBUG
                    { "IsDebug", "True" },
#endif
                    {"Version", StaticService.Version }
                };
                Crashes.TrackError(ex, properties);

                logger.Error(ex, ex.Message);

            }

        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            try
            {
                if (player.CurrentTrack is not { Data: VkTrackData data } || data.Info.Id != Audio.Id)
                {
                    PlayButtons.Visibility = Visibility.Collapsed;
                }
            }catch(Exception ex)
            {

                var properties = new Dictionary<string, string>
                {
#if DEBUG
                    { "IsDebug", "True" },
#endif
                    {"Version", StaticService.Version }
                };
                Crashes.TrackError(ex, properties);

                logger.Error(ex, ex.Message);

            }
        }

        private async void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var properties = new Dictionary<string, string>
                {
#if DEBUG
                    { "IsDebug", "True" },
#endif
                    {"Version", StaticService.Version }
                };
                Analytics.TrackEvent("PlayTrack", properties);
                
                var connectionService = StaticService.Container.GetRequiredService<BackendConnectionService>();
                connectionService.ReportMetric("PlayTrack", "TrackControl");

                if (e.Source is TextBlock)
                    return;

                if (string.IsNullOrEmpty(Audio.Url))
                {
                    var navigationService = StaticService.Container.GetRequiredService<NavigationService>();
                    var modalViewModel = StaticService.Container.GetRequiredService<TrackNotAvalibleModalViewModel>();
                    await modalViewModel.LoadAsync(Audio.TrackCode, Audio.OwnerId + "_" + Audio.Id + "_" + Audio.AccessKey);

                    navigationService.OpenModal<TrackNotAvalibleModal>(modalViewModel);
                    return;
                }

                var index = IndexInItemsControl;

                //костыль для бума, да мне лень править.
                if (Audio.Url.EndsWith(".mp3"))
                {
                    await player.PlayAsync(new SinglePlaylist(Audio.ToTrack()), index);
                    return;
                }

                var vkService = StaticService.Container.GetRequiredService<VkService>();


                if (this.FindAncestor<PlaylistView>() is { DataContext: PlaylistViewModel viewModel })
                    await player.PlayAsync(
                        new VkPlaylistPlaylist(vkService, viewModel.PlaylistData with { Count = (int)viewModel.Playlist.Count }), index);
                //костыль для реков, да мне лень править.
                else if (Audio.ParentBlockId is "recomms" or "track_recomms_full" &&
                         this.FindAncestor<BlockControl>() is { DataContext: Block { Audios.Count: > 0 } block })
                    await player.PlayAsync(new ListPlaylist(block.Audios.Select(TrackExtensions.ToTrack).ToImmutableList()), index);
                else
                    await player.PlayAsync(new VkBlockPlaylist(vkService, Audio.ParentBlockId, LoadOtherTracks), index);
            }catch(Exception ex)
            {
                var properties = new Dictionary<string, string>
                {
#if DEBUG
                    { "IsDebug", "True" },
#endif
                    {"Version", StaticService.Version }
                };
                Crashes.TrackError(ex, properties);

                logger.Error(ex, ex.Message);
            }
           
        }

        private void Artists_MouseEnter(object sender, MouseEventArgs e)
        {
            try
            {
                if (sender is not TextBlock block)
                    return;
                block.TextDecorations.Add(TextDecorations.Underline);
                Cursor = Cursors.Hand;
            }catch (Exception ex)
            {

                var properties = new Dictionary<string, string>
                {
#if DEBUG
                    { "IsDebug", "True" },
#endif
                    {"Version", StaticService.Version }
                };
                Crashes.TrackError(ex, properties);

                logger.Error(ex, ex.Message);

            }

        }

        private void Artists_MouseLeave(object sender, MouseEventArgs e)
        {
            try
            {
                if (sender is not TextBlock block)
                    return;
                foreach (var dec in TextDecorations.Underline)
                {
                    block.TextDecorations.Remove(dec);
                }
                Cursor = Cursors.Arrow;
            }catch (Exception ex)
            {
                var properties = new Dictionary<string, string>
                {
#if DEBUG
                    { "IsDebug", "True" },
#endif
                    {"Version", StaticService.Version }
                };
                Crashes.TrackError(ex, properties);

                logger.Error(ex, ex.Message);
            }

        }

        private void Artists_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var navigationService = StaticService.Container.GetRequiredService<NavigationService>();

                if (Audio.MainArtists == null)
                {
                    navigationService.OpenSection(Audio.Artist, SectionType.Search);
                }
                else if (sender is FrameworkElement {DataContext: MainArtist artist})
                {
                    navigationService.OpenSection(artist.Id, SectionType.Artist);
                }
            }
            catch (Exception ex)
            {

                var properties = new Dictionary<string, string>
                {
#if DEBUG
                    { "IsDebug", "True" },
#endif
                    {"Version", StaticService.Version }
                };
                Crashes.TrackError(ex, properties);

                logger.Error(ex, ex.Message);
                StaticService.Container.GetRequiredService<ISnackbarService>()
                    .ShowException("Нам не удалось перейти на эту секцию", ex);
            }
        }

        private async void AddRemove_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var configService = StaticService.Container.GetRequiredService<ConfigService>();
                var vkService = StaticService.Container.GetRequiredService<VkService>();
                var eventService = StaticService.Container.GetRequiredService<SectionEventService>();

                var config = await configService.GetConfig();

                if (Audio.OwnerId == config.UserId)
                {
                    await vkService.AudioDeleteAsync(Audio.Id, Audio.OwnerId);
                    eventService.Dispatch(this, SectionEvent.AudiosRemove);
                }
                else
                {
                    await vkService.AudioAddAsync(Audio.Id, Audio.OwnerId);
                    eventService.Dispatch(this, SectionEvent.AudiosAdd);
                } 
            }catch(Exception ex)
            {
                logger.Error(ex, ex.Message);
                var properties = new Dictionary<string, string>
                {
#if DEBUG
                    { "IsDebug", "True" },
#endif
                    {"Version", StaticService.Version }
                };
                Crashes.TrackError(ex, properties);
            }
           
        }

        private void Download_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var downloader = StaticService.Container.GetRequiredService<DownloaderViewModel>();

            try
            {
                downloader.DownloadQueue.Add(Audio.ToTrack());
                downloader.StartDownloadingCommand.Execute(null);
            }catch(FileNotFoundException)
            {



                var navigation = StaticService.Container.GetRequiredService<NavigationService>();
                navigation.OpenMenuSection("downloads");
            }
        }

        private async void Title_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (Audio.Album != null)
                {
                    var navigationService = StaticService.Container.GetRequiredService<NavigationService>();
                    navigationService.OpenExternalPage(new PlaylistView(Audio.Album.Id, Audio.Album.OwnerId, Audio.Album.AccessKey));
                }

            }
            catch (Exception ex)
            {

                var properties = new Dictionary<string, string>
                {
#if DEBUG
                    { "IsDebug", "True" },
#endif
                    {"Version", StaticService.Version }
                };
                Crashes.TrackError(ex, properties);

                logger.Error(ex, ex.Message);
            }
            
        }

        private async void RecommendedAudio_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            try
            {
                var snackbarService = StaticService.Container.GetRequiredService<ISnackbarService>();

                snackbarService.Show("Уже ищем", "Сейчас мы найдем похожие треки, подождите", ControlAppearance.Success);

                var vk = StaticService.Container.GetRequiredService<VkService>();

                var items = await vk.GetRecommendationsAudio(Audio.OwnerId + "_" + Audio.Id);

                var navigation = StaticService.Container.GetRequiredService<NavigationService>();
                
                foreach (var audio in items.Items)
                {
                    audio.ParentBlockId = "recomms";
                }

                var ids = items.Items.Select(audio => $"{audio.OwnerId}_{audio.Id}_{audio.AccessKey}").ToList();

                var block = new Block { Id = "recomms", Audios = items.Items, AudiosIds = ids, DataType = "music_audios", Layout = new Layout() { Name = "list" } };
                var title = new Block { DataType = "none", Layout = new Layout() { Name = "header", Title = $"Треки похожие на \"{Audio.Title}\"" } };

                var blocks = new List<Block>
                {
                    title,
                    block
                };

                navigation.OpenBlocks(blocks);
            }
            catch (Exception ex)
            {

                var properties = new Dictionary<string, string>
                {
#if DEBUG
                    { "IsDebug", "True" },
#endif
                    {"Version", StaticService.Version }
                };
                Crashes.TrackError(ex, properties);

                logger.Error(ex, ex.Message);
                var snackbarService = StaticService.Container.GetRequiredService<ISnackbarService>();

                snackbarService.ShowException("Мы не смогли найти подходящие треки", ex);

            }


        }
        private void PlayNext_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                player.InsertToQueue(Audio.ToTrack(), true);
            }
            catch (Exception ex)
            {

                var properties = new Dictionary<string, string>
                {
#if DEBUG
                    { "IsDebug", "True" },
#endif
                    {"Version", StaticService.Version }
                };
                Crashes.TrackError(ex, properties);

                logger.Error(ex);
            }
        }
        private void AddToQueue_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                player.InsertToQueue(Audio.ToTrack(), false);
            }
            catch (Exception ex)
            {

                var properties = new Dictionary<string, string>
                {
#if DEBUG
                    { "IsDebug", "True" },
#endif
                    {"Version", StaticService.Version }
                };
                Crashes.TrackError(ex, properties);

                logger.Error(ex);
            }
        }

        private void AddToPlaylist_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var viewModel = StaticService.Container.GetRequiredService<PlaylistSelectorModalViewModel>();
            var navigationService = StaticService.Container.GetRequiredService<NavigationService>();

            viewModel.PlaylistSelected += ViewModel_PlaylistSelected;

            navigationService.OpenModal<PlaylistSelectorModal>(viewModel);
        }

        private async void ViewModel_PlaylistSelected(Playlist playlist)
        {
            var vk = StaticService.Container.GetRequiredService<VkService>();
            var snackbarService = StaticService.Container.GetRequiredService<ISnackbarService>();

            try
            {
                await vk.AddToPlaylistAsync(Audio, playlist.OwnerId, playlist.Id);

                snackbarService.Show("Трек добавлен", $"Трек '{Audio.Title}' добавлен в плейлист '{playlist.Title}'", ControlAppearance.Success);


            }
            catch (Exception ex)
            {

                var properties = new Dictionary<string, string>
                {
#if DEBUG
                    { "IsDebug", "True" },
#endif
                    {"Version", StaticService.Version }
                };
                Crashes.TrackError(ex, properties);

                logger.Error(ex, ex.Message);

                snackbarService.ShowException("Ошибка при добавлении трека в плейлист", ex);
            }
        }

        private async void AddArtistIgnore_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var snackbarService = StaticService.Container.GetRequiredService<ISnackbarService>();
            var configService = StaticService.Container.GetRequiredService<ConfigService>();

            try
            {
                var config = await configService.GetConfig();

                if(config.IgnoredArtists is null) config.IgnoredArtists = new List<string>();

                if(Audio.MainArtists!= null) config.IgnoredArtists?.AddRange(Audio.MainArtists.Select(x => x.Name));
                if (Audio.FeaturedArtists != null) config.IgnoredArtists?.AddRange(Audio.FeaturedArtists.Select(x => x.Name));

                await configService.SetConfig(config);

                snackbarService.Show("Готово!", "Теперь треки с этим исполнителем будет автоматически пропускаться", ControlAppearance.Success);
            }
            catch (Exception ex)
            {
                var properties = new Dictionary<string, string>
                {
                    {"Version", StaticService.Version }
                };
                Crashes.TrackError(ex, properties);

                logger.Error(ex, ex.Message);

                snackbarService.ShowException("Ошибка",
                    "Произошла ошибка при добавлении добавлении исполнителя в черный список");
            }
        }

        private void TrackControl_OnUnloaded(object sender, RoutedEventArgs e)
        {
            player.TrackChangedEvent -= Player_TrackChangedEvent;
            player.PlayStateChangedEvent -= Player_PlayStateChangedEvent;
        }

        private void MainGrid_OnMouseEnter(object sender, MouseEventArgs e)
        {
            RecommendedAudioColumn.Width = GridLength.Auto;
            ExplicitBadgeColumn.Width = new(0);
            TimeColumn.Width = new(0);
        }

        private void MainGrid_OnMouseLeave(object sender, MouseEventArgs e)
        {
            RecommendedAudioColumn.Width = new(0);
            ExplicitBadgeColumn.Width = GridLength.Auto;
            TimeColumn.Width = GridLength.Auto;
        }
    }
}
