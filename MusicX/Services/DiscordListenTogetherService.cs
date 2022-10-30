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

            
        }

        public void Init()
        {
            listenTogetherService.StartedSession += ListenTogetherService_StartedSession;
            listenTogetherService.SessionStoped += ListenTogetherService_SessionStoped;
        }

        private async Task ListenTogetherService_SessionStoped()
        {
            discordService.DisableListenTogether();
        }

        private async Task ListenTogetherService_StartedSession(string sessionId)
        {
            discordService.EnableListenTogether(sessionId);
        }
    }
}
