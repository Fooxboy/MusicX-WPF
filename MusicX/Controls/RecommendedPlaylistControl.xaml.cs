﻿using DryIoc;
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
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MusicX.Helpers;

namespace MusicX.Controls
{
    /// <summary>
    /// Логика взаимодействия для RecommendedPlaylistControl.xaml
    /// </summary>
    public partial class RecommendedPlaylistControl : UserControl
    {
        public static readonly DependencyProperty PlaylistProperty = DependencyProperty.Register(
            nameof(Playlist), typeof(RecommendedPlaylist), typeof(RecommendedPlaylistControl));

        public RecommendedPlaylist Playlist
        {
            get => (RecommendedPlaylist)GetValue(PlaylistProperty);
            set => SetValue(PlaylistProperty, value);
        }
        public RecommendedPlaylistControl()
        {
            InitializeComponent();
            this.Loaded += RecommendedPlaylistControl_Loaded;
        }

        private void RecommendedPlaylistControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                double value;

                try
                {
                    value = Convert.ToDouble(Playlist.Percentage);

                }catch(Exception ex)
                {
                    value = Convert.ToDouble(Playlist.Percentage.Replace('.', ','));
                }


                value = Math.Round(value, 2);
                Percent.Text = $"{value * 100}%";
                Subtitle.Text = Playlist.PercentageTitle;

                NamePlaylist.Text = Playlist.Playlist.Title;
                OwnerName.Text = Playlist.Playlist.OwnerName;
                var brr = (SolidColorBrush)new BrushConverter().ConvertFrom(Playlist.Color);

                var brrTwo = brr.Clone();


                Color color = brrTwo.Color;
                color.R += 50;
                color.B += 20;
                color.G += 72;

                GradientBackground.GradientStops.Add(new GradientStop() { Color = brr.Color, Offset = 0.0 });
                GradientBackground.GradientStops.Add(new GradientStop() { Color = color, Offset = 1.1 });

                //BackgroundRectangle.Fill = 

                foreach (var audio in Playlist.Audios)
                {
                    MainStackPanel.Children.Add(new TrackControl() { Audio = audio, Width = 284, Margin = new Thickness(0, 5, 0, 0) });
                }
                
                var player = StaticService.Container.Resolve<PlayerService>();
                player.CurrentPlaylistChanged += PlayerOnCurrentPlaylistChanged;

                if (player.CurrentPlaylistId == Playlist.Id)
                {
                    nowPlay = true;
                    Icons.Symbol = Wpf.Ui.Common.SymbolRegular.Pause24;
                }
            }catch (Exception ex)
            {
                var logger = StaticService.Container.Resolve<Logger>();

                logger.Error(ex, ex.Message);
            }
            

        }
        private void PlayerOnCurrentPlaylistChanged(object? sender, EventArgs e)
        {
            if (sender is not PlayerService service)
                return;

            if (service.CurrentPlaylistId == Playlist?.Id)
            {
                nowPlay = true;
                Icons.Symbol = Wpf.Ui.Common.SymbolRegular.Pause24;
            }
            else
            {
                Icons.Symbol = Wpf.Ui.Common.SymbolRegular.Play24;
            }
        }

        private void TitleCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var navigationService = StaticService.Container.Resolve<Services.NavigationService>();

            navigationService.NavigateToPage(new PlaylistView(Playlist.Playlist.Id,Playlist.Playlist.OwnerId , Playlist.Playlist.AccessKey));
        }

        private void PlayButton_MouseEnter(object sender, MouseEventArgs e)
        {
            PlayButton.Opacity = 0.5;

        }

        private void PlayButton_MouseLeave(object sender, MouseEventArgs e)
        {
            PlayButton.Opacity = 0.2;
        }


        bool nowLoad = false;
        bool nowPlay = false;
        private async void PlayButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                nowLoad = true;

                var playerService = StaticService.Container.Resolve<PlayerService>();

                if (!nowPlay)
                {
                    nowPlay = true;

                    Icons.Symbol = Wpf.Ui.Common.SymbolRegular.Timer20;
                    var vkService = StaticService.Container.Resolve<VkService>();

                    var audios = await vkService.LoadFullPlaylistAsync(Playlist.Playlist.Id, Playlist.Playlist.OwnerId, Playlist.Playlist.AccessKey);

                    await playerService.Play(0, audios.Items);
                    playerService.CurrentPlaylistId = Playlist.Id;

                    Icons.Symbol = Wpf.Ui.Common.SymbolRegular.Pause24;

                    nowLoad = false;

                }
                else
                {
                    playerService.Pause();
                    Icons.Symbol = Wpf.Ui.Common.SymbolRegular.Play24;

                    await Task.Delay(400);
                    nowLoad = false;

                    nowPlay = false;

                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                nowLoad = false;
            }
        }
    }
}
