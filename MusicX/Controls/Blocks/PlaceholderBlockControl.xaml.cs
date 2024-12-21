using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;
using MusicX.Core.Services;
using MusicX.Helpers;
using MusicX.Services;
using MusicX.ViewModels;
using NLog;
using Wpf.Ui;
using Button = MusicX.Core.Models.Button;
using NavigationService = MusicX.Services.NavigationService;

namespace MusicX.Controls.Blocks
{
    /// <summary>
    /// Логика взаимодействия для PlaceholderBlockControl.xaml
    /// </summary>
    public partial class PlaceholderBlockControl : UserControl
    {
        public static readonly DependencyProperty BlockProperty = DependencyProperty.Register(
            nameof(Block), typeof(BlockViewModel), typeof(PlaceholderBlockControl), new PropertyMetadata(BlockChangedCallback));

        public BlockViewModel Block
        {
            get => (BlockViewModel)GetValue(BlockProperty);
            set => SetValue(BlockProperty, value);
        }
        
        public PlaceholderBlockControl()
        {
            InitializeComponent();
        }

        private Button buttonAction;
        
        private static void BlockChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not PlaceholderBlockControl control || e.NewValue is not BlockViewModel)
                return;
            
            control.Load();
        }

        private void Load()
        {
            if (Block.Placeholders.Count == 0) return;

            var placeholder = Block.Placeholders[0];
            if(placeholder.Icons.Count > 0)
            {
                var image = placeholder.Icons.MaxBy(i => i.Width);
                this.Icon.Source = new BitmapImage(new Uri(image.Url));

            }else
            {
                this.Icon.Visibility = Visibility.Collapsed;
            }

            this.Title.Text = placeholder.Title;
            if (placeholder.Title == String.Empty) this.Title.Visibility = Visibility.Collapsed;
            this.Text.Text = placeholder.Text;

            if(placeholder.Buttons.Count > 0)
            {
                var button = placeholder.Buttons.FirstOrDefault();

                this.buttonAction = button;

                ActionButton.Content = button.Title;
            }else
            {
                ActionButton.Visibility = Visibility.Collapsed;
            }
        }

        private async void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(buttonAction.Action.Type == "custom_open_browser")
                {
                    Process.Start(new ProcessStartInfo()
                    {
                        UseShellExecute = true,
                        FileName = buttonAction.Action.Url
                    });

                    return;
                }

                var navigationService = StaticService.Container.GetRequiredService<NavigationService>();
                var vkService = StaticService.Container.GetRequiredService<VkService>();

                var music = await vkService.GetAudioCatalogAsync(buttonAction.Action.Url);
                navigationService.OpenSection(music.Catalog.DefaultSection);
            }
            catch(Exception ex)
            {
                var logger = StaticService.Container.GetRequiredService<Logger>();
                
                logger.Error(ex, "Failed to open placeholder action {Type} {Url}", buttonAction.Action.Type, buttonAction.Action.Url);
                
                var snackbarService = StaticService.Container.GetRequiredService<ISnackbarService>();

                snackbarService.ShowException("Music X не смог открыть контент", ex);
            }

        }
    }
}
