using DryIoc;
using MusicX.Controls.Blocks;
using MusicX.Core.Models;
using MusicX.Services;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using MusicX.ViewModels.Controls;
using Wpf.Ui.Controls;

namespace MusicX.Controls
{
    /// <summary>
    /// Логика взаимодействия для BlockControl.xaml
    /// </summary>
    public partial class BlockControl : UserControl
    {
        private readonly Services.NavigationService navigationService;
        public BlockControl()
        {
            InitializeComponent();

            navigationService = StaticService.Container.Resolve<Services.NavigationService>();
            this.Unloaded += BlockControl_Unloaded;
        }

        private void BlockControl_Unloaded(object sender, RoutedEventArgs e)
        {
            BlocksPanel.Children.Clear();
        }

        public static readonly DependencyProperty BlockProperty =
          DependencyProperty.Register("Block", typeof(Block), typeof(BlockControl), new PropertyMetadata(new Block()));

        public Block Block
        {
            get { return (Block)GetValue(BlockProperty); }
            set
            {
                SetValue(BlockProperty, value);
            }
        }

        public static readonly DependencyProperty ArtistProperty = DependencyProperty.Register(
            nameof(Artist), typeof(Artist), typeof(BlockControl));

        public Artist? Artist
        {
            get => (Artist)GetValue(ArtistProperty);
            set => SetValue(ArtistProperty, value);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var logger = StaticService.Container.Resolve<Logger>();

            try
            {
                if (Block.DataType == "artist")
                {
                    BlocksPanel.Children.Add(new ArtistBannerBlockControl(Block) { Block = Block });
                  
                    logger.Info($"loaded {Block.DataType} block with block id = {Block.Id}");
                    return;
                }

                if (Block.DataType == "texts")
                {
                    BlocksPanel.Children.Add(new TextsBlockControl() { Block = Block });

                    logger.Info($"loaded {Block.DataType} block with block id = {Block.Id}");

                    return;
                }

                if (Block.DataType == "placeholder")
                {
                    BlocksPanel.Children.Add(new PlaceholderBlockControl());

                    logger.Info($"loaded {Block.DataType} block with block id = {Block.Id}");

                    return;
                }

                if (Block.DataType == "groups")
                {
                    BlocksPanel.Children.Add(new GroupsBlockControl() { Block = Block});
                   
                    logger.Info($"loaded {Block.DataType} block with block id = {Block.Id}");

                    return;

                }

                if (Block.DataType == "curator")
                {
                    BlocksPanel.Children.Add(new CuratorBannerBlockControl() { Block = Block });
                   
                    logger.Info($"loaded {Block.DataType} block with block id = {Block.Id}");

                    return;
                }

                if (Block.DataType == "music_playlists")
                {

                    if (Block.Layout.Name == "music_chart_large_slider")
                    {
                        BlocksPanel.Children.Add(new ListPlaylists() { Playlists = Block.Playlists, /*TODO ShowChart = true,*/ ShowFull = false });
                        logger.Info($"loaded {Block.DataType} block with block id = {Block.Id}");

                        return;
                    }

                    if (Block.Layout.Name == "large_slider")
                    {
                        BlocksPanel.Children.Add(new ListPlaylists() { Playlists = Block.Playlists, ShowFull = false });
                        logger.Info($"loaded {Block.DataType} block with block id = {Block.Id}");

                        return;
                    }

                    if (Block.Layout.Name == "slider")
                    {
                        BlocksPanel.Children.Add(new ListPlaylists() { Playlists = Block.Playlists, ShowFull = false });
                        logger.Info($"loaded {Block.DataType} block with block id = {Block.Id}");

                        return;
                    }

                    if (Block.Layout.Name == "recomms_slider")
                    {
                        BlocksPanel.Children.Add(new ListPlaylists() { Playlists = Block.Playlists, ShowFull = false });
                        logger.Info($"loaded {Block.DataType} block with block id = {Block.Id}");

                        return;
                    }

                    if (Block.Layout.Name == "list")
                    {
                        BlocksPanel.Children.Add(new ListPlaylists() { Playlists = Block.Playlists, ShowFull = true });
                        logger.Info($"loaded {Block.DataType} block with block id = {Block.Id}");

                        return;
                    }

                    if (Block.Layout.Name == "compact_list")
                    {
                        BlocksPanel.Children.Add(new ListPlaylists() { Playlists = Block.Playlists, ShowFull = false });
                        logger.Info($"loaded {Block.DataType} block with block id = {Block.Id}");

                        return;
                    }
                }

                if (Block.DataType == "search_suggestions")
                {
                    
                    BlocksPanel.Children.Add(new SearchSuggestionsBlockControl() { Block = Block });
                    logger.Info($"loaded {Block.DataType} block with block id = {Block.Id}");
                    
                    return;
                }

                if (Block.DataType == "links")
                {

                    if (Block.Layout.Name == "categories_list" && Block.Links.FirstOrDefault().Title == "Недавнee")
                    {
                        var downloadsLink = Block.Links.SingleOrDefault(x => x.Title == "Скачанная музыка");
                        if(downloadsLink != null)
                        {
                            Block.Links.Remove(downloadsLink);
                        }

                        try
                        {
                            Block.Links.SingleOrDefault(x => x.Title == "Недавнee").Image[0].Url = "https://fooxboy.blob.core.windows.net/musicx/icons/recent_white.png";
                            Block.Links.SingleOrDefault(x => x.Title == "Плейлисты").Image[0].Url = "https://fooxboy.blob.core.windows.net/musicx/icons/playlists_white.png";
                            Block.Links.SingleOrDefault(x => x.Title == "Альбомы").Image[0].Url = "https://fooxboy.blob.core.windows.net/musicx/icons/albums_white.png";
                            Block.Links.SingleOrDefault(x => x.Title == "Подписки").Image[0].Url = "https://fooxboy.blob.core.windows.net/musicx/icons/follows_white.png";
                        }catch(Exception ex)
                        {
                            logger.Error("Music X не смог заменить картинки в моей музыке:");
                            logger.Error(ex,ex.Message);
                        }


                        BlocksPanel.Children.Add(new MusicCategoryBlockControl() { Links = Block.Links });
                        logger.Info($"loaded {Block.DataType} block with block id = {Block.Id}");
                    }
                    else if (Block.Layout.Name == "music_newsfeed_title")
                    {
                        BlocksPanel.Children.Add(new LinksNewsfeedBlockControl() { Links = Block.Links });
                        logger.Info($"loaded {Block.DataType} block with block id = {Block.Id}");
                    }
                    else
                    {
                        BlocksPanel.Children.Add(new LinksBlockControl() { Block = Block });
                        logger.Info($"loaded {Block.DataType} block with block id = {Block.Id}");
                    }


                    return;
                }

                if (Block.DataType == "catalog_banners")
                {

                    if(Block.Banners[0].ClickAction.Action.Url.Contains("subscription")) return;
                    if(Block.Banners[0].ClickAction.Action.Url.Contains("combo")) return;
                    //if (Block.Banners[0].Buttons != null) 

                    BlocksPanel.Children.Add(new BigBannerControl() { Banners = Block.Banners, Margin = new Thickness(0,0,-10,0) });

                    logger.Info($"loaded {Block.DataType} block with block id = {Block.Id}");

                    return;
                }

                if (Block.DataType == "music_audios")
                {
                    if (Block.Layout.Name == "triple_stacked_slider")
                    {
                        BlocksPanel.Children.Add(new ListTracks() { Tracks = Block.Audios });
                      
                        logger.Info($"loaded {Block.DataType} block with block id = {Block.Id}");

                        return;
                    }

                    if (Block.Layout.Name == "music_chart_triple_stacked_slider")
                    {
                        BlocksPanel.Children.Add(new ListTracks() { Tracks = Block.Audios, ShowChart = true });

                        logger.Info($"loaded {Block.DataType} block with block id = {Block.Id}");

                        return;
                    }

                    if (Block.Layout.Name == "list")
                    {

                        BlocksPanel.Children.Add(new AudiosListControl() { Audios = Block.Audios });

                        logger.Info($"loaded {Block.DataType} block with block id = {Block.Id}");

                        return;

                    }

                    if (Block.Layout.Name == "music_chart_list")
                    {

                        BlocksPanel.Children.Add(new AudiosListControl() { Audios = Block.Audios });

                        logger.Info($"loaded {Block.DataType} block with block id = {Block.Id}");

                        return;

                    }

                }

                if (Block.DataType == "action")
                {
                    if (Block.Buttons == null) return;

                    var actionBlocksGrid = new Grid();

                    for (var i = 0; i < Block.Buttons.Count; i++)
                    {
                        var blockButton = Block.Buttons[i];
                    
                        var text = new TextBlock();
                        var card = new CardAction()
                        {
                            Margin = new Thickness(0, 10, 15, 10), 
                            Content = text,
                            DataContext = new BlockButtonViewModel(blockButton, Artist, Block),
                        };
                        
                        card.SetBinding(ButtonBase.CommandProperty, new Binding(nameof(BlockButtonViewModel.InvokeCommand)));
                        card.SetBinding(CardAction.IconProperty, new Binding(nameof(BlockButtonViewModel.Icon)));
                        text.SetBinding(TextBlock.TextProperty, new Binding(nameof(BlockButtonViewModel.Text)));

                        actionBlocksGrid.ColumnDefinitions.Add(new(){ Width = new GridLength(1, GridUnitType.Star) });
                        card.SetValue(Grid.ColumnProperty, i);
                        actionBlocksGrid.Children.Add(card);
                    }

                    BlocksPanel.Children.Add(actionBlocksGrid);
                    
                    logger.Info($"loaded {Block.DataType} block with block id = {Block.Id}");

                    return;

                }

                if (Block.DataType == "none")
                {
                    logger.Info($"loaded {Block.DataType} block with block id = {Block.Id}");

                    if (Block.Layout.Name == "header" || Block.Layout.Name == "header_compact" || Block.Layout.Name == "header_extended")
                    {
                        BlocksPanel.Children.Add(new TitleBlockControl() { Block = Block });
                        return;
                        
                    }

                    if (Block.Layout.Name == "separator")
                    {
                        BlocksPanel.Children.Add(new Rectangle() { Margin = new Thickness(5, 10, 5, 10), Height = 1, Opacity = 0.1, Fill = Brushes.White });
                        logger.Info($"loaded {Block.DataType} block with block id = {Block.Id}");

                        return;
                    }
                }

                if (Block.DataType == "loader")
                {
                    BlocksPanel.Children.Add(new LoaderBlockControl());
                    logger.Info($"loaded {Block.DataType} block ");
                    return;
                }

                if(Block.DataType == "podcast_episodes")
                {
                    BlocksPanel.Children.Add(new PodcastsListBlockControl() { Podcasts = Block.PodcastEpisodes});
                    logger.Info($"loaded {Block.DataType} block ");
                    return;
                }

                if(Block.DataType == "podcast_slider_items")
                {
                    var p = new List<PodcastEpisode>();
                    foreach (var item in Block.PodcastSliderItems) p.Add(item.Episode);
                    BlocksPanel.Children.Add(new PodcastsListBlockControl() { IsSlider = true, Podcasts = p });
                    logger.Info($"loaded {Block.DataType} block ");
                    return;
                }

                if (Block.DataType == "longreads")
                {
                    BlocksPanel.Children.Add(new LongreadsSliderBlockControl() { Longreads = Block.Longreads });
                    logger.Info($"loaded {Block.DataType} block ");
                    return;
                }

                if(Block.DataType == "music_recommended_playlists")
                {
                    BlocksPanel.Children.Add(new RecommendedPlaylistsBlockControl() { Playlists = Block.RecommendedPlaylists});
                    logger.Info($"loaded {Block.DataType} block ");
                    return;
                }

                if(Block.DataType == "videos")
                {
                    if(Block.Layout.Name == "slider")
                    {
                        BlocksPanel.Children.Add(new VideosSliderBlockControl() { Videos = Block.Videos, ShowFull = false });

                        return;
                    }
                    
                    if(Block.Layout.Name == "list")
                    {
                        BlocksPanel.Children.Add(new VideosSliderBlockControl() { Videos = Block.Videos, ShowFull = true });
                        return;
                    }

                }

                if (Block.DataType == "artist_videos")
                {
                    if (Block.Layout.Name == "slider")
                    {
                        BlocksPanel.Children.Add(new VideosSliderBlockControl() { Videos = Block.ArtistVideos, ShowFull = false });

                        return;
                    }

                    if (Block.Layout.Name == "list")
                    {
                        BlocksPanel.Children.Add(new VideosSliderBlockControl() { Videos = Block.ArtistVideos, ShowFull = true });
                        return;
                    }

                }


                NotFoundBlock.Visibility = Visibility.Visible;
                DataTypeName.Text = Block.DataType;
                LayoutName.Text = Block.Layout.Name;
                logger.Info($"loaded NOT FOUND {Block.DataType} block with block id = {Block.Id}");

            }
            catch (Exception ex)
            {

                logger.Error("Fatal error show block content:");
                logger.Error(ex);

                var notificationService = StaticService.Container.Resolve<Services.NotificationsService>();

                notificationService.Show("Произошла ошибка", $"Music X не смог показать блок {Block.DataType}");

            }
        }
    }
}
