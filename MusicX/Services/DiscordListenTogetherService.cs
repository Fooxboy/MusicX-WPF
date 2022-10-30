using MusicX.Core.Services;
using MusicX.Views.Modals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicX.Services
{
    public class DiscordListenTogetherService
    {
        private readonly DiscordService discordService;
        private readonly ListenTogetherService listenTogetherService;
        private readonly NavigationService navigationService;

        public DiscordListenTogetherService(DiscordService discordService, ListenTogetherService listenTogetherService, NavigationService navigationService)
        {
            this.discordService = discordService;
            this.listenTogetherService = listenTogetherService;
            this.navigationService = navigationService;


            discordService.OnJoinRequested += DiscordService_OnJoinRequested;
        }

        private async Task DiscordService_OnJoinRequested(string arg)
        {
            navigationService.OpenModal<RequestListenTogetherModal>(new RequestListenTogetherModal());
        }
    }
}
