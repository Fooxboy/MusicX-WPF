using DiscordRPC;
using MusicX.Core.Models;
using NLog;
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

        public DiscordService(Logger logger)
        {
            this.logger = logger;
            client = new DiscordRpcClient("652832654944894976");

            client.Initialize();
        }


        public void SetTrackPlay(string artist, string name, TimeSpan toEnd)
        {
            try
            {
                client.SetPresence(new RichPresence()
                {
                    Details = "Сейчас слушает",
                    State = artist + " - " + name,
                    Assets = new Assets()
                    {
                        LargeImageKey = "album",
                        LargeImageText = "Слушает в MusicX",
                    },
                    Timestamps = new Timestamps()
                    {
                        Start = DateTime.UtcNow,
                        End = DateTime.UtcNow + toEnd,

                    }
                });
            }catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
            }
           
        }


        public void RemoveTrackPlay()
        {
            try
            {
                client.ClearPresence();

            }catch (Exception ex)
            {
                logger.Error(ex, ex.Message);

            }
        }
    }
}
