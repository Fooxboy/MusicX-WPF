using DiscordRPC;
using MusicX.Core.Models;
using MusicX.Shared.ListenTogether;
using NLog;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicX.Core.Services
{
    public class DiscordService
    {
        private readonly DiscordRpcClient client;
        private readonly Logger logger;
        private readonly ListenTogetherService _listenTogetherService;

        private bool _listenTogetherEnable = false;

        public DiscordService(Logger logger, ListenTogetherService listenTogetherService)
        {
            this.logger = logger;
            _listenTogetherService = listenTogetherService;
            client = new DiscordRpcClient("652832654944894976");
            client.Initialize();

            listenTogetherService.StartedSession += ListenTogetherServiceOnStartedSession;
            listenTogetherService.SessionStoped += ListenTogetherServiceOnSessionStoped;
        }

        private Task ListenTogetherServiceOnSessionStoped()
        {
            _listenTogetherEnable = false;
            Update();
            return Task.CompletedTask;
        }

        private Task ListenTogetherServiceOnStartedSession(string sessionId)
        {
            _listenTogetherEnable = true;
            Update();
            return Task.CompletedTask;
        }

        private void Update()
        {
            try
            {
                var rpc = client.CurrentPresence;
                SetTrackPlay(rpc.Details, rpc.State, rpc.Timestamps.End!.Value - DateTime.UtcNow, rpc.Assets.LargeImageKey);

            }catch(Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
          
        }

        public void SetTrackPlay(string artist, string name, TimeSpan toEnd, string cover)
        {
            try
            {
                DiscordRPC.Button[] buttons = null;

                if(_listenTogetherEnable)
                {
                    buttons = new[]
                    {
                        new DiscordRPC.Button
                        {
                            Label = "Слушать вместе",
                            Url = _listenTogetherService.ConnectUrl
                        }
                    };
                }
                client.SetPresence(new RichPresence()
                {
                    Details = TruncateString(artist),
                    State = TruncateString(name),
                    Assets = new Assets()
                    {
                        LargeImageKey = cover,
                        LargeImageText = "Слушает в MusicX",
                    },
                    Timestamps = new Timestamps()
                    {
                        Start = DateTime.UtcNow,
                        End = DateTime.UtcNow + toEnd,
                    },
                    Buttons = buttons
                }) ;
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
           
        }

        public void RemoveTrackPlay(bool pause = false)
        {
            try
            {
                if(pause)
                {
                    client.SetPresence(new RichPresence()
                    {
                        Details = "Трек на паузе",
                        State = ""
                    });
                }else
                {
                    client.ClearPresence();
                }
                

            }catch (Exception ex)
            {
                logger.Error(ex, ex.Message);

            }
        }

        private string TruncateString(string input)
        {
            if (input.Length <= 128)
            {
                return input;
            }

            return input.Substring(0, 128 - 3) + "...";
        }
    }
}