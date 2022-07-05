﻿using DryIoc;
using MusicX.Core.Services;
using MusicX.Models;
using MusicX.Services;
using MusicX.Views.Modals;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Windows.Storage;

namespace MusicX.Views
{
    /// <summary>
    /// Логика взаимодействия для SettingsView.xaml
    /// </summary>
    public partial class SettingsView : Page
    {
        private readonly ConfigService configService;
        private ConfigModel config;
        private readonly VkService vkService;
        public SettingsView(ConfigService configService)
        {
            InitializeComponent();

            this.vkService = StaticService.Container.Resolve<VkService>();

            this.configService = configService;

            this.Loaded += SettingsView_Loaded;
            
        }

        private async void SettingsView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.config = await configService.GetConfig();

                if (config.ShowRPC == null)
                {
                    config.ShowRPC = true;
                }

                if(config.BroadcastVK == null)
                {
                    config.BroadcastVK = false;
                }


                ShowRPC.IsChecked = config.ShowRPC.Value;
                BroacastVK.IsChecked = config.BroadcastVK.Value;

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

                this.VersionApp.Text = StaticService.Version + " " + StaticService.VersionKind;
                this.BuildDate.Text = StaticService.BuildDate;
            }catch (Exception ex)
            {
                var logger = StaticService.Container.Resolve<Logger>();
                logger.Error(ex, ex.Message);
            }

        }

        private async void DeleteAccount_Click(object sender, RoutedEventArgs e)
        {
            config.AccessToken = null;
            config.UserName = null;
            config.UserId = 0;

            await configService.SetConfig(config);

            var logger = StaticService.Container.Resolve<Logger>();
            var navigation = StaticService.Container.Resolve<Services.NavigationService>();
            var notifications = StaticService.Container.Resolve<Services.NotificationsService>();

            new LoginWindow(vkService, configService, logger, navigation, notifications).Show();

            navigation.CloseRootWindow();
        }

        private async void CheckUpdates_Click(object sender, RoutedEventArgs e)
        {
            var notifications = StaticService.Container.Resolve<Services.NotificationsService>();

            try
            {
                var navigation = StaticService.Container.Resolve<Services.NavigationService>();
                var github = StaticService.Container.Resolve<GithubService>();

                var release = await github.GetLastRelease();



                if (release.TagName == StaticService.Version)
                {
                    notifications.Show("Уже обновлено!", "У Вас установлена последняя версия MusicX! Обновлений пока что нет");

                }
                else
                {
                    navigation.OpenModal(new AvalibleNewUpdateModal(navigation, release), 350, 450);

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

            config.ShowRPC = true;

            await configService.SetConfig(config);
        }

        private async void ShowRPC_Unchecked(object sender, RoutedEventArgs e)
        {
            config.ShowRPC = false;

            await configService.SetConfig(config);
        }

        private async void BroacastVK_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                config.BroadcastVK = true;

                await configService.SetConfig(config);
            }catch (Exception ex)
            {

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
                
            }
           
        }
    }
}