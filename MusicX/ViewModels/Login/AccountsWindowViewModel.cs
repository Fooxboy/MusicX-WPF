using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Networking.WindowsWebServices;
using AsyncAwaitBestPractices;
using AsyncAwaitBestPractices.MVVM;
using Microsoft.Extensions.DependencyInjection;
using MusicX.Core.Services;
using MusicX.Helpers;
using MusicX.Services;
using MusicX.ViewModels.Modals;
using MusicX.Views.Login;
using MusicX.Views.Modals;
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
    
    public ICommand NextStepCommand { get; }
    
    public ICommand ShowAnotherVerificationMethodsCommand { get; }

    public AccountsWindowPage CurrentPage { get; set; } = AccountsWindowPage.EnterLogin;
    
    public string? Sid { get; set; }
    
    public string? Login { get; set; }
    
    public DrawingImage? QrCode { get; set; }

    public AuthCheckStatus QrStatus { get; set; } = AuthCheckStatus.Loading;
    
    public AuthValidatePhoneResponse? Vk2FaResponse { get; set; }

    public bool Vk2FaCanUsePassword { get; set; } = true;

    public EcosystemProfile? Profile { get; set; }
    
    public bool HasAnotherVerificationMethods { get; set; }

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

        NextStepCommand = new AsyncCommand<LoginWay>(way => NextStepAsync(way!), onException: Handler);

        ShowAnotherVerificationMethodsCommand =
            new AsyncCommand(ShowAnotherVerificationMethodsAsync, onException: Handler);

        // maybe later (cannot get token as vk android app)
        // LoadQrCode().SafeFireAndForget();
    }

    private async Task ShowAnotherVerificationMethodsAsync()
    {
        if (Sid is null)
            return;
        
        var modal = StaticService.Container.GetRequiredService<LoginVerificationMethodsModalViewModel>();
        
        _navigationService.OpenModal<LoginVerificationMethodsModal>(modal);
        
        var (loginWay, _, _, info, _) = await await modal.LoadAsync(Sid);

        if (loginWay == LoginWay.Password)
        {
            OpenPage(AccountsWindowPage.EnterPassword);
            return;
        }

        await NextStepAsync(loginWay, info);
    }

    private async Task Vk2FaCompleteAsync(string? arg)
    {
        if (string.IsNullOrEmpty(arg) || !int.TryParse(arg, out _) || arg.Length < Vk2FaResponse?.CodeLength)
            return;

        if (_grantType == AndroidGrantType.PhoneConfirmationSid)
        {
            var response = await _ecosystemCategory.CheckOtpAsync(Sid, Vk2FaResponse!.ValidationType, arg);

            Profile = response.Profile;

            Sid = response.Sid;

            if (response is { ProfileExist: true })
            {
                if (response.CanSkipPassword)
                {
                    _grantType = AndroidGrantType.WithoutPassword;
                    await AuthAsync(null);
                }
                else
                    OpenPage(AccountsWindowPage.EnterPassword);
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

    private Task LoginPasswordAsync(string? arg)
    {
        return string.IsNullOrEmpty(arg) ? Task.CompletedTask : AuthAsync(arg);
    }

    private async Task AuthAsync(string? password)
    {
        await _vkApiAuth.AuthorizeAsync(new AndroidApiAuthParams(Login, Sid, ActionRequestedAsync, 
            new[] { LoginWay.Push, LoginWay.Email }, password)
        {
            AndroidGrantType = _grantType
        });

        await LoggedInAsync();
    }

    private async Task LoggedInAsync()
    {
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
            AccountsWindowPage.SelectLoginMethod => new SelectLoginMethodPage(),
            AccountsWindowPage.Passkey => new PasskeyPage(),
            _ => throw new ArgumentOutOfRangeException(nameof(page), page, null)
        };

        CurrentPage = page;
        _navigationService.OpenExternalPage(view);
    }

    private async Task AuthPasskeyAsync()
    {
        var hResult = PInvoke.WebAuthNGetCancellationId(out var cancellationId);

        if (hResult.Succeeded)
        {
            hResult = PInvoke.WebAuthNCancelCurrentOperation(cancellationId);
            if (!hResult.Succeeded)
            {
                _snackbarService.Show("Не удалось отменить текущий процесс входа", "MusicX не смог отменить текущий процесс входа! Закройте все диалоги входа с ключем и повторите попытку.");
                return;
            }
        }
        
        var (_, passkeyData) = await _authCategory.BeginPasskeyAsync(Sid);

        if (!TryBeginPasskey(passkeyData, out var authenticatorData, out var signature, out var userHandle,
                out var clientDataJson, out var usedCredential))
            return;

        var json = new JsonObject
        {
            ["response"] = new JsonObject
            {
                ["authenticatorData"] = authenticatorData.Base64UrlEncode(),
                ["signature"] = signature.Base64UrlEncode(),
                ["userHandle"] = userHandle.Base64UrlEncode(),
                ["clientDataJson"] = clientDataJson.Base64UrlEncode()
            },
            ["id"] = usedCredential.Base64UrlEncode(),
            ["rawId"] = usedCredential.Base64UrlEncode(),
            ["type"] = PInvoke.WebauthnCredentialTypePublicKey,
            ["clientExtensionResults"] = new JsonArray()
        };
        
        await _vkApiAuth.AuthorizeAsync(new AndroidApiAuthParams(null, Sid, ActionRequestedAsync, 
            new[] { LoginWay.Passkey }, PasskeyData: json.ToJsonString())
        {
            AndroidGrantType = _grantType
        });

        await LoggedInAsync();
    }

    private unsafe bool TryBeginPasskey(string passkeyData, out byte[] authenticatorData, out byte[] signature,
        out byte[] userHandle, out string clientDataJson, out byte[] usedCredential)
    {
        var data = JsonSerializer.Deserialize<PasskeyDataResponse>(passkeyData,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        var hWnd = new WindowInteropHelper(Application.Current.Windows[0]!).EnsureHandle();

        var dwVersion = PInvoke.WebAuthNGetApiVersionNumber();

        var publicKeyPtr = Marshal.StringToHGlobalUni(PInvoke.WebauthnCredentialTypePublicKey);

        var credList = data!.AllowCredentials
            .Where(b => b.Type == PInvoke.WebauthnCredentialTypePublicKey)
            .Select(b =>
            {
                var id = b.Id.Base64UrlDecode();
                var ptr = Marshal.AllocHGlobal(id.Length);
                Marshal.Copy(id, 0, ptr, id.Length);
            
                return new WEBAUTHN_CREDENTIAL
                {
                    dwVersion = dwVersion,
                    pwszCredentialType = (char*)publicKeyPtr,
                    pbId = (byte*)ptr,
                    cbId = (uint)id.Length
                };
        }).ToArray();

        clientDataJson =
            JsonSerializer.Serialize(new PInvoke.SecurityKeyClientData(PInvoke.SecurityKeyClientData.GetAssertion,
                data.Challenge, "https://id.vk.ru"));
        
        var clientDataJsonPtr = Utf8StringMarshaller.ConvertToUnmanaged(clientDataJson);
        var sha256Ptr = Marshal.StringToHGlobalUni("SHA-256");

        HRESULT hResult;
        WEBAUTHN_ASSERTION* assertion;
        try
        {
            fixed (WEBAUTHN_CREDENTIAL* credListPtr = &credList[0])
                hResult = PInvoke.WebAuthNAuthenticatorGetAssertion(new(hWnd), data.RpId, new()
                {
                    pbClientDataJSON = clientDataJsonPtr,
                    cbClientDataJSON = (uint)Encoding.UTF8.GetByteCount(clientDataJson),
                    dwVersion = 2,
                    pwszHashAlgId = (char*)sha256Ptr,
                }, new WEBAUTHN_AUTHENTICATOR_GET_ASSERTION_OPTIONS
                {
                    dwVersion = 4,
                    dwTimeoutMilliseconds = (uint)data.Timeout,
                    CredentialList = new()
                    {
                        cCredentials = (uint)credList.Length,
                        pCredentials = credListPtr
                    },
                    dwUserVerificationRequirement = data.UserVerification == "required" ? 1u : 0,
                    dwAuthenticatorAttachment = PInvoke.WebauthnAuthenticatorAttachmentCrossPlatformU2FV2
                }, out assertion);
        }
        finally
        {
            Utf8StringMarshaller.Free(clientDataJsonPtr);
            
            foreach (var credential in credList)
            {
                Marshal.FreeHGlobal((IntPtr)credential.pbId);
            }
            
            Marshal.FreeHGlobal(sha256Ptr);
            Marshal.FreeHGlobal(publicKeyPtr);
        }

        if (hResult.Failed)
        {
            var ptr = PInvoke.WebAuthNGetErrorName(hResult);
            throw new COMException(new(ptr.AsSpan()), hResult.Value);
        }

        authenticatorData = new ReadOnlySpan<byte>(assertion->pbAuthenticatorData, (int)assertion->cbAuthenticatorData)
            .ToArray();
        signature = new ReadOnlySpan<byte>(assertion->pbSignature, (int)assertion->cbSignature)
            .ToArray();
        userHandle = new ReadOnlySpan<byte>(assertion->pbUserId, (int)assertion->cbUserId)
            .ToArray();
        usedCredential = new ReadOnlySpan<byte>(assertion->Credential.pbId, (int)assertion->Credential.cbId)
            .ToArray();
        
        PInvoke.WebAuthNFreeAssertion(assertion);
        
        return hResult.Succeeded;
    }

    private async Task LoginAsync(string? arg)
    {
        if (string.IsNullOrEmpty(arg))
            return;

        var (_, isPhone, authFlow, flowNames, sid, nextStep) = await _authCategory.ValidateAccountAsync(arg, passkeySupported: true, loginWays:
            new[]
            {
                LoginWay.Password, LoginWay.Push, LoginWay.Sms, LoginWay.CallReset, LoginWay.ReserveCode,
                LoginWay.Codegen, LoginWay.Email, LoginWay.Passkey
            });

        PInvoke.WebAuthNIsUserVerifyingPlatformAuthenticatorAvailable(out var authenticatorAvailable);
        uint? authenticatorVersion = authenticatorAvailable ? PInvoke.WebAuthNGetApiVersionNumber() : null;
        
        if (authenticatorVersion >= 4 && flowNames.All(b => b != AuthType.Password && b != AuthType.Otp))
        {
            _snackbarService.Show("Упс!", "К сожалению, ваша система не поддерживает вход с аккаунтом без пароля, установите пароль или обновите систему.");
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
        
        HasAnotherVerificationMethods = nextStep?.HasAnotherVerificationMethods ?? false;

        if (flowNames.Any(b => b == AuthType.Passkey))
        {
            if (flowNames.Count > 1 && nextStep?.HasAnotherVerificationMethods is null or true)
            {
                OpenPage(AccountsWindowPage.SelectLoginMethod);
                return;
            }

            await NextStepAsync(LoginWay.Passkey);
            return;
        }
        
        if (nextStep is null || nextStep.VerificationMethod == LoginWay.Password)
        {
            Profile = new EcosystemProfile("Незнакомец", string.Empty, Login, false, false, "https://vk.com/images/camera_200.png");
            OpenPage(AccountsWindowPage.EnterPassword);
            return;
        }
        
        await NextStepAsync(nextStep.VerificationMethod);
    }

    private async Task NextStepAsync(LoginWay loginWay, string? phone = null)
    {
        Vk2FaCanUsePassword = false;

        if (loginWay == LoginWay.Passkey)
        {
            _grantType = AndroidGrantType.Passkey;
            OpenPage(AccountsWindowPage.Passkey);
            await AuthPasskeyAsync();
            return;
        }
        
        _grantType = AndroidGrantType.PhoneConfirmationSid;

        var codeLength = 6;

        if (loginWay == LoginWay.Sms)
        {
            var (_, otpSid, smsInfo, requestedCodeLength) = await _ecosystemCategory.SendOtpSmsAsync(Sid);

            Sid = otpSid;
            codeLength = requestedCodeLength;
            phone ??= smsInfo;
        }
        else if (loginWay == LoginWay.CallReset)
        {
            var (_, otpSid, smsInfo, requestedCodeLength) = await _ecosystemCategory.SendOtpCallResetAsync(Sid);

            Sid = otpSid;
            codeLength = requestedCodeLength;
            phone ??= smsInfo;
        }
        else if (loginWay == LoginWay.Push)
        {
            var (_, otpSid, smsInfo, requestedCodeLength) = await _ecosystemCategory.SendOtpPushAsync(Sid);
            
            Sid = otpSid;
            codeLength = requestedCodeLength;
            phone ??= smsInfo;
        }
        
        phone ??= Login;

        Vk2FaResponse = new(loginWay, LoginWay.None, Sid, 0, codeLength, false, phone);
        OpenPage(AccountsWindowPage.Vk2Fa);
    }
}

public enum AccountsWindowPage
{
    EnterLogin,
    EnterPassword,
    Vk2Fa,
    Passkey,
    SelectLoginMethod
}