using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MusicX.Core.Models;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MusicX.Core.Services;
using MusicX.Services;
using DryIoc;

namespace MusicX.Controls.Blocks
{
    /// <summary>
    /// Логика взаимодействия для PlaceholderBlockControl.xaml
    /// </summary>
    public partial class PlaceholderBlockControl : UserControl
    {
        private readonly Block block;
        public PlaceholderBlockControl(Block block)
        {
            this.block = block;
            this.Loaded += PlaceholderBlockControl_Loaded;
            InitializeComponent();
            
        }

        private Core.Models.Button buttonAction;

        private void PlaceholderBlockControl_Loaded(object sender, RoutedEventArgs e)
        {
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
                var button = placeholder.Buttons.SingleOrDefault(b=> b.Action.Type == "open_url");

                this.buttonAction = button;

                ActionButton.Content = button.Title;
            }else
            {
                ActionButton.Visibility = Visibility.Collapsed;
            }
        }

        private async void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            var navigationService = StaticService.Container.Resolve<Services.NavigationService>();
            var vkService = StaticService.Container.Resolve<VkService>();

            var music = await vkService.GetAudioCatalogAsync(buttonAction.Action.Url);
            navigationService.OpenSection(music.Catalog.DefaultSection);
        }
    }
}
