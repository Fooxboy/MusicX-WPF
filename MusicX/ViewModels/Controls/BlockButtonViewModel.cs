using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using MusicX.Core.Models;
using MusicX.Core.Services;
using MusicX.Helpers;
using MusicX.Services;
using MusicX.Services.Player;
using MusicX.Services.Player.Playlists;
using MusicX.ViewModels.Modals;
using MusicX.Views.Modals;
using NLog;
using Wpf.Ui.Common;
using Wpf.Ui.Controls;
using Button = MusicX.Core.Models.Button;

namespace MusicX.ViewModels.Controls;

public class BlockButtonViewModel : BaseViewModel
{
    private Button _action;

    public Button Action
    {
        get => _action; set
        {
            _action = value;
            if (value is not null)
                Refresh();
        }
    }
    public Artist? Artist { get; set; }
    public BlockViewModel? ParentBlock { get; set; }

    public BlockButtonViewModel()
    {
        InvokeCommand = new RelayCommand(Invoke);
        // Refresh();
    }

    public BlockButtonViewModel(Button action, Artist? artist = null, BlockViewModel? parentBlock = null) : this()
    {
        Artist = artist;
        ParentBlock = parentBlock;
        Action = action;
    }

    private void Refresh()
    {
        switch (Action.Action.Type)
        {
            case "toggle_artist_subscription" when Artist is not null && ParentBlock is not null:
                Icon = Artist.IsFollowed ? SymbolRegular.Dismiss24 : SymbolRegular.Checkmark24;
                Text = Artist.IsFollowed ? "Отписаться" : "Подписаться";
                break;
            case "play_shuffled_audios_from_block":
                Icon = SymbolRegular.MusicNote2Play20;
                Text = "Перемешать все";
                break;
            case "create_playlist":
                Icon = SymbolRegular.Add24;
                Text = "Создать плейлист";
                break;
            case "play_audios_from_block":
                Icon = SymbolRegular.Play24;
                Text = "Слушать всё";
                break;
            case "open_section":
                Icon = SymbolRegular.Open24;
                Text = Action.Title ?? "Открыть";
                break;
            case "music_follow_owner":
                Icon = Action.IsFollowing ? SymbolRegular.Checkmark24 : SymbolRegular.Add24;
                Text = Action.IsFollowing ? "Вы подписаны на музыку" : "Подписаться на музыку";
                break;
            case "open_url":
                Icon = SymbolRegular.MusicNote120;
                Text = Action.Title;
                break;
            default:
                Icon = SymbolRegular.AlertOn24;
                Text = "content";
                break;
        }
    }
    
    public SymbolRegular Icon { get; set; }

    public string Text { get; set; } = string.Empty;

    public ICommand InvokeCommand { get; }
    
    private async void Invoke()
    {
        try
        {
            switch (Action.Action.Type)
            {
                case "toggle_artist_subscription" when Artist is not null && ParentBlock is not null:
                {
                    var vkService = StaticService.Container.GetRequiredService<VkService>();

                    if (Artist.IsFollowed)
                        await vkService.UnfollowArtist(Action.ArtistId, ParentBlock.Id);
                    else
                        await vkService.FollowArtist(Action.ArtistId, ParentBlock.Id);

                        Artist.IsFollowed = !Artist.IsFollowed;
                    Refresh();
                    break;
                }
                case "play_shuffled_audios_from_block" or "play_audios_from_block":
                {
                    var vkService = StaticService.Container.GetRequiredService<VkService>();
                    var playerService = StaticService.Container.GetRequiredService<PlayerService>();

                    await playerService.PlayAsync(new VkBlockPlaylist(vkService, Action.BlockId));
                    break;
                }
                case "create_playlist":
                {
                    var vkService = StaticService.Container.GetRequiredService<VkService>();
                    var navigationService = StaticService.Container.GetRequiredService<NavigationService>();
                    var viewModel = StaticService.Container.GetRequiredService<CreatePlaylistModalViewModel>();
                    viewModel.IsEdit = false;
                    
                    if (!string.IsNullOrEmpty(Action.BlockId))
                    {
                        viewModel.Tracks.AddRange(await vkService.LoadFullAudiosAsync(Action.BlockId).ToArrayAsync());
                        viewModel.CreateIsEnable = true;
                    }
                    
                    navigationService.OpenModal<CreatePlaylistModal>(viewModel);
                    break;
                }
                case "open_section":
                {
                    var navigation = StaticService.Container.GetRequiredService<NavigationService>();

                    navigation.OpenSection(Action.SectionId);
                    break;
                }
                case "music_follow_owner":
                {
                    var vkService = StaticService.Container.GetRequiredService<VkService>();

                    if (Action.IsFollowing)
                        await vkService.UnfollowOwner(Action.OwnerId);
                    else
                        await vkService.FollowOwner(Action.OwnerId);

                    Action.IsFollowing = !Action.IsFollowing;
                    Refresh();
                    break;
                }
                case "open_url":
                {
                        Process.Start(new ProcessStartInfo()
                        {
                            UseShellExecute = true,
                            FileName = Action.Action.Url
                        });
                   break;
                }

            }

        }
        catch (Exception e)
        {
            StaticService.Container.GetRequiredService<Logger>().Error(e, "Failed to invoke action {Type}", Action.Action.Type);
        }
    }
}
