using MusicX.Core.Services;
using MusicX.Services;
using MusicX.ViewModels;
using MusicX.ViewModels.Modals;
using NLog;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using MusicX.Services.Player;
using MusicX.Services.Player.Sources;
using VkNet.AudioBypassService.Extensions;
using VkNet.Extensions.DependencyInjection;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace MusicX.Views
{
    /// <summary>
    /// Логика взаимодействия для StartingWindow.xaml
    /// </summary>
    public partial class StartingWindow : UiWindow
    {
        public StartingWindow()
        {
            InitializeComponent();
           // this.Background = Brushes.Black;
           Accent.Apply(Accent.GetColorizationColor(), ThemeType.Dark);
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {

            await Task.Run(async () =>
            {
                var collection = new ServiceCollection();

                collection.AddAudioBypass();
                collection.AddVkNet();

                collection.AddSingleton<VkService>();
                collection.AddSingleton<ServerService>();
                collection.AddSingleton<GithubService>();
                collection.AddSingleton<DiscordService>();
                collection.AddSingleton<BoomService>();
                collection.AddSingleton(LogManager.Setup().GetLogger("Common"));

                collection.AddSingleton<ITrackMediaSource, BoomMediaSource>();
                collection.AddSingleton<ITrackMediaSource, VkMediaSource>();

                collection.AddTransient<SectionViewModel>();
                collection.AddTransient<PlaylistViewModel>();
                collection.AddTransient<PlaylistSelectorModalViewModel>();
                collection.AddTransient<CreatePlaylistModalViewModel>();
                collection.AddTransient<TracksSelectorModalViewModel>();
                collection.AddSingleton<DownloaderViewModel>();
                collection.AddSingleton<VKMixViewModel>();

                collection.AddSingleton<NavigationService>();
                collection.AddSingleton<ConfigService>();
                collection.AddSingleton<PlayerService>();
                collection.AddSingleton<NotificationsService>();
                collection.AddSingleton<DownloaderService>();
                collection.AddSingleton<BannerService>();
                
                var container = StaticService.Container = collection.BuildServiceProvider();

                var vkService = container.GetRequiredService<VkService>();
                var logger = container.GetRequiredService<Logger>();
                logger.Info("");
                logger.Info("");
                logger.Info("");
                logger.Info("");

                logger.Info("=====================================================================================");
                logger.Info("Starting MusicX App...");
                logger.Info("=====================================================================================");



                var navigationService = container.GetRequiredService<NavigationService>();
                var configService = container.GetRequiredService<ConfigService>();
                var notificationsService = container.GetRequiredService<NotificationsService>();

                var config = await configService.GetConfig();

                logger.Info("Check new updater");
                if(File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\MusicX.UpdaterNew.exe"))
                {
                    logger.Info("Check old updater");

                    if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\MusicX.Updater.exe"))
                    {
                        logger.Info("Delete old updater");

                        File.Delete(AppDomain.CurrentDomain.BaseDirectory + "\\MusicX.Updater.exe");
                    }

                    logger.Info("Rename new updater");

                    File.Move(AppDomain.CurrentDomain.BaseDirectory + "\\MusicX.UpdaterNew.exe", AppDomain.CurrentDomain.BaseDirectory + "\\MusicX.Updater.exe");
                }

                
                await Application.Current.Dispatcher.BeginInvoke(async () =>
                {
                    try
                    {
                        if (string.IsNullOrEmpty(config.AccessToken))
                        {
                            var login = new LoginWindow(vkService, configService, logger, navigationService, notificationsService);
                            login.Show();
                            this.Close();
                        }
                        else
                        {
                            try
                            {

                                await vkService.SetTokenAsync(config.AccessToken);
                                var rootWindow = new RootWindow(navigationService, vkService, logger, configService, notificationsService);
                                rootWindow.Show();
                                this.Close();
                            }
                            catch (VkNet.Exception.VkApiMethodInvokeException e) when (e.ErrorCode is 5 or 1117)
                            {
                                config.AccessToken = null;
                                config.UserName = null;
                                config.UserId = 0;

                                await configService.SetConfig(config);

                                var logger = StaticService.Container.GetRequiredService<Logger>();
                                var navigation = StaticService.Container.GetRequiredService<Services.NavigationService>();
                                var notifications = StaticService.Container.GetRequiredService<Services.NotificationsService>();

                                new LoginWindow(vkService, configService, logger, navigation, notifications, true).Show();

                                this.Close();
                            }

                        }
                    }catch(Exception ex)
                    {
                        logger.Error(ex, ex.Message);

                        var error = new FatalErrorView(ex);

                        error.Show();
                        this.Close();
                    }
                    
                });

                
            });
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            


        }
    }
}
