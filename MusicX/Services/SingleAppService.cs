using MusicX.Models;
using System;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VkNet.Abstractions.Authorization;

namespace MusicX.Services
{
    public class SingleAppService
    {
        public event Func<string[], Task> RunWitchArgs;

        private static SingleAppService _instance;

        public static SingleAppService Instance
        {
            get { return _instance ?? (_instance = new SingleAppService()); }
        }


        public async Task StartArgsListener()
        {
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    var pipeServer = new NamedPipeServerStream("argsServer");

                    pipeServer.WaitForConnection();

                    var textStream = new StreamString(pipeServer);

                    var args = textStream.ReadString();

                    RunWitchArgs?.Invoke(args.Split('-'));

                    pipeServer.Close();
                }
            });
            
        }

        public async Task SendArguments(string[] args)
        {
            var pipeClient = new NamedPipeClientStream(".", "argsServer");
            pipeClient.Connect();

            var textString = new StreamString(pipeClient);

            textString.WriteString(String.Join('-', args));

            pipeClient.Close();

        }
    }
}
