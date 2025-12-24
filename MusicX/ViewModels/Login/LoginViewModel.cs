using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using AsyncAwaitBestPractices.MVVM;
using Microsoft.Extensions.DependencyInjection;
using MusicX.Services;
using MusicX.ViewModels.Modals;
using MusicX.Views.Login;
using MusicX.Views.Modals;
using VkNet.Abstractions;
using VkNet.Extensions.Auth.Abstractions;
using VkNet.Extensions.Auth.Models.Auth;
using VkNet.Extensions.Auth.Models.Ecosystem;
using IAuthCategory = VkNet.Extensions.Auth.Abstractions.Categories.IAuthCategory;
using NavigationService = MusicX.Services.NavigationService;
using OtpCodePage = MusicX.Views.Login.OtpCodePage;

namespace MusicX.ViewModels.Login;

public class LoginViewModel : BaseViewModel
{
    private readonly IVkApiAuthAsync _auth;
    private readonly NavigationService _navigationService;
    private readonly IServiceProvider _serviceProvider;
    private readonly ConfigService _configService;
    private readonly IAuthCategory _authCategory;
    private readonly IExchangeTokenStore _exchangeTokenStore;
    public ICommand LoginCommand { get; }
    
    public TaskCompletionSource<bool> LoggedIn { get; } = new();
    
    public LoginViewModel(IVkApiAuthAsync auth, NavigationService navigationService, IServiceProvider serviceProvider,
        ConfigService configService, IAuthCategory authCategory, IExchangeTokenStore exchangeTokenStore)
    {
        _auth = auth;
        _navigationService = navigationService;
        _serviceProvider = serviceProvider;
        _configService = configService;
        _authCategory = authCategory;
        _exchangeTokenStore = exchangeTokenStore;
        LoginCommand = new AsyncCommand<string>(LoginAsync);
    }

    private async Task LoginAsync(string? login)
    {
        if (string.IsNullOrEmpty(login))
            return;

        try
        {
            await _auth.AuthorizeAsync(new AndroidApiAuthParams());

            await _auth.AuthorizeAsync(new AndroidApiAuthParams(login, null, CodeRequestedAsync,
                VerificationMethodRequestedAsync: VerificationMethodRequestedAsync));

            var (exchangeToken, profile) = await _authCategory.GetExchangeToken();

            _configService.Config.UserId = profile.Id;
            _configService.Config.UserName = $"{profile.FirstName} {profile.LastName}";
            await _exchangeTokenStore.SetExchangeTokenAsync(exchangeToken);
        }
        catch (Exception e)
        {
            LoggedIn.SetException(e);
            return;
        }
        
        LoggedIn.SetResult(true);
    }

    private ValueTask<LoginWay> VerificationMethodRequestedAsync(
        IReadOnlyCollection<EcosystemVerificationMethod> verificationMethods, AuthState state)
    {
        var viewModel = new LoginVerificationMethodsModalViewModel(verificationMethods, _navigationService);
        
        _navigationService.OpenModal<LoginVerificationMethodsModal>(viewModel);

        return new(viewModel.Submitted.Task);
    }

    private ValueTask<string?> CodeRequestedAsync(LoginWay requestedLoginWay, AuthState state)
    {
        if (requestedLoginWay == LoginWay.Passkey)
        {
            _navigationService.OpenExternalPage(new PasskeyPage());
            return default;
        }

        if (requestedLoginWay == LoginWay.Password)
        {
            var passwordViewModel = _serviceProvider.GetRequiredService<PasswordViewModel>();
            
            if (state is ProfileAuthState profileAuthState)
                passwordViewModel.Profile = profileAuthState.Profile;
            
            _navigationService.OpenExternalPage(new PasswordPage(passwordViewModel));

            return new(passwordViewModel.PasswordSubmitted.Task);
        }

        requestedLoginWay = UnwrapTwoFactorWay(requestedLoginWay);
        
        var otpCodeViewModel = _serviceProvider.GetRequiredService<OtpCodeViewModel>();
        
        otpCodeViewModel.LoginWay = requestedLoginWay;

        if (state is VerificationAuthState verificationAuthState)
        {
            otpCodeViewModel.CodeLength = verificationAuthState.CodeLength;
            otpCodeViewModel.Info = verificationAuthState.Info;
        }
        
        _navigationService.OpenExternalPage(new OtpCodePage(otpCodeViewModel));
        
        return new(otpCodeViewModel.Submitted.Task);
    }

    private static LoginWay UnwrapTwoFactorWay(LoginWay way)
    {
        if (way == LoginWay.TwoFactorCallReset)
            return LoginWay.CallReset;
        if (way == LoginWay.TwoFactorSms)
            return LoginWay.Sms;
        if (way == LoginWay.TwoFactorPush)
            return LoginWay.Push;
        if (way == LoginWay.TwoFactorEmail)
            return LoginWay.Email;
        
        return way;
    }
}