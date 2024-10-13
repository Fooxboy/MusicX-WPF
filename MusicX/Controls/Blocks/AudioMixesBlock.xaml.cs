using Microsoft.Extensions.DependencyInjection;
using MusicX.Core.Services;
using MusicX.Models.Enums;
using MusicX.Services;
using MusicX.Services.Player;
using MusicX.Services.Player.Playlists;
using MusicX.ViewModels.Modals;
using MusicX.Views.Modals;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using AsyncAwaitBestPractices.MVVM;

namespace MusicX.Controls.Blocks;

public sealed partial class AudioMixesBlock : UserControl
{

    public MixMode Mode
    {
        get => (MixMode)GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    public static readonly DependencyProperty ModeProperty =
        DependencyProperty.Register(nameof(Mode), typeof(MixMode), typeof(AudioMixesBlock), new PropertyMetadata(ModePropertyChangedCallback));

    private static void ModePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AudioMixesBlock block)
            block.CurrentPlaylistChanged(null, EventArgs.Empty);
    }

    public bool IsPlaying
    {
        get => (bool)GetValue(IsPlayingProperty);
        set => SetValue(IsPlayingProperty, value);
    }

    public static readonly DependencyProperty IsPlayingProperty =
        DependencyProperty.Register(nameof(IsPlaying), typeof(bool), typeof(AudioMixesBlock));
    
    private readonly PlayerService _player;
    private ImmutableDictionary<string, ImmutableArray<string>>? _options;

    private string MixId => Mode switch
    {
        MixMode.Mix => "common",
        MixMode.Library => "my_music",
        _ => throw new ArgumentOutOfRangeException()
    };

    public AudioMixesBlock()
    {
        InitializeComponent();

        Mode = MixMode.Mix;

        _player = StaticService.Container.GetRequiredService<PlayerService>();

        _player.CurrentPlaylistChanged += CurrentPlaylistChanged;
        _player.PlayStateChangedEvent += CurrentPlaylistChanged;
        CurrentPlaylistChanged(_player, EventArgs.Empty);
    }

    private void CurrentPlaylistChanged(object? sender, EventArgs e)
    {
        IsPlaying = _player is { CurrentPlaylist: MixPlaylist mixPlaylist, IsPlaying: true } && mixPlaylist.Data.Id == MixId;
    }

    private async void Button_Click(object sender, RoutedEventArgs e)
    {
        if (IsPlaying)
        {
            _player.Pause();
            return;
        }

        if (_player.CurrentPlaylist is MixPlaylist mixPlaylist && mixPlaylist.Data.Id == MixId)
        {
            _player.Play();
            return;
        }

        await PlayPlaylist();
    }

    private Task PlayPlaylist()
    {
        var data = new MixOptions(MixId, Options: _options);

        return _player.PlayAsync(new MixPlaylist(data, StaticService.Container.GetRequiredService<VkService>()));
    }

    private void LibraryButton_Click(object sender, RoutedEventArgs e)
    {
        Mode = MixMode.Library;

        MixButton.Appearance = Wpf.Ui.Controls.ControlAppearance.Transparent;
        MixSettings.Visibility = Visibility.Hidden;
        LibraryButton.Appearance = Wpf.Ui.Controls.ControlAppearance.Secondary;
    }

    private void MixButton_Click(object sender, RoutedEventArgs e)
    {
        Mode = MixMode.Mix;

        MixButton.Appearance = Wpf.Ui.Controls.ControlAppearance.Secondary;
        MixSettings.Visibility = Visibility.Visible;
        LibraryButton.Appearance = Wpf.Ui.Controls.ControlAppearance.Transparent;
    }

    private async void MixSettings_Click(object sender, RoutedEventArgs e)
    {
        var navigationService = StaticService.Container.GetRequiredService<NavigationService>();
        var vm = StaticService.Container.GetRequiredService<MixSettingsModalViewModel>();

        vm.ApplyCommand = new AsyncCommand(async () =>
        {
            SetOptions(vm.Categories);
            navigationService.CloseModal();
            
            await PlayPlaylist();
        });
        vm.ResetCommand = new AsyncCommand(async () =>
        {
            _options = null;
            navigationService.CloseModal();
            
            await PlayPlaylist();
        });

        await vm.LoadSettings(MixId);

        navigationService.OpenModal<MixSettingsModal>(vm);
    }

    private void SetOptions(IEnumerable<MixSettingsCategoryViewModel> categories)
    {
        var builder = ImmutableDictionary<string, ImmutableArray<string>>.Empty.ToBuilder();
        
        foreach (var category in categories)
        {
            foreach (var option in category.Options.Where(b => b.Selected))
            {
                builder[category.Id] = builder.TryGetValue(category.Id, out var value)
                    ? value.Add(option.Id)
                    : [option.Id];
            }
        }

        _options = builder.Count == 0 ? null : builder.ToImmutable();
    }
}
