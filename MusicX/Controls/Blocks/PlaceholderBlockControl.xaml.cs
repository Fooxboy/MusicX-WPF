using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.AppCenter.Crashes;
using Microsoft.Extensions.DependencyInjection;
using MusicX.Core.Models;
using MusicX.Core.Services;
using MusicX.Services;
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
        public PlaceholderBlockControl()
        {
            this.Loaded += PlaceholderBlockControl_Loaded;
            InitializeComponent();
            
        }

        private Button buttonAction;

        private void PlaceholderBlockControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is not Block block)
                return;
            if (block.Placeholders.Count == 0) return;

            var placeholder = block.Placeholders[0];
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

                var snackbarService = StaticService.Container.GetRequiredService<ISnackbarService>();

                snackbarService.Show("Ошибка", "Music X не смог открыть контент");
            }

        }
    }
}
