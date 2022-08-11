using System;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows.Input;
using AsyncAwaitBestPractices.MVVM;
using DryIoc;
using MusicX.Core.Services;
using MusicX.Services;
using VkNet.AudioBypassService;
using VkNet.AudioBypassService.Abstractions;
using VkNet.Model;
namespace MusicX.ViewModels.Login;

public class LoginViewModel : BaseViewModel
{
    private readonly IBypassAuthorizationFlow _authFlow;
    public ulong PhoneNumber { get; set; }
    public int CountryCode { get; set; } = 7;
    public bool ForceSms { get; set; } = true;
    
    public ICommand SubmitPhoneCommand { get; }
    
    public ChannelReader<(string Header, string Content)> Notifications { get; }

    public event EventHandler? AuthorizationCompleted;

    private readonly ChannelWriter<(string Header, string Content)> _notificationsWriter;
    private readonly VkService _vkService;

    public LoginViewModel()
    {
        _vkService = StaticService.Container.Resolve<VkService>();
        _authFlow = (IBypassAuthorizationFlow)_vkService.vkApi.AuthorizationFlow;
        SubmitPhoneCommand = new AsyncCommand(SubmitPhone);

        var channel = Channel.CreateUnbounded<(string Header, string Content)>();
        Notifications = channel;
        _notificationsWriter = channel;
    }

    private async Task SubmitPhone()
    {
        if (PhoneNumber.ToString().Length != 10)
            return;
        
        _authFlow.SetAuthorizationParams(new ApiAuthParams
        {
            Phone = $"+{CountryCode}{PhoneNumber}",
            ForceSms = ForceSms
        });

        try
        {
            await _authFlow.AuthorizeAsync().ConfigureAwait(false);
            await Task.Delay(3000);
            AuthorizationCompleted?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception e)
        {
            await _notificationsWriter.WriteAsync(("Депрессия...", "Ты не смог залогинится"));
            throw;
        }
        await _notificationsWriter.WriteAsync(("Успешно!", "Успешный вход в аккаунт."));
    }
}
