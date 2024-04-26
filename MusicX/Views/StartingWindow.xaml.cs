using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.AppCenter.Analytics;
using Microsoft.Extensions.DependencyInjection;
using MusicX.Core.Services;
using MusicX.RegistryPatches;
using MusicX.Services;
using MusicX.Services.Player;
using MusicX.Services.Player.Sources;
using MusicX.Services.Player.TrackStats;
using MusicX.Services.Stores;
using MusicX.ViewModels;
using MusicX.ViewModels.Controls;
using MusicX.ViewModels.Login;
using MusicX.ViewModels.Modals;
using MusicX.Views.Login;
using NLog;
using VkNet.Abstractions;
using VkNet.AudioBypassService.Abstractions;
using VkNet.AudioBypassService.Extensions;
using VkNet.AudioBypassService.Models.Auth;
using VkNet.Exception;
using VkNet.Extensions.DependencyInjection;
using Wpf.Ui;
using NavigationService = MusicX.Services.NavigationService;

namespace MusicX.Views
{
    /// <summary>
    /// Логика взаимодействия для StartingWindow.xaml
    /// </summary>
    public partial class StartingWindow
    {
        private readonly string[] _args;

        public StartingWindow(string[] args)
        {
            InitializeComponent();
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

                collection.AddSingleton<IAsyncCaptchaSolver, CaptchaSolverService>();

                collection.AddSingleton<IVkTokenStore, TokenStore>();
                collection.AddSingleton<IDeviceIdStore, DeviceIdStore>();
                collection.AddSingleton<IExchangeTokenStore, ExchangeTokenStore>();

                collection.AddAudioBypass();
                collection.AddVkNet();

                collection.AddSingleton<VkService>();
                collection.AddSingleton<ListenTogetherService>();
                collection.AddSingleton<UserRadioService>();
                collection.AddSingleton<GithubService>();
                collection.AddSingleton<DiscordService>();
                collection.AddSingleton<BoomService>();
                collection.AddSingleton(LogManager.Setup().GetLogger("Common"));
                collection.AddSingleton<GeniusService>();

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
                collection.AddTransient<LyricsViewModel>();
                collection.AddTransient<CaptchaModalViewModel>();
                collection.AddTransient<AccountsWindowViewModel>();
                collection.AddTransient<LoginVerificationMethodsModalViewModel>();

                collection.AddSingleton<NavigationService>();
                collection.AddSingleton<ConfigService>();
                collection.AddSingleton<PlayerService>();
                collection.AddSingleton<DownloaderService>();
                collection.AddSingleton<BannerService>();
                collection.AddTransient<RegistryPatchManager>();
                collection.AddSingleton<WindowsAudioMixerService>();
                collection.AddSingleton<ICustomSectionsService, CustomSectionsService>();
                collection.AddSingleton<ISnackbarService, MusicXSnackbarService>();

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



                var configService = container.GetRequiredService<ConfigService>();
                var patchManager = container.GetRequiredService<RegistryPatchManager>();

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
                            if (string.IsNullOrEmpty(config.AnonToken))
                                await container.GetRequiredService<IVkApiAuthAsync>()
                                    .AuthorizeAsync(new AndroidApiAuthParams());
                            
                            ActivatorUtilities.CreateInstance<AccountsWindow>(container).Show();
                            
                            this.Close();
                        }
                        else
                        {
                            try
                            {

                                await vkService.SetTokenAsync(config.AccessToken);
                                var rootWindow = ActivatorUtilities.CreateInstance<RootWindow>(container);
                                rootWindow.Show();

                                if(_args is { Length: > 0 })
                                {
                                    var arg = _args[0].Split(":");

                                    if (arg[0] == "musicxshare")
                                    {
                                        await rootWindow.StartListenTogether(arg[1]);
                                    }
                                }

                                this.Close();
                            }
                            catch (VkApiMethodInvokeException ex) when (ex.ErrorCode is 5 or 1117)
                            {
                                config.AccessToken = null;
                                config.UserName = null!;
                                config.UserId = 0;
                                config.AccessTokenTtl = default;
                                config.ExchangeToken = null;

                                if (string.IsNullOrEmpty(config.AnonToken))
                                    await container.GetRequiredService<IVkApiAuthAsync>()
                                        .AuthorizeAsync(new AndroidApiAuthParams());
                                
                                await configService.SetConfig(config);
                            
                                ActivatorUtilities.CreateInstance<AccountsWindow>(container).Show();

                                this.Close();
                            }

                        }
                    }
                    catch (HttpRequestException ex)
                    {
                        logger.Error(ex, ex.Message);

                        var error = new NoInternetWindow();

                        error.Show();
                        this.Close();
                    }
                    catch(Exception ex)
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
