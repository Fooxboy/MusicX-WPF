using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.AppCenter.Crashes;
using Microsoft.Extensions.DependencyInjection;
using MusicX.Core.Models;
using MusicX.Core.Services;
using MusicX.Services;
using NLog;
using Wpf.Ui.Controls;

namespace MusicX.Controls.Blocks
{
    /// <summary>
    /// Логика взаимодействия для CuratorBannerBlockControl.xaml
    /// </summary>
    public partial class CuratorBannerBlockControl : UserControl
    {
        public static readonly DependencyProperty CuratorsProperty = DependencyProperty.Register(
            nameof(Curators), typeof(IList<Curator>), typeof(CuratorBannerBlockControl), new PropertyMetadata(Array.Empty<Curator>()));

        public IList<Curator> Curators
        {
            get => (IList<Curator>)GetValue(CuratorsProperty);
            set => SetValue(CuratorsProperty, value);
        }
        
        public CuratorBannerBlockControl()
        {
            InitializeComponent();
        }

        private async void ActionCuratorButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var vkService = StaticService.Container.GetRequiredService<VkService>();
                if (Curators[0].IsFollowed)
                {
                    ActionCuratorButton.IsEnabled = false;
                    ActionCuratorButton.Content = "Секунду..";
                    ActionCuratorButton.Icon = new SymbolIcon(SymbolRegular.Timer28);

                    await vkService.UnfollowCurator(Curators[0].Id);

                    ActionCuratorButton.IsEnabled = true;

                    Curators[0].IsFollowed = false;
                    ActionCuratorButton.Content = "Подписаться";
                    ActionCuratorButton.Icon = new SymbolIcon(SymbolRegular.Add24);
                }
                else
                {
                    ActionCuratorButton.IsEnabled = false;
                    ActionCuratorButton.Content = "Секунду..";
                    ActionCuratorButton.Icon = new SymbolIcon(SymbolRegular.Timer28);

                    await vkService.FollowCurator(Curators[0].Id);

                    ActionCuratorButton.IsEnabled = true;
                    Curators[0].IsFollowed = true;
                    ActionCuratorButton.Content = "Отписаться";
                    ActionCuratorButton.Icon = new SymbolIcon(SymbolRegular.DeleteDismiss20);
                }
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

                var logger = StaticService.Container.GetRequiredService<Logger>();

                logger.Error(ex, ex.Message);

            }

        }

        private void CuratorBannerBlockControl_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is not Core.Models.Block)
                return;
            
            CuratorBannerImage.ImageSource = new BitmapImage(new Uri(Curators[0].Photo[2].Url));
            CuratorText.Text = Curators[0].Name;
            //CuratorDescription.Text = Block.Curators[0].Description;

            if (Curators[0].IsFollowed)
            {
                ActionCuratorButton.Content = "Отписаться";
                ActionCuratorButton.Icon = new SymbolIcon(SymbolRegular.DeleteDismiss20);
            }
            else
            {

                ActionCuratorButton.Content = "Подписаться";
                ActionCuratorButton.Icon = new SymbolIcon(SymbolRegular.Add24);
            }
        }
    }
}
