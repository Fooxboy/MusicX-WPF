using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using VkNet.AudioBypassService.Abstractions;
using VkNet.AudioBypassService.Models.Oauth;
using VkNet.Model;
namespace VkNet.AudioBypassService.Utils;

public sealed class BypassOauthService
{
    private const string OauthDomain = "https://oauth.vk.com";
    
    private readonly IVkApiInvoker _invoker;
    private readonly IDeviceIdProvider _idProvider;
    public BypassOauthService(IVkApiInvoker invoker, IDeviceIdProvider idProvider)
    {
        _invoker = invoker;
        _idProvider = idProvider;
    }
    public Task<AnonymousTokenResponse> GetAnonymousTokenAsync(string privateKey, string apiId, string clientId)
    {
        return _invoker.CallAsync<AnonymousTokenResponse>(new Uri($"{OauthDomain}/get_anonym_token"), new()
        {
            {"lang", "ru"},
            {"client_id", clientId},
            {"api_id", apiId},
            {"client_secret", privateKey},
            {"device_id", _idProvider.DeviceId}
        });
    }

    public Task<SilentTokenResponse> AuthenticateBySidAsync(string privateKey, string apiId, string clientId, string sid, string phone, string password, bool twoFactorSupported, string code)
    {
        return _invoker.CallAsync<SilentTokenResponse>(new Uri($"{OauthDomain}/token"), new()
        {
            {"lang", "ru"},
            {"client_id", clientId},
            {"api_id", apiId},
            {"client_secret", privateKey},
            {"grant_type", "phone_confirmation_sid"},
            {"device_id", _idProvider.DeviceId},
            {"gaid", _idProvider.Gaid},
            {"sak_version", RestClientWithUserAgent.SakVersion},
            {"2fa_supported", twoFactorSupported},
            {"sid", sid},
            {"username", phone},
            {"password", password},
            {"code", code}
        });
    }
    
    public Task<AuthorizationResult> AuthenticateByPasswordAsync(string privateKey, string apiId, string clientId, string anonToken, [CanBeNull] string sid, string phone, [CanBeNull] string password, [CanBeNull] string code, bool twoFactorSupported, bool libVerifySupport, string flowType = "auth_without_password")
    {
        return _invoker.CallAsync<AuthorizationResult>(new Uri($"{OauthDomain}/token"), new()
        {
            {"lang", "ru"},
            {"client_id", clientId},
            {"api_id", apiId},
            {"client_secret", privateKey},
            {"grant_type", string.IsNullOrEmpty(password) ? "without_password" : "password"},
            {"device_id", _idProvider.DeviceId},
            {"gaid", _idProvider.Gaid},
            {"sak_version", RestClientWithUserAgent.SakVersion},
            {"2fa_supported", twoFactorSupported},
            {"username", phone},
            {"password", password},
            {"flow_type", flowType},
            {"scope", "all"},
            {"libverify_support", libVerifySupport},
            {"anonymous_token", anonToken},
            {"sid", sid},
            {"code", code}
        });
    }

    public Task<SilentTokenResponse> CheckSilentTokenAsync(string privateKey, string apiId, string clientId, string token, string uuid)
    {
        return _invoker.CallAsync<SilentTokenResponse>(new Uri($"{OauthDomain}/check_silent_token"), new()
        {
            {"lang", "ru"},
            {"client_id", clientId},
            {"api_id", apiId},
            {"client_secret", privateKey},
            {"device_id", _idProvider.DeviceId},
            {"gaid", _idProvider.Gaid},
            {"sak_version", RestClientWithUserAgent.SakVersion},
            {"skip", null as string},
            {"token", token},
            {"uuid", uuid}
        });
    }
}
