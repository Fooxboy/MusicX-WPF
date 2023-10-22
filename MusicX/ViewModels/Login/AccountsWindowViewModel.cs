using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using AsyncAwaitBestPractices;
using AsyncAwaitBestPractices.MVVM;
using MusicX.Core.Services;
using MusicX.Services;
using MusicX.Views.Login;
using NLog;
using QRCoder;
using QRCoder.Xaml;
using VkNet.Abstractions;
using VkNet.AudioBypassService.Abstractions;
using VkNet.AudioBypassService.Abstractions.Categories;
using VkNet.AudioBypassService.Models.Auth;
using VkNet.AudioBypassService.Models.Ecosystem;
using VkNet.Enums.SafetyEnums;
using Wpf.Ui;
using Wpf.Ui.Common;
using IAuthCategory = VkNet.AudioBypassService.Abstractions.Categories.IAuthCategory;
using NavigationService = MusicX.Services.NavigationService;

namespace MusicX.ViewModels.Login;

public class AccountsWindowViewModel : BaseViewModel
{
    private readonly IAuthCategory _authCategory;
    private readonly ISnackbarService _snackbarService;
    private readonly NavigationService _navigationService;
    private readonly IVkApiAuthAsync _vkApiAuth;
    private readonly ILoginCategory _loginCategory;
    private readonly VkService _vkService;
    private readonly ConfigService _configService;
    private readonly Logger _logger;
    private readonly IExchangeTokenStore _exchangeTokenStore;
    private readonly IVkApi _vkApi;
    private readonly IEcosystemCategory _ecosystemCategory;
    public ICommand LoginCommand { get; }
    public ICommand LoginPasswordCommand { get; }
    
    public ICommand RegenerateQrCommand { get; }
    
    public ICommand Vk2FaCompleteCommand { get; }
    
    public ICommand RequestPasswordLogin { get; }

    public ICommand OpenTgChannelCommand { get; }

    public AccountsWindowPage CurrentPage { get; set; } = AccountsWindowPage.EnterLogin;
    
    public string? Sid { get; set; }
    
    public string? Login { get; set; }
    
    public DrawingImage? QrCode { get; set; }

    public AuthCheckStatus QrStatus { get; set; } = AuthCheckStatus.Loading;
    
    public AuthValidatePhoneResponse? Vk2FaResponse { get; set; }

    public bool Vk2FaCanUsePassword { get; set; } = true;

    public EcosystemProfile? Profile { get; set; }

    public event EventHandler? LoggedIn;

    private TaskCompletionSource<string>? _codeTask;
    private AndroidGrantType _grantType = AndroidGrantType.Password;

    public AccountsWindowViewModel(IAuthCategory authCategory, ISnackbarService snackbarService,
        NavigationService navigationService, IVkApiAuthAsync vkApiAuth, ILoginCategory loginCategory,
        VkService vkService, ConfigService configService, Logger logger, IExchangeTokenStore exchangeTokenStore, IVkApi vkApi, IEcosystemCategory ecosystemCategory)
    {
        _authCategory = authCategory;
        _snackbarService = snackbarService;
        _navigationService = navigationService;
        _vkApiAuth = vkApiAuth;
        _loginCategory = loginCategory;
        _vkService = vkService;
        _configService = configService;
        _logger = logger;
        _exchangeTokenStore = exchangeTokenStore;
        _vkApi = vkApi;
        _ecosystemCategory = ecosystemCategory;

        void Handler(Exception e)
        {
            snackbarService.Show("Не удалось авторизоваться!", e.Message);
            logger.Error(e);
        }
        
        LoginCommand = new AsyncCommand<string>(LoginAsync, onException: Handler);
        LoginPasswordCommand = new AsyncCommand<string>(LoginPasswordAsync, onException: Handler);
        RegenerateQrCommand = new RelayCommand(() => LoadQrCode(true).SafeFireAndForget());
        Vk2FaCompleteCommand = new AsyncCommand<string>(Vk2FaCompleteAsync, onException: Handler);
        RequestPasswordLogin = new RelayCommand(() =>
        {
            Vk2FaResponse = null;
            OpenPage(AccountsWindowPage.EnterPassword);
        });

        OpenTgChannelCommand = new RelayCommand(() =>
        {
            Process.Start(new ProcessStartInfo("https://t.me/MusicXPlayer") { UseShellExecute = true });
        });
        // maybe later (cannot get token as vk android app)
        // LoadQrCode().SafeFireAndForget();
    }

    private async Task Vk2FaCompleteAsync(string? arg)
    {
        if (string.IsNullOrEmpty(arg) || !int.TryParse(arg, out _) || arg.Length < Vk2FaResponse?.CodeLength)
            return;

        if (_grantType == AndroidGrantType.PhoneConfirmationSid)
        {
            if(string.IsNullOrEmpty(Sid))
            {
                _snackbarService.Show("Ошибка", "Sid оказался пустым");
            }

            var response = await _ecosystemCategory.CheckOtpAsync(Sid, Vk2FaResponse!.ValidationType, arg);

            Profile = response.Profile;

            Sid = response.Sid;

            if (response is { ProfileExist: true})
            {
                OpenPage(AccountsWindowPage.EnterPassword);

                return;
            }

            if(response is { ProfileExist: false })
            {
                _snackbarService.Show("Ошибка", "Вконтакте сообщил, что такого пользователя не существует");

                return;
            }
        }

        if (_codeTask is not null)
        {
            _codeTask.SetResult(arg);
            _codeTask = null;
            return;
        }

        _snackbarService.Show("Ошибка", $"Отправка кода с типом {_grantType} не реализована!");
    }

    private async Task LoadQrCode(bool forceRegenerate = false)
    {
        var (_, hash, _, url, _) = await _authCategory.GetAuthCodeAsync("MusicX Player", forceRegenerate);

        var generator = new QRCodeGenerator();
        var qrCode = generator.CreateQrCode(new PayloadGenerator.Url(url));
        var xaml = new XamlQRCode(qrCode);
        QrCode = xaml.GetGraphic(new(76, 76), new SolidColorBrush(Colors.Black), new SolidColorBrush(Colors.Transparent), false);
        
        while (QrStatus != AuthCheckStatus.Ok)
        {
            var response = await _authCategory.CheckAuthCodeAsync(hash);
            
            QrStatus = response.Status;
            
            switch (response.Status)
            {
                case AuthCheckStatus.Continue or AuthCheckStatus.ConfirmOnPhone:
                    await Task.Delay(5000);
                    continue;
                case AuthCheckStatus.Expired:
                    QrCode = null;
                    break;
                case AuthCheckStatus.Ok:
                {
                    if (response.SuperAppToken is not null)
                    {
                        var uuid = Guid.NewGuid().ToString().Replace("-", "");
                        await _loginCategory.ConnectAsync(uuid);
                        await _loginCategory.ConnectAuthCodeAsync(response.SuperAppToken, uuid);
                    }
                    else if (response.AccessToken is not null)
                    {
                        await _vkService.SetTokenAsync(response.AccessToken);
                    }
                    
                    LoggedIn?.Invoke(this, EventArgs.Empty);
                    break;
                }
            }
        }
    }

    private async Task LoginPasswordAsync(string? arg)
    {
        if (string.IsNullOrEmpty(arg))
            return;
        
        await _vkApiAuth.AuthorizeAsync(new AndroidApiAuthParams(Login, Sid, ActionRequestedAsync, 
                        new[] { LoginWay.Push, LoginWay.Email }, arg)
        {
            AndroidGrantType = _grantType
        });

        var (token, profile) = await _authCategory.GetExchangeToken();

        _vkApi.UserId = profile.Id;

        _configService.Config.UserId = profile.Id;
        _configService.Config.UserName = $"{profile.FirstName} {profile.LastName}";
        
        await _exchangeTokenStore.SetExchangeTokenAsync(token);
                    
        LoggedIn?.Invoke(this, EventArgs.Empty);
    }

    private ValueTask<string> ActionRequestedAsync(LoginWay requestedLoginWay, AuthState state)
    {
        if (state is TwoFactorAuthState twoFactorAuthState)
        {
            Vk2FaResponse = new(twoFactorAuthState.IsSms ? LoginWay.Sms : LoginWay.CallReset, LoginWay.None, null, 0,
                twoFactorAuthState.CodeLength, true, twoFactorAuthState.PhoneMask);
        }
        else
        {
            if (requestedLoginWay == LoginWay.TwoFactorCallReset)
                requestedLoginWay = LoginWay.CallReset;
            else if (requestedLoginWay == LoginWay.TwoFactorSms)
                requestedLoginWay = LoginWay.Sms;
            else if (requestedLoginWay == LoginWay.TwoFactorPush)
                requestedLoginWay = LoginWay.Push;
            else if (requestedLoginWay == LoginWay.TwoFactorEmail)
                requestedLoginWay = LoginWay.Email;
            else _logger.Error("Unknown login way {LoginWay}", requestedLoginWay);
            
            Vk2FaResponse = new(requestedLoginWay, LoginWay.None, null, 0, 6, false, Login);
        }
        
        _codeTask = new();

        Vk2FaCanUsePassword = false;
        OpenPage(AccountsWindowPage.Vk2Fa);

        return new(_codeTask.Task);
    }

    public void OpenPage(AccountsWindowPage page)
    {
        object view = page switch
        {
            AccountsWindowPage.EnterLogin => new EnterLoginPage(),
            AccountsWindowPage.EnterPassword => new EnterPasswordPage(),
            AccountsWindowPage.Vk2Fa => new Vk2FaPage(),
            _ => throw new ArgumentOutOfRangeException(nameof(page), page, null)
        };

        CurrentPage = page;
        _navigationService.OpenExternalPage(view);
    }

    private async Task LoginAsync(string? arg)
    {
        if (string.IsNullOrEmpty(arg))
            return;

        var (_, isPhone, authFlow, flowNames, sid, nextStep) = await _authCategory.ValidateAccountAsync(arg, loginWays:
            new[]
            {
                LoginWay.Password, LoginWay.Push, LoginWay.Sms, LoginWay.CallReset, LoginWay.ReserveCode,
                LoginWay.Codegen, LoginWay.Email
            });

        if (flowNames.All(b => b != AuthType.Password && b != AuthType.Otp))
        {
            _snackbarService.Show("Упс!", "К сожалению, плеер не поддерживает вход с аккаунтом без пароля, установите пароль.");
            return;
        }

        Sid = sid;
        Login = arg;
        Vk2FaCanUsePassword = flowNames.Any(b => b == AuthType.Password);

        // TODO implement without password flow
        /*if (isPhone && authFlow == AuthFlow.NeedValidation)
        {
            Vk2FaResponse = await _authCategory.ValidatePhoneAsync(arg, sid,
                    loginWays: new[]
                    {
                        LoginWay.Push, LoginWay.Email
                    });
            
            OpenPage(AccountsWindowPage.Vk2Fa);
            return;
        }*/
        
        if (nextStep is null || nextStep.VerificationMethod == LoginWay.Password)
        {
            Profile = new EcosystemProfile("Незнакомец", string.Empty, Login, false, false, "https://vk.com/images/camera_200.png");
            OpenPage(AccountsWindowPage.EnterPassword);
            return;
        }
        
        Vk2FaCanUsePassword = false;

        if (nextStep.VerificationMethod == LoginWay.Sms)
        {
            var (_, otpSid, smsInfo, codeLength) = await _ecosystemCategory.SendOtpSmsAsync(Sid);

            Sid = otpSid;
            
            _grantType = AndroidGrantType.PhoneConfirmationSid;

            Vk2FaResponse = new(nextStep.VerificationMethod, LoginWay.None, Sid, 0, codeLength, false, smsInfo);
            OpenPage(AccountsWindowPage.Vk2Fa);
            return;
        }

        if (nextStep.VerificationMethod == LoginWay.Codegen)
        {
            Vk2FaResponse = new(nextStep.VerificationMethod, LoginWay.None, Sid, 0, 6, false, null);
            _grantType = AndroidGrantType.PhoneConfirmationSid;

            OpenPage(AccountsWindowPage.Vk2Fa);
            return;
        }

        _snackbarService.Show("Ошибка", $"Вход с помощью {nextStep.VerificationMethod} пока что не реализован.");

        // TODO nextStep.HasAnotherVerificationMethods
    }
}

public enum AccountsWindowPage
{
    EnterLogin,
    EnterPassword,
    Vk2Fa
}