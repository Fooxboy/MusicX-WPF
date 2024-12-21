using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;
using MusicX.Core.Models;
using MusicX.Core.Services;
using MusicX.Helpers;
using MusicX.Services;
using MusicX.ViewModels;
using MusicX.Views;
using NLog;
using Wpf.Ui.Controls;

namespace MusicX.Controls
{
    /// <summary>
    /// Логика взаимодействия для LinkControl.xaml
    /// </summary>
    public partial class LinkControl : UserControl
    {
        private readonly VkService vkService;
        private readonly NavigationService navigationService;
        private readonly Logger logger;

        public LinkControl()
        {
            InitializeComponent();
            vkService = StaticService.Container.GetRequiredService<VkService>();
            navigationService = StaticService.Container.GetRequiredService<NavigationService>();
            logger = StaticService.Container.GetRequiredService<Logger>();

        }

        public static readonly DependencyProperty LinkProperty =
          DependencyProperty.Register("Link", typeof(Link), typeof(LinkControl), new PropertyMetadata(new Link()));

        public Link Link
        {
            get { return (Link)GetValue(LinkProperty); }
            set
            {
                SetValue(LinkProperty, value);
            }
        }

        public bool FullLink{ get; set; }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {

                if(Link.Url != null)
                {
                    RectanglePlaceholder.Visibility = Visibility.Collapsed;
                    IconPlaceholder.Visibility = Visibility.Collapsed;
                }

                LinkText.Text = Link.Title;
                if(Link.Meta.ContentType == null)
                {
                    this.FullLink = true;
                    this.Width = 200;
                    this.Height = 80;
                }

                if (Link.Meta.ContentType is "group" or "user" or "chat")
                {
                    if (Link.Image is not (null or { Count: 0 }))
                        LinkImage.ImageSource = new BitmapImage(new(Link.Image[1].Url));

                }else
                {
                    if (Link.Image != null) LinkImage.ImageSource = new BitmapImage(new Uri(Link.Image[0].Url));

                }

                if (FullLink)
                {
                    StackPanelLink.Orientation = Orientation.Horizontal;
                    RectangleLink.Height = 50;
                    RectangleLink.Width = 50;
                    RectanglePlaceholder.Width = 50;
                    RectanglePlaceholder.Height = 50;
                    LinkText.Margin = new Thickness(10, 0, 0, 0);
                    LinkText.VerticalAlignment = VerticalAlignment.Center;

                    if (Link.Meta.ContentType is "group" or "user" or "chat")
                    {
                        Card.Icon = new SymbolIcon(SymbolRegular.Link48);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed load link control");
            }
            
        }

        private async void CardAction_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await navigationService.OpenLinkAsync(Link);
            }
            catch(Exception ex)
            {
                logger.Error(ex, "Failed click action in link control");
            }
           
        }
    }
}
