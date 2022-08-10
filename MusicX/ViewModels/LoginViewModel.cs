using System;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows.Input;
using AsyncAwaitBestPractices.MVVM;
using DryIoc;
using MusicX.Core.Services;
using MusicX.Services;
using VkNet.AudioBypassService;
using VkNet.Model;
namespace MusicX.ViewModels;

public class LoginViewModel : BaseViewModel
{
    private readonly IBoomAuthorizationFlow _authFlow;
    public ulong PhoneNumber { get; set; }
    public int CountryCode { get; set; } = 7;
    
    public ICommand SubmitPhoneCommand { get; }
    
    public ChannelReader<(string Header, string Content)> Notifications { get; }

    public TaskCompletionSource<string> CodeSource { get; } = new();
    public TaskCompletionSource<string> PasswordSource { get; } = new();
    public TaskCompletionSource<string>? TwoFactorSource { get; set; }

    private readonly ChannelWriter<(string Header, string Content)> _notificationsWriter;
    private readonly VkService _vkService;

    public LoginViewModel()
    {
        _vkService = StaticService.Container.Resolve<VkService>();
        _authFlow = (IBoomAuthorizationFlow)_vkService.vkApi.AuthorizationFlow;
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
            Phone = $"+{CountryCode}{PhoneNumber}"
        });

        try
        {
            var result = await _authFlow.AuthorizeAsync();
            await _vkService.SetTokenAsync(result.AccessToken, null!);
        }
        catch (Exception e)
        {
            await _notificationsWriter.WriteAsync(("Депрессия...", "Ты не смог залогинится"));
            throw;
        }
        await _notificationsWriter.WriteAsync(("Успешно!", "Успешный вход в аккаунт."));
    }
}
