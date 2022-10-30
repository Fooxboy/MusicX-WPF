using DiscordRPC;
using MusicX.Core.Models;
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

        public event Func<string, Task> OnJoinRequested;

        public DiscordService(Logger logger)
        {
            this.logger = logger;
            client = new DiscordRpcClient("652832654944894976", autoEvents: true);
            client.OnJoinRequested += Client_OnJoinRequested;
            client.OnJoin += Client_OnJoin;
            client.OnError += Client_OnError;
            client.RegisterUriScheme();
            client.Initialize();
        }

        private void Client_OnError(object sender, DiscordRPC.Message.ErrorMessage args)
        {
            throw new NotImplementedException();
        }

        private void Client_OnJoin(object sender, DiscordRPC.Message.JoinMessage args)
        {
            throw new NotImplementedException();
        }

        private void Client_OnJoinRequested(object sender, DiscordRPC.Message.JoinRequestMessage args)
        {
            OnJoinRequested?.Invoke(args.User.Username);
        }

        public void SetTrackPlay(string artist, string name, TimeSpan toEnd, string cover)
        {
            try
            {
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
                    Party = new Party() { ID = "w823894923423", Max = 10, Privacy = Party.PrivacySetting.Public, Size = 1},
                    Secrets = new Secrets() { JoinSecret = "gfdgdfsgdgsdfg" }
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
    }
}
