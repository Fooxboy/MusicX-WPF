using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using VkNet.Abstractions;
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

public sealed class BoomAuthorization : IBypassAuthorizationFlow
{
    private const string ApiId = "6767438";
    private const string OkDeviceId = "1a8c3efbc29bbd74";

    private sealed record BoomResponse<TPoco>(TPoco Data);

    private readonly BypassAuthCategory _authCategory;
    private readonly BypassOauthService _oauthService;
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
    private IApiAuthParams _params;
    public BoomAuthorization(BypassAuthCategory authCategory, BypassOauthService oauthService)
    {
        _authCategory = authCategory;
        _oauthService = oauthService;
    }

    private async Task<string> GetPrivateKeyAsync()
    {
        var response = await _musicApiClient.GetFromJsonAsync<BoomResponse<PrivateKeysResponse>>("/system/settings/?q=/extAppKeys");
        return response!.Data.VkAppPrivateKey;
    }

    private Task<BoomTokensResponse> GetBoomTokensAsync(string silentToken, string uuid)
    {
        var args = "?" + Url.QueryFrom(new("device_id", OkDeviceId), new("device_os", "android"), new("uuid", uuid), new("silent_token", silentToken));
        
        return _musicApiClient.GetFromJsonAsync<BoomTokensResponse>(Url.Combine("/oauth/vkconnect/vk/token", args), new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = new JsonSnakeCaseNamingPolicy()
        });
    }

    private async Task<BoomVkConnectTokenResponse> GetVkConnectTokenAsync(string boomToken)
    {
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
        var (anonToken, _) = await _oauthService.GetAnonymousTokenAsync(privateKey, ApiId, ApiId);

        async Task<(PhoneConfirmationEventArgs Args, string Sid)> BuildArgs(string validationSid = null)
        {
            var phoneResponse = await _authCategory.ValidatePhoneAsync(_params.Phone, anonToken, ApiId, validationSid, validationSid is not null, !_params.ForceSms.GetValueOrDefault(false));
            DateTimeOffset? resendDate = phoneResponse.Delay.HasValue ? DateTimeOffset.Now + TimeSpan.FromSeconds(phoneResponse.Delay.Value) : null;
            
            if (phoneResponse.ValidationType is not PhoneConfirmationType.CallReset)
                return (new(phoneResponse.ValidationType, phoneResponse.CodeLength, null, phoneResponse.ValidationResend, resendDate), phoneResponse.Sid);
            
            var phoneInfo = await _authCategory.ValidatePhoneInfoAsync(_params.Phone, anonToken, ApiId, phoneResponse.Sid);
            
            if (phoneInfo.CallReset.CodeLength == 0)
                return (new(phoneResponse.ValidationType, phoneResponse.CodeLength, null, phoneResponse.ValidationResend, resendDate), phoneResponse.Sid);
            
            return (new(phoneResponse.ValidationType, phoneInfo.CallReset.CodeLength, phoneInfo.CallReset.PhoneTemplate, phoneResponse.ValidationResend, resendDate), phoneResponse.Sid);
        }

        var (args, sid) = await BuildArgs();

        ValidatePhoneConfirmResponse response;
        while (true)
        {
            try
            {
                _params.Code = await PhoneConfirmationRequested!.Invoke(args, async () => (await BuildArgs(sid)).Args);
                response = await _authCategory.ValidatePhoneConfirmAsync(_params.Phone, anonToken, ApiId, sid, _params.Code);
                break;
            }
            catch (VkApiMethodInvokeException e) when (e.ErrorCode == 1110)
            {
                // try again
            }
        }

        var (confirmationSid, _, skipPassword, profile) = response;

        var password = skipPassword ? null : await PasswordRequested!.Invoke(profile);

        SilentTokenResponse sakTokenResponse;

        while (true)
        {
            try
            {
                sakTokenResponse = await _oauthService.AuthenticateBySidAsync(privateKey, ApiId, ApiId, confirmationSid, _params.Phone, password, _params.TwoFactorSupported.GetValueOrDefault(true), _params.Code);
                break;
            }
            catch (VkAuthException ex) when (ex.AuthError.Error == "need_validation")
            {
                _params.Code = _params.TwoFactorAuthorization is null ? await TwoFactorRequested!.Invoke() : _params.TwoFactorAuthorization();
            }
        }
        
        var (silentToken, silentTokenUuid, _) = await _oauthService.CheckSilentTokenAsync(privateKey, ApiId, ApiId, sakTokenResponse.SilentToken, sakTokenResponse.SilentTokenUuid);

        var (boomToken, _, expiresIn) = await GetBoomTokensAsync(silentToken, silentTokenUuid);

        var (token, userId) = await GetVkConnectTokenAsync(boomToken);

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
    
    public event PhoneConfirmationHandler PhoneConfirmationRequested;
    public event PasswordRequestedHandler PasswordRequested;
    public event TwoFactorRequestedHandler TwoFactorRequested;
    public void Dispose()
    {
        _musicApiClient.Dispose();
    }
}
