using MusicX.Services;
using NLog;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using MusicX.Core.Models;

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
                var logger = StaticService.Container.GetRequiredService<Logger>();

                logger.Error(ex, "Failed to open group {GroupName}", Group?.ScreenName);
            }
        }
    }
}
