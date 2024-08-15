using System;
using System.Collections.Generic;
using System.Windows;
using MusicX.Models;
using Wpf.Ui.Appearance;

namespace MusicX.Services;

public class WindowThemeService(ConfigService configService)
{
    private readonly List<Window> _openedWindows = [];
    private bool Enabled => configService.Config.Theme == MusicXTheme.Default;

    public void Register(Window window)
    {
        if (Enabled)
            SystemThemeWatcher.Watch(window);
        
        _openedWindows.Add(window);
    }
    
    public void Unregister(Window window)
    {
        SystemThemeWatcher.UnWatch(window);
        _openedWindows.Remove(window);
    }
    
    public void Update(MusicXTheme previousTheme)
    {
        foreach (var window in _openedWindows)
        {
            if (Enabled && previousTheme != MusicXTheme.Default)
                SystemThemeWatcher.Watch(window);
            else
                SystemThemeWatcher.UnWatch(window);
        }

        if (ApplicationThemeManager.GetAppTheme() == configService.Config.Theme switch
            {
                MusicXTheme.Default => ApplicationThemeManager.GetSystemTheme() switch
                {
                    SystemTheme.Dark or SystemTheme.CapturedMotion or SystemTheme.Glow => ApplicationTheme.Dark,
                    SystemTheme.Light or SystemTheme.Flow or SystemTheme.Sunrise => ApplicationTheme.Light,
                    _ => ApplicationTheme.Unknown
                },
                MusicXTheme.Light => ApplicationTheme.Light,
                MusicXTheme.Dark => ApplicationTheme.Dark,
                _ => throw new IndexOutOfRangeException()
            })
            return;
        
        if (configService.Config.Theme == MusicXTheme.Default)
            ApplicationThemeManager.ApplySystemTheme();
        else
            ApplicationThemeManager.Apply(configService.Config.Theme == MusicXTheme.Dark
                ? ApplicationTheme.Dark
                : ApplicationTheme.Light);
    }
}