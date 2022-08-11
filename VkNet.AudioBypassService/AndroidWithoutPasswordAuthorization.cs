using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using VkNet.AudioBypassService.Abstractions;
using VkNet.AudioBypassService.Exceptions;
using VkNet.AudioBypassService.Models.Vk;
using VkNet.AudioBypassService.Utils;
using VkNet.Exception;
using VkNet.Model;
namespace VkNet.AudioBypassService;

public sealed class AndroidWithoutPasswordAuthorization : IBypassAuthorizationFlow
{
    private const string ApiId = "2274003";
    private const string AppSecret = "hHbZxrka2uZ6jB1inYsH";
    private const string VkApplicationId = "ae0715a6-7048-4ea1-894e-2fd22278d425";

    private readonly BypassAuthCategory _authCategory;
    private readonly BypassOauthService _oauthService;
    private readonly IDistributedCache _storage;
    private readonly LibVerifyService _verifyService;
    private IApiAuthParams _params;

    public AndroidWithoutPasswordAuthorization(BypassAuthCategory authCategory, BypassOauthService oauthService, IDistributedCache storage, LibVerifyService verifyService)
    {
        _authCategory = authCategory;
        _oauthService = oauthService;
        _storage = storage;
        _verifyService = verifyService;
    }

    private async Task<string> GetAnonymousTokenAsync()
    {
        const string key = "vkAnonymousToken";
        
        if (await _storage.GetStringAsync(key) is { } token)
            return token;
        
        var (anonToken, anonExpire) = await _oauthService.GetAnonymousTokenAsync(AppSecret, ApiId, ApiId);
        
        await _storage.SetStringAsync(key, anonToken, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(anonExpire)
        });

        return anonToken;
    }
    
    public async Task<AuthorizationResult> AuthorizeAsync()
    {
        const string key = "vkToken";
        
        if (await _storage.GetStringAsync(key) is { } token)
            return new()
            {
                AccessToken = token
            };
        
        var anonToken = await GetAnonymousTokenAsync();

        var (_, flowType, sid) = await _authCategory.ValidateAccountAsync(_params.Phone, anonToken, ApiId);

        AuthorizationResult result;
        switch (flowType)
        {
            case FlowType.NeedRegistration:
                throw new UserAuthorizationFailException(null);
            case FlowType.NeedValidation:
                result = await VerifyAsync(anonToken, sid);
                break;
            case FlowType.NeedPasswordAndValidation:
                _params.Password = await PasswordRequested!.Invoke(null);
                try
                {
                    result = await _oauthService.AuthenticateByPasswordAsync(AppSecret, ApiId, ApiId, anonToken, sid, _params.Phone, _params.Password, true, false);
                }
                catch (VkAuthException e) when (e.AuthError.Error == "need_validation")
                {
                    result = await VerifyAsync(anonToken, sid);
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(flowType), flowType, null);
        }

        if (result.AccessToken is not null)
            await _storage.SetStringAsync(key, result.AccessToken, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(result.ExpiresIn)
            });
        
        return result;
    }

    private async Task<AuthorizationResult> VerifyAsync(string anonToken, string sid)
    {
        async Task<(PhoneConfirmationEventArgs Args, string Sid, string ExternalId)> BuildArgs(string validationSid = null)
        {
            var phoneResponse = await _authCategory.ValidatePhoneAsync(_params.Phone, anonToken, ApiId, validationSid, validationSid is not null, !_params.ForceSms.GetValueOrDefault(false), "auth_without_password");
            DateTimeOffset? resendDate = phoneResponse.Delay.HasValue ? DateTimeOffset.Now + TimeSpan.FromSeconds(phoneResponse.Delay.Value) : null;
            
            if (phoneResponse.ValidationType is not PhoneConfirmationType.CallReset)
                return (new(phoneResponse.ValidationType, phoneResponse.CodeLength, null, phoneResponse.ValidationResend, resendDate), phoneResponse.Sid, phoneResponse.ExternalId);
            
            var phoneInfo = await _authCategory.ValidatePhoneInfoAsync(_params.Phone, anonToken, ApiId, phoneResponse.Sid, "auth_without_password");
            
            if (phoneInfo.CallReset.CodeLength == 0)
                return (new(phoneResponse.ValidationType, phoneResponse.CodeLength, null, phoneResponse.ValidationResend, resendDate), phoneResponse.Sid, phoneResponse.ExternalId);
            
            return (new(phoneResponse.ValidationType, phoneInfo.CallReset.CodeLength, phoneInfo.CallReset.PhoneTemplate, phoneResponse.ValidationResend, resendDate), phoneResponse.Sid, phoneResponse.ExternalId);
        }

        var (args, loginSid, _) = await BuildArgs(sid);

        // var sessionId = RandomString.Generate(32);
        //
        // _ = await _verifyService.VerifyAsync("VK", VkApplicationId, "VKCONNECT", "vk_registration", sessionId, externalId, _params.Phone);
        //
        // var (_, _, sessionToken) = await _verifyService.AttemptAsync("VK", VkApplicationId, "vk_registration", sessionId, _params.Code, _params.Phone);

        ValidatePhoneConfirmResponse response;
        while (true)
        {
            try
            {
                _params.Code = await PhoneConfirmationRequested!.Invoke(args, async () => (await BuildArgs(loginSid)).Args);
                response = await _authCategory.ValidatePhoneConfirmAsync(_params.Phone, anonToken, ApiId, loginSid, _params.Code, "auth_without_password");
                break;
            }
            catch (VkApiMethodInvokeException e) when (e.ErrorCode == 1110)
            {
                // try again
            }
        }

        var (confirmationSid, _, skipPassword, profile) = response;

        var password = skipPassword ? _params.Password : await PasswordRequested!.Invoke(profile);

        return await _oauthService.AuthenticateByPasswordAsync(AppSecret, ApiId, ApiId, anonToken, confirmationSid, _params.Phone, password, true, false);
    }
    public void SetAuthorizationParams(IApiAuthParams authorizationParams)
    {
        _params = authorizationParams;
    }
    
    public void Dispose()
    {
    }
    
    public event PhoneConfirmationHandler PhoneConfirmationRequested;
    public event PasswordRequestedHandler PasswordRequested;
    public event TwoFactorRequestedHandler TwoFactorRequested;
}
