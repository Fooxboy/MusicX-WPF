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

        private bool _listenTogetherEnable = false;
        private string sessionId = string.Empty;

        public DiscordService(Logger logger)
        {
            this.logger = logger;
            client = new DiscordRpcClient("652832654944894976");
            client.RegisterUriScheme(executable: "musicxshare:11111");
            client.Initialize();
        }

        public void SetTrackPlay(string artist, string name, TimeSpan toEnd, string cover)
        {
            try
            {
                DiscordRPC.Button[] buttons = null;

                if(_listenTogetherEnable)
                {
                    buttons = new[] { new DiscordRPC.Button() { Label = "Слушать вместе", Url = $"musicxshare:{sessionId}" } };
                }
                client.SetPresence(new RichPresence()
                {
                    Details = artist,
                    State = name,
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


        public void EnableListenTogether(string sessionId)
        {
            _listenTogetherEnable = true;
            this.sessionId = sessionId;

            var rpc = client.UpdateButtons(new[] { new DiscordRPC.Button() { Label = "Слушать вместе", Url = $"musicxshare:{sessionId}" } });

            SetTrackPlay(rpc.Details, rpc.State, TimeSpan.FromSeconds(30), rpc.Assets.LargeImageKey); //костыль с 30тью секундами...
        }

        public void DisableListenTogether()
        {
            _listenTogetherEnable = false;

            client.UpdateButtons();
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
    }
}
