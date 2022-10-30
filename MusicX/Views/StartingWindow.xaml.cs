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
using MusicX.Services.Player.TrackStats;
using VkNet.AudioBypassService.Extensions;
using VkNet.Extensions.DependencyInjection;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using System.Collections.Generic;
using Microsoft.AppCenter.Analytics;
using Microsoft.AspNetCore.SignalR.Client;
using MusicX.ViewModels.Controls;
using MusicX.RegistryPatches;

namespace MusicX.Views
{
    /// <summary>
    /// Логика взаимодействия для StartingWindow.xaml
    /// </summary>
    public partial class StartingWindow : UiWindow
    {
        private readonly string[] _args;

        public StartingWindow(string[] args)
        {
            InitializeComponent();
            Accent.Apply(Accent.GetColorizationColor(), ThemeType.Dark);
            _args = args;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {

            var properties = new Dictionary<string, string>
                {
#if DEBUG
                    { "IsDebug", "True" },
#endif
                    {"Version", StaticService.Version }
                };
            Analytics.TrackEvent("StartApp", properties);

            await Task.Run(async () =>
            {
                var collection = new ServiceCollection();

                collection.AddAudioBypass();
                collection.AddVkNet();

                collection.AddSingleton<VkService>();
                collection.AddSingleton<ListenTogetherService>();
                collection.AddSingleton<GithubService>();
                collection.AddSingleton<DiscordService>();
                collection.AddSingleton<BoomService>();
                collection.AddSingleton<DiscordListenTogetherService>();
                collection.AddSingleton(LogManager.Setup().GetLogger("Common"));

                collection.AddSingleton<IRegistryPatch, ListenTogetherPatch>();

                collection.AddSingleton<ITrackMediaSource, BoomMediaSource>();
                collection.AddSingleton<ITrackMediaSource, VkMediaSource>();

                collection.AddSingleton<ITrackStatsListener, DiscordTrackStats>();
                collection.AddSingleton<ITrackStatsListener, VkTrackBroadcastStats>();
                collection.AddSingleton<ITrackStatsListener, VkTrackStats>();
                collection.AddSingleton<ITrackStatsListener, ListenTogetherStats>();

                collection.AddTransient<SectionViewModel>();
                collection.AddTransient<PlaylistViewModel>();
                collection.AddTransient<PlaylistSelectorModalViewModel>();
                collection.AddTransient<CreatePlaylistModalViewModel>();
                collection.AddTransient<TracksSelectorModalViewModel>();
                collection.AddTransient<TrackNotAvalibleModalViewModel>();
                collection.AddSingleton<DownloaderViewModel>();
                collection.AddSingleton<VKMixViewModel>();
                collection.AddSingleton<BoomProfileViewModel>();
                collection.AddTransient<ListenTogetherControlViewModel>();

                collection.AddSingleton<NavigationService>();
                collection.AddSingleton<ConfigService>();
                collection.AddSingleton<PlayerService>();
                collection.AddSingleton<NotificationsService>();
                collection.AddSingleton<DownloaderService>();
                collection.AddSingleton<BannerService>();
                collection.AddTransient<RegistryPatchManager>();


                
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
                var patchManager = container.GetRequiredService<RegistryPatchManager>();
                var dl = container.GetRequiredService<DiscordListenTogetherService>();
                dl.Init();

                logger.Info("Поиск нужных патчей...");
                await patchManager.Execute();
                logger.Info("Поиск завершен");

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

                                if(_args != null && _args.Length > 0)
                                {
                                    var arg = _args[0].Split(":");

                                    if (arg[0] == "musicxshare")
                                    {
                                        await rootWindow.StartListenTogether(arg[1]);
                                    }
                                }

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
    }
}
