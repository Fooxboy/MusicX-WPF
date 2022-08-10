using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using JetBrains.Annotations;
using VkNet.Abstractions;
using VkNet.Abstractions.Authorization;
using VkNet.AudioBypassService.Abstractions;
using VkNet.AudioBypassService.Exceptions;
using VkNet.AudioBypassService.Models.Boom;
using VkNet.AudioBypassService.Models.Oauth;
using VkNet.AudioBypassService.Models.Vk;
using VkNet.AudioBypassService.Utils;
using VkNet.Exception;
using VkNet.Model;
using VkNet.Utils;
namespace VkNet.AudioBypassService;

public enum BoomAuthState
{
    None,
    GetPrivateKey,
    GetAnonymousToken,
    PhoneValidation,
    PhoneConfirmation,
    GetSilentToken,
    CheckSilentToken,
    GetBearerToken,
    GetVkToken,
    Authorized
}

public delegate Task<string> PhoneConfirmationHandler(PhoneConfirmationEventArgs args, [CanBeNull] Func<Task<PhoneConfirmationEventArgs>> resend);
public delegate Task<string> PasswordRequestedHandler(ValidatePhoneProfile profile);
public delegate Task<string> TwoFactorRequestedHandler();

public record PhoneConfirmationEventArgs(
    PhoneConfirmationType ValidationType, 
    int CodeLength,
    [property: CanBeNull] string PhoneTemplate, 
    PhoneConfirmationType? ValidationResend, 
    DateTimeOffset? DelayUntilResend);

public interface IBoomAuthorizationFlow : IAuthorizationFlow, IDisposable
{
    BoomAuthState State { get; }
    event EventHandler<BoomAuthState> StateChanged;
    
    event PhoneConfirmationHandler PhoneConfirmationRequested;
    event PasswordRequestedHandler PasswordRequested;
    event TwoFactorRequestedHandler TwoFactorRequested;
}

public class BoomAuthorization : IBoomAuthorizationFlow
{
    private const string OauthDomain = "https://oauth.vk.com";
    private const string ClientId = "6767438";
    private const string ApiId = "6767438";
    private const string OkDeviceId = "1a8c3efbc29bbd74";

    private sealed record BoomResponse<TPoco>(TPoco Data);
    
    private readonly IVkApiInvoker _invoker;
    private readonly IDeviceIdProvider _idProvider;
    private readonly HttpClient _musicApiClient = new()
    {
        BaseAddress = new("https://api.moosic.io"),
        DefaultRequestHeaders =
        {
            {"User-Agent", "okhttp/5.0.0-alpha.2"},
            {"X-App-Id", "android"},
            {"X-Client-Version", "10275"},
            {"X-Crc", "421954067"},
            {"X-From", OkDeviceId}
        }
    };
    private BoomAuthState _state;
    private IApiAuthParams _params;
    public BoomAuthorization(IVkApiInvoker invoker, IDeviceIdProvider idProvider)
    {
        _invoker = invoker;
        _idProvider = idProvider;
    }

    private async Task<string> GetPrivateKeyAsync()
    {
        State = BoomAuthState.GetPrivateKey;
        var response = await _musicApiClient.GetFromJsonAsync<BoomResponse<PrivateKeysResponse>>("/system/settings/?q=/extAppKeys");
        return response!.Data.VkAppPrivateKey;
    }

    private Task<AnonymousTokenResponse> GetAnonymousTokenAsync(string privateKey)
    {
        State = BoomAuthState.GetAnonymousToken;
        return _invoker.CallAsync<AnonymousTokenResponse>(new Uri($"{OauthDomain}/get_anonym_token"), new()
        {
            {"lang", "ru"},
            {"client_id", ClientId},
            {"api_id", ApiId},
            {"client_secret", privateKey},
            {"device_id", _idProvider.DeviceId}
        });
    }

    private Task<ValidatePhoneResponse> ValidatePhoneAsync(string phone, string anonToken, [CanBeNull] string sid, bool isResend, bool allowCallReset = true)
    {
        State = BoomAuthState.PhoneValidation;
        return _invoker.CallAsync<ValidatePhoneResponse>("auth.validatePhone", new()
        {
            {"lang", "ru"},
            {"api_id", ApiId},
            {"access_token", anonToken},
            {"device_id", _idProvider.DeviceId},
            {"gaid", _idProvider.Gaid},
            {"sak_version", RestClientWithUserAgent.SakVersion},
            {"sid", sid},
            {"phone", phone},
            {"allow_callreset", allowCallReset ? "1" : "0"},
            {"voice", isResend ? "1" : "0"}
        });
    }

    private Task<ValidatePhoneInfoResponse> ValidatePhoneInfoAsync(string phone, string anonToken, string sid)
    {
        return _invoker.CallAsync<ValidatePhoneInfoResponse>("auth.validatePhoneInfo", new()
        {
            {"lang", "ru"},
            {"api_id", ApiId},
            {"access_token", anonToken},
            {"device_id", _idProvider.DeviceId},
            {"sid", sid},
            {"phone", phone}
        });
    }

    private Task<ValidatePhoneConfirmResponse> ValidatePhoneConfirmAsync(string phone, string anonToken, string sid, string code)
    {
        State = BoomAuthState.PhoneConfirmation;
        return _invoker.CallAsync<ValidatePhoneConfirmResponse>("auth.validatePhoneConfirm", new()
        {
            {"lang", "ru"},
            {"api_id", ApiId},
            {"access_token", anonToken},
            {"device_id", _idProvider.DeviceId},
            {"gaid", _idProvider.Gaid},
            {"sak_version", RestClientWithUserAgent.SakVersion},
            {"sid", sid},
            {"phone", phone},
            {"code", code}
        });
    }

    private Task<SilentTokenResponse> AuthenticateAsync(string privateKey, string sid, string phone, string password, bool twoFactorSupported, string code)
    {
        State = BoomAuthState.GetSilentToken;
        return _invoker.CallAsync<SilentTokenResponse>(new Uri($"{OauthDomain}/token"), new()
        {
            {"lang", "ru"},
            {"client_id", ClientId},
            {"api_id", ApiId},
            {"client_secret", privateKey},
            {"grant_type", "phone_confirmation_sid"},
            {"device_id", _idProvider.DeviceId},
            {"gaid", _idProvider.Gaid},
            {"sak_version", RestClientWithUserAgent.SakVersion},
            {"2fa_supported", twoFactorSupported ? "1" : "0"},
            {"sid", sid},
            {"username", phone},
            {"password", password},
            {"code", code}
        });
    }

    private Task<SilentTokenResponse> CheckSilentTokenAsync(string privateKey, string token, string uuid)
    {
        State = BoomAuthState.CheckSilentToken;
        return _invoker.CallAsync<SilentTokenResponse>(new Uri($"{OauthDomain}/check_silent_token"), new()
        {
            {"lang", "ru"},
            {"client_id", ClientId},
            {"api_id", ApiId},
            {"client_secret", privateKey},
            {"device_id", _idProvider.DeviceId},
            {"gaid", _idProvider.Gaid},
            {"sak_version", RestClientWithUserAgent.SakVersion},
            {"skip", null as string},
            {"token", token},
            {"uuid", uuid}
        });
    }

    private Task<BoomTokensResponse> GetBoomTokensAsync(string silentToken, string uuid)
    {
        State = BoomAuthState.GetBearerToken;
        var args = "?" + Url.QueryFrom(new("device_id", OkDeviceId), new("device_os", "android"), new("uuid", uuid), new("silent_token", silentToken));
        
        return _musicApiClient.GetFromJsonAsync<BoomTokensResponse>(Url.Combine("/oauth/vkconnect/vk/token", args), new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = new JsonSnakeCaseNamingPolicy()
        });
    }

    private async Task<BoomVkConnectTokenResponse> GetVkConnectTokenAsync(string boomToken)
    {
        State = BoomAuthState.GetVkToken;
        using var request = new HttpRequestMessage(HttpMethod.Get, "/user/vkconnect_token");
        
        request.Headers.Authorization = new("Bearer", boomToken);

        using var response = await _musicApiClient.SendAsync(request);

        response.EnsureSuccessStatusCode();

        var boomResponse = await response.Content.ReadFromJsonAsync<BoomResponse<BoomVkConnectTokenResponse>>();

        return boomResponse!.Data;
    }

    public async Task<AuthorizationResult> AuthorizeAsync()
    {
        if (_params is null)
            throw new InvalidOperationException($"call {nameof(SetAuthorizationParams)} first");
        
        var privateKey = await GetPrivateKeyAsync();
        var (anonToken, _) = await GetAnonymousTokenAsync(privateKey);

        async Task<(PhoneConfirmationEventArgs Args, string Sid)> BuildArgs(string validationSid = null)
        {
            var phoneResponse = await ValidatePhoneAsync(_params.Phone, anonToken, validationSid, validationSid is not null, !_params.ForceSms.GetValueOrDefault(false));
            DateTimeOffset? resendDate = phoneResponse.Delay.HasValue ? DateTimeOffset.Now + TimeSpan.FromSeconds(phoneResponse.Delay.Value) : null;
            
            if (phoneResponse.ValidationType is not PhoneConfirmationType.CallReset)
                return (new(phoneResponse.ValidationType, phoneResponse.CodeLength, null, phoneResponse.ValidationResend, resendDate), phoneResponse.Sid);
            
            var phoneInfo = await ValidatePhoneInfoAsync(_params.Phone, anonToken, phoneResponse.Sid);
            
            if (phoneInfo.CallReset.CodeLength == 0)
                return (new(phoneResponse.ValidationType, phoneResponse.CodeLength, null, phoneResponse.ValidationResend, resendDate), phoneResponse.Sid);
            
            return (new(phoneResponse.ValidationType, phoneInfo.CallReset.CodeLength, phoneInfo.CallReset.PhoneTemplate, phoneResponse.ValidationResend, resendDate), phoneResponse.Sid);
        }

        var (args, sid) = await BuildArgs();

        var code = await PhoneConfirmationRequested!.Invoke(args, async () => (await BuildArgs(sid)).Args);

        var (confirmationSid, _, profile) = await ValidatePhoneConfirmAsync(_params.Phone, anonToken, sid, code);

        var password = await PasswordRequested!.Invoke(profile);

        SilentTokenResponse sakTokenResponse;

        while (true)
        {
            try
            {
                sakTokenResponse = await AuthenticateAsync(privateKey, confirmationSid, _params.Phone, password, _params.TwoFactorSupported.GetValueOrDefault(true), _params.Code);
                break;
            }
            catch (VkAuthException ex) when (ex.AuthError.Error == "need_validation")
            {
                _params.Code = _params.TwoFactorAuthorization is null ? await TwoFactorRequested!.Invoke() : _params.TwoFactorAuthorization();
            }
        }
        
        var (silentToken, silentTokenUuid, _) = await CheckSilentTokenAsync(privateKey, sakTokenResponse.SilentToken, sakTokenResponse.SilentTokenUuid);

        var (boomToken, _, expiresIn) = await GetBoomTokensAsync(silentToken, silentTokenUuid);

        var (token, userId) = await GetVkConnectTokenAsync(boomToken);

        State = BoomAuthState.Authorized;
        return new()
        {
            AccessToken = token,
            UserId = long.Parse(userId),
            ExpiresIn = expiresIn
        };
    }
    
    public void SetAuthorizationParams(IApiAuthParams authorizationParams)
    {
        _params = authorizationParams;
    }
    
    public BoomAuthState State
    {
        get => _state;
        private set
        {
            if (_state == value)
                return;
            _state = value;
            StateChanged?.Invoke(this, value);
        }
    }
    public event EventHandler<BoomAuthState> StateChanged;
    public event PhoneConfirmationHandler PhoneConfirmationRequested;
    public event PasswordRequestedHandler PasswordRequested;
    public event TwoFactorRequestedHandler TwoFactorRequested;
    public void Dispose()
    {
        _musicApiClient.Dispose();
    }
}
