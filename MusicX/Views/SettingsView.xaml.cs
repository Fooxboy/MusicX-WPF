using MusicX.Core.Services;
using MusicX.Models;
using MusicX.Services;
using MusicX.Views.Modals;
using NLog;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;
using MusicX.Controls;
using Ookii.Dialogs.Wpf;
using System.Collections.Generic;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using System.Linq;
using Wpf.Ui.Controls;
using MusicX.Helpers;

namespace MusicX.Views
{
    /// <summary>
    /// Логика взаимодействия для SettingsView.xaml
    /// </summary>
    public partial class SettingsView : Page, IMenuPage
    {
        private readonly ConfigService configService;
        private ConfigModel config;
        private readonly VkService vkService;
        public SettingsView()
        {
            InitializeComponent();

            this.vkService = StaticService.Container.GetRequiredService<VkService>();

            this.configService = StaticService.Container.GetRequiredService<ConfigService>();

            this.Loaded += SettingsView_Loaded;
            
        }

        private async void SettingsView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var properties = new Dictionary<string, string>
                {
#if DEBUG
                    { "IsDebug", "True" },
#endif
                    {"Version", StaticService.Version }
                };
                Analytics.TrackEvent("OpenSettings", properties);

                this.config = await configService.GetConfig();

                if (config.ShowRPC == null)
                {
                    config.ShowRPC = true;
                }

                if(config.BroadcastVK == null)
                {
                    config.BroadcastVK = false;
                }

                if(config.WinterTheme == null)
                {
                    config.WinterTheme = false;
                }

                if (config.MinimizeToTray == null)
                {
                    config.MinimizeToTray = false;
                }

                ShowRPC.IsChecked = config.ShowRPC.Value;
                BroacastVK.IsChecked = config.BroadcastVK.Value;
                ShowAmimatedBackground.IsChecked = config.AmimatedBackground;
                WinterTheme.IsChecked = config.WinterTheme.Value;
                MinimizeToTray.IsChecked = config.MinimizeToTray.Value;

                UserName.Text = config.UserName.Split(' ')[0];

                var usr = await vkService.GetCurrentUserAsync();

                if (usr.Photo200 != null) UserImage.ImageSource = new BitmapImage(usr.Photo200);


                var path = $"{AppDomain.CurrentDomain.BaseDirectory}/logs";

                DirectoryInfo di = new DirectoryInfo(path);

                double memory = 0;

                foreach (FileInfo file in di.GetFiles())
                {
                    memory += file.Length / 1024;
                }

                if (memory > 1024)
                {
                    memory /= 1024;
                    MemoryType.Text = "МБ";
                }
                else
                {
                    MemoryType.Text = "КБ";

                }

                memory = Math.Round(memory, 2);
                MemoryLogs.Text = memory.ToString();
                
                if(config.DownloadDirectory != null)
                {
                    di = new DirectoryInfo(config.DownloadDirectory);

                    if (di.Exists)
                    {
                        memory = 0;

                        foreach (FileInfo file in di.GetFiles())
                        {
                            memory += file.Length / 1024;
                        }

                        if (memory > 1024)
                        {
                            memory /= 1024;
                            MemoryTypeTracks.Text = "МБ";
                        }
                        else
                        {
                            MemoryTypeTracks.Text = "КБ";

                        }

                        memory = Math.Round(memory, 2);
                        MemoryTracks.Text = memory.ToString();
                    }
                }
                

                this.VersionApp.Text = StaticService.Version + " " + StaticService.VersionKind;
                this.BuildDate.Text = StaticService.BuildDate;

                if (config.IgnoredArtists is null)
                {
                    config.IgnoredArtists = new List<string>();
                }

                IgnoredArtistList.Items.Clear();

                foreach (var artist in config.IgnoredArtists)
                {
                    IgnoredArtistList.Items.Add(artist);

                }

            }
            catch (Exception ex)
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

        private async void DeleteAccount_Click(object sender, RoutedEventArgs e)
        {
            config.AccessToken = null;
            config.UserName = null;
            config.UserId = 0;

            await configService.SetConfig(config);

            var logger = StaticService.Container.GetRequiredService<Logger>();
            var navigation = StaticService.Container.GetRequiredService<Services.NavigationService>();
            var notifications = StaticService.Container.GetRequiredService<Services.NotificationsService>();

            new LoginWindow(vkService, configService, logger, navigation, notifications).Show();
            Window.GetWindow(this)?.Close();
        }

        private async void CheckUpdates_Click(object sender, RoutedEventArgs e)
        {
            var notifications = StaticService.Container.GetRequiredService<Services.NotificationsService>();

            try
            {
                var navigation = StaticService.Container.GetRequiredService<Services.NavigationService>();
                var github = StaticService.Container.GetRequiredService<GithubService>();

                var release = await github.GetLastRelease();



                if (release.TagName == StaticService.Version)
                {
                    notifications.Show("Уже обновлено!", "У Вас установлена последняя версия MusicX! Обновлений пока что нет");

                }
                else
                {
                    navigation.OpenModal<AvalibleNewUpdateModal>(release);
                }
            }
            catch (Exception ex)
            {
                notifications.Show("Ошибка", "Произошла ошибка при проверке обновлений");

            }

        }

        private void TelegramButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://t.me/MusicXPlayer",
                UseShellExecute = true
            });
        }

        private void OpenLogs_Click(object sender, RoutedEventArgs e)
        {
            var path = $"{AppDomain.CurrentDomain.BaseDirectory}/logs";

            Process.Start(new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true
            });
        }

        private void RemoveLogs_Click(object sender, RoutedEventArgs e)
        {
            var path = $"{AppDomain.CurrentDomain.BaseDirectory}/logs";

            DirectoryInfo di = new DirectoryInfo(path);

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }

            MemoryLogs.Text = "0";
            MemoryType.Text = "КБ";


        }

        private async void ShowRPC_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                config.ShowRPC = true;

                await configService.SetConfig(config);
            }
            catch(Exception ex)
            {
                var properties = new Dictionary<string, string>
                {
                    {"Version", StaticService.Version }
                };
                Crashes.TrackError(ex, properties);

                var logger = StaticService.Container.GetRequiredService<Logger>();
                logger.Error(ex, ex.Message);
            }
           
        }

        private async void ShowRPC_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                config.ShowRPC = false;

                await configService.SetConfig(config);
            }catch(Exception ex)
            {
                var properties = new Dictionary<string, string>
                {
                    {"Version", StaticService.Version }
                };
                Crashes.TrackError(ex, properties);

                var logger = StaticService.Container.GetRequiredService<Logger>();
                logger.Error(ex, ex.Message);
            }
            
        }

        private async void BroacastVK_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                config.BroadcastVK = true;

                await configService.SetConfig(config);
            }catch (Exception ex)
            {
                var properties = new Dictionary<string, string>
                {
                    {"Version", StaticService.Version }
                };
                Crashes.TrackError(ex, properties);

                var logger = StaticService.Container.GetRequiredService<Logger>();
                logger.Error(ex, ex.Message);
            }
            
        }

        private async void BroacastVK_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                config.BroadcastVK = false;

                await configService.SetConfig(config);
                await vkService.SetBroadcastAsync(null);
            }catch (Exception ex)
            {
                var properties = new Dictionary<string, string>
                {
                    {"Version", StaticService.Version }
                };
                Crashes.TrackError(ex, properties);

                var logger = StaticService.Container.GetRequiredService<Logger>();
                logger.Error(ex, ex.Message);
            }
           
        }
        private async void DownloadPath_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaFolderBrowserDialog
            {
                Multiselect = false,
                RootFolder = Environment.SpecialFolder.MyMusic
            };

            if (dialog.ShowDialog() == true)
            {
                config.DownloadDirectory = dialog.SelectedPath;
                await configService.SetConfig(config);
            }
        }
        private void DownloadPathOpen_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(config.DownloadDirectory) || !Directory.Exists(config.DownloadDirectory))
            {
                StaticService.Container.GetRequiredService<NotificationsService>().Show("Ошибка", "Сначала выберите папку");
                return;
            }
            
            Process.Start(new ProcessStartInfo
            {
                FileName = config.DownloadDirectory,
                UseShellExecute = true
            });
        }
        public string MenuTag { get; set; }

        private async void DeleteIgnoredArtist_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (config.IgnoredArtists is null)
                {
                    config.IgnoredArtists = new List<string>();
                }

                var selectedArtistName = (sender as Wpf.Ui.Controls.Button).Tag;
                var selectedArtist = config.IgnoredArtists.SingleOrDefault(x => x == selectedArtistName);

                if (selectedArtist != null)
                {
                    config.IgnoredArtists.Remove(selectedArtist);
                    await configService.SetConfig(config);

                    IgnoredArtistList.Items.Clear();

                    foreach (var artist in config.IgnoredArtists)
                    {
                        IgnoredArtistList.Items.Add(artist);
                    }
                }
            }catch(Exception ex)
            {
                var properties = new Dictionary<string, string>
                {
                    {"Version", StaticService.Version }
                };
                Crashes.TrackError(ex, properties);

                var logger = StaticService.Container.GetRequiredService<Logger>();
                logger.Error(ex, ex.Message);
            }
          
        }

        private async void AddIgnoredArtist_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (config.IgnoredArtists is null)
                {
                    config.IgnoredArtists = new List<string>();
                }

                if (string.IsNullOrEmpty(NameIgnoredArtist.Text))
                {
                    return;
                }

                config.IgnoredArtists.Add(NameIgnoredArtist.Text);

                await configService.SetConfig(config);

                IgnoredArtistList.Items.Clear();

                foreach (var artist in config.IgnoredArtists)
                {
                    IgnoredArtistList.Items.Add(artist);

                }

                NameIgnoredArtist.Text = string.Empty;

            }catch(Exception ex)
            {
                var properties = new Dictionary<string, string>
                {
                    {"Version", StaticService.Version }
                };
                Crashes.TrackError(ex, properties);

                var logger = StaticService.Container.GetRequiredService<Logger>();
                logger.Error(ex, ex.Message);
            }
            
        }

        private async void ShowAmimatedBackground_Checked(object sender, RoutedEventArgs e)
        {
            if (config.AmimatedBackground == (sender as ToggleSwitch).IsChecked)
            {
                return;
            }

            config.AmimatedBackground = true;

            await configService.SetConfig(config);

            if(RootWindow.SnowEngine is null)
            {
                StaticService.Container.GetRequiredService<NotificationsService>().Show("Необходим перезапуск", "Перезапустите Music X чтобы пошел снег :)");

                return;
            }

            RootWindow.SnowEngine.Start();
        }

        private async void ShowAmimatedBackground_Unchecked(object sender, RoutedEventArgs e)
        {

            config.AmimatedBackground = false;

            RootWindow.SnowEngine.Stop();

            await configService.SetConfig(config);
        }

        private async void WinterTheme_Checked(object sender, RoutedEventArgs e)
        {

            if (config.WinterTheme == (sender as ToggleSwitch).IsChecked)
            {
                return;
            }

            config.WinterTheme = true;

            await configService.SetConfig(config);

            StaticService.Container.GetRequiredService<NotificationsService>().Show("Необходим перезапуск", "Перезапустите Music X чтобы началась зима :)");
        }

        private async void WinterTheme_Unchecked(object sender, RoutedEventArgs e)
        {
            config.WinterTheme = false;

            await configService.SetConfig(config);

            StaticService.Container.GetRequiredService<NotificationsService>().Show("Необходим перезапуск", "Перезапустите Music X чтобы зима закончилась :)");

        }

        private async void MinimizeToTray_Checked(object sender, RoutedEventArgs e)
        {
            if (config.MinimizeToTray == (sender as ToggleSwitch).IsChecked)
            {
                return;
            }

            config.MinimizeToTray = true;

            await configService.SetConfig(config);

            StaticService.Container.GetRequiredService<NotificationsService>().Show("Необходим перезапуск", "Перезапустите Music X чтобы изменения применились");

        }

        private async void MinimizeToTray_Unchecked(object sender, RoutedEventArgs e)
        {
            config.MinimizeToTray = false;

            await configService.SetConfig(config);

            StaticService.Container.GetRequiredService<NotificationsService>().Show("Необходим перезапуск", "Перезапустите Music X чтобы изменения применились");
        }
    }
}
