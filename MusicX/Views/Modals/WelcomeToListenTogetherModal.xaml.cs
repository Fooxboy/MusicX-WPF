using Microsoft.Extensions.DependencyInjection;
using MusicX.Services;
using System;
using System.Windows;
using System.Windows.Controls;

namespace MusicX.Views.Modals
{
    /// <summary>
    /// Логика взаимодействия для WelcomeToListenTogetherModal.xaml
    /// </summary>
    public partial class WelcomeToListenTogetherModal : Page
    {
        public WelcomeToListenTogetherModal()
        {
            this.Loaded += WelcomeToListenTogetherModal_Loaded;
            InitializeComponent();
        }

        private async void WelcomeToListenTogetherModal_Loaded(object sender, RoutedEventArgs e)
        {
            var configService = StaticService.Container.GetRequiredService<ConfigService>();

            var config = await configService.GetConfig();

            config.NotifyMessages.ShowListenTogetherModal = false;

            await configService.SetConfig(config);
        }
    }
}
