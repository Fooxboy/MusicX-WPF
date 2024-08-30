﻿using MusicX.Core.Models;
using MusicX.Services;
using NLog;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using Microsoft.AppCenter.Crashes;
using MusicX.ViewModels;

namespace MusicX.Controls.Blocks
{
    /// <summary>
    /// Логика взаимодействия для GroupsBlockControl.xaml
    /// </summary>
    public partial class GroupsBlockControl : UserControl
    {
        public static readonly DependencyProperty GroupProperty = DependencyProperty.Register(
            nameof(Group), typeof(Group), typeof(GroupsBlockControl), new PropertyMetadata(default(Group)));

        public Group Group
        {
            get => (Group)GetValue(GroupProperty);
            set => SetValue(GroupProperty, value);
        }
        
        public GroupsBlockControl()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://vk.com/" + Group.ScreenName,
                    UseShellExecute = true
                });
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
    }
}
