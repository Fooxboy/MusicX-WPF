using System;
using System.Windows.Input;
using DryIoc;
using MusicX.Core.Models;
using MusicX.Core.Services;
using MusicX.Services;
using MusicX.ViewModels.Modals;
using MusicX.Views.Modals;
using NLog;
using WPFUI.Common;
namespace MusicX.ViewModels.Controls;

public class BlockButtonViewModel : BaseViewModel
{
    public BlockButtonViewModel(Button action)
    {
        Action = action;
        InvokeCommand = new RelayCommand(Invoke);
    }

    public Button Action { get; }

    public ICommand InvokeCommand { get; }
    
    private async void Invoke()
    {
        try
        {
            switch (Action.Action.Type)
            {
                case "play_shuffled_audios_from_block" or "play_audios_from_block":
                {
                    var vkService = StaticService.Container.Resolve<VkService>();
                    var playerService = StaticService.Container.Resolve<PlayerService>();

                    var res = await vkService.GetBlockItemsAsync(Action.BlockId);

                    await playerService.PlayTrack(res.Audios[0]);
                    break;
                }
                case "create_playlist":
                {
                    var notificationService = StaticService.Container.Resolve<NotificationsService>();
                    var navigationService = StaticService.Container.Resolve<NavigationService>();
                    var viewModel = StaticService.Container.Resolve<CreatePlaylistModalViewModel>();
                    viewModel.IsEdit = false;
                    navigationService.OpenModal(new CreatePlaylistModal(viewModel), 700, 600);
                    break;
                }
                case "open_section":
                {
                    var navigation = StaticService.Container.Resolve<NavigationService>();

                    await navigation.OpenSection(Action.SectionId, true);
                    break;
                }
            }

        }
        catch (Exception e)
        {
            StaticService.Container.Resolve<Logger>().Error(e, "Failed to invoke action {Type}", Action.Action.Type);
        }
    }
}
