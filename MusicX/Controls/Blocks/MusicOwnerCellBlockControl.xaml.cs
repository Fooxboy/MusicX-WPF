using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using MusicX.Core.Models;
using MusicX.Services;
using NLog;

namespace MusicX.Controls.Blocks;

public partial class MusicOwnerCellBlockControl : UserControl
{
    public MusicOwnerCellBlockControl()
    {
        InitializeComponent();
    }

    private void OwnerCard_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = ((MusicOwner)DataContext).Url,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            var logger = StaticService.Container.GetRequiredService<Logger>();

            logger.Error(ex, "Failed to open music owner {Url}", ((MusicOwner?)DataContext)?.Url);
        }
    }
}