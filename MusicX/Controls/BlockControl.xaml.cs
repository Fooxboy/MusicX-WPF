using DryIoc;
using MusicX.Controls.Blocks;
using MusicX.Core.Models;
using MusicX.Core.Services;
using MusicX.Services;
using MusicX.Views;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WPFUI.Controls;

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

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var logger = StaticService.Container.Resolve<Logger>();

            try
            {
                if (Block.DataType == "artist")
                {
                    BlocksPanel.Children.Add(new ArtistBannerBlockControl() { Block = Block });
                  
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
                        BlocksPanel.Children.Add(new ListPlaylists() { Playlists = Block.Playlists, ShowFull = false });
                        logger.Info($"loaded {Block.DataType} block with block id = {Block.Id}");

                        return;
                    }

                    if (Block.Layout.Name == "large_slider")
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
                }

                if (Block.DataType == "search_suggestions")
                {
                    
                    BlocksPanel.Children.Add(new SearchSuggestionsBlockControl() { Block = Block });
                    logger.Info($"loaded {Block.DataType} block with block id = {Block.Id}");
                    
                    return;
                }

                if (Block.DataType == "links")
                {
                    BlocksPanel.Children.Add(new LinksBlockControl() { Block = Block });
                    logger.Info($"loaded {Block.DataType} block with block id = {Block.Id}");

                    return;
                }

                if (Block.DataType == "catalog_banners")
                {
                    if (Block.Banners[0].Buttons != null) return;

                    BlocksPanel.Children.Add(new ListBanners() { Banners = Block.Banners });

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

                }

                if (Block.DataType == "action")
                {
                    var card = new CardAction() { Margin = new Thickness(0, 10, 15,10), Icon = WPFUI.Common.Icon.AlertOn24 };
                    card.Click += CardAction_Click;
                    var text = new TextBlock() { Text = "content" };

                    if (Block.Buttons == null) return;
                    if (Block.Buttons[0].Action.Type == "play_shuffled_audios_from_block")
                    {
                        card.Icon = WPFUI.Common.Icon.MusicNote2Play20;
                        text.Text = "Перемешать все";
                    }

                    if (Block.Buttons[0].Action.Type == "create_playlist")
                    {
                        card.Icon = WPFUI.Common.Icon.Add24;
                        text.Text = "Создать плейлист";
                    }

                    card.Content = text;

                    BlocksPanel.Children.Add(card);
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


                NotFoundBlock.Visibility = Visibility.Visible;
                DataTypeName.Text = Block.DataType;
                LayoutName.Text = Block.Layout.Name;
                logger.Info($"loaded NOT FOUND {Block.DataType} block with block id = {Block.Id}");

            }
            catch (Exception ex)
            {
                logger.Error("Fatal error show block content:");
                logger.Error(ex);
            }
        }

        private async void CardAction_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                var action = Block.Buttons[0].Action;


                if (Block.Buttons[0].Action.Type == "play_shuffled_audios_from_block")
                {
                    var vkService = StaticService.Container.Resolve<VkService>();
                    var playerService = StaticService.Container.Resolve<PlayerService>();

                    var res = await vkService.GetBlockItemsAsync(Block.Buttons[0].BlockId);

                    await playerService.Play(0, res.Audios);

                }

                if (Block.Buttons[0].Action.Type == "create_playlist")
                {

                }
            }catch(Exception ex)
            {
                var logger = StaticService.Container.Resolve<Logger>();

                logger.Error(ex, ex.Message);
            }
        }
    }
}
