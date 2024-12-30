using MusicX.Models;
using System;
using System.IO;
using System.IO.Pipes;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NLog;

namespace MusicX.Services
{
    public class SingleAppService
    {
        public event Func<string[], Task>? RunWitchArgs;
        
        public event Action? Focus;

        private static SingleAppService? _instance;

        public static SingleAppService Instance => _instance ??= new SingleAppService();


        public async void StartArgsListener()
        {
            try
            {
                while (true)
                {
                    await using var pipeServer = new NamedPipeServerStream("argsServer");
                    await pipeServer.WaitForConnectionAsync();
                    
                    var command = await JsonSerializer.DeserializeAsync<AppCommand>(pipeServer);

                    switch (command)
                    {
                        case ArgsCommand { Args: var args }:
                            if (RunWitchArgs is not null)
                                await RunWitchArgs(args);
                            break;
                        case FocusCommand focusCommand:
                            Focus?.Invoke();
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(command));
                    }
                }
            }
            catch (Exception e)
            {
                StaticService.Container.GetRequiredService<Logger>().Error(e, "Failed to start args listener");
            }
        }

        public async Task SendArguments(string[] args)
        {
            await SendCommand(new ArgsCommand(args));
        }
        
        public async Task FocusWindow()
        {
            await SendCommand(new FocusCommand());
        }

        private async Task SendCommand<T>(T command) where T : AppCommand
        {
            await using var pipeClient = new NamedPipeClientStream(".", "argsServer");
            await pipeClient.ConnectAsync();

            await JsonSerializer.SerializeAsync<AppCommand>(pipeClient, command);
        }

        [JsonDerivedType(typeof(ArgsCommand), "args")]
        [JsonDerivedType(typeof(FocusCommand), "focus")]
        private abstract record AppCommand;
        
        private record ArgsCommand(string[] Args) : AppCommand;
        
        private record FocusCommand : AppCommand;
    }
}
