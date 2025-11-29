using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using VkNet.Abstractions.Authorization;
using VkNet.Abstractions.Core;
using VkNet.Abstractions.Utils;
using VkNet.AudioBypassService.Abstractions;
using VkNet.AudioBypassService.Abstractions.Categories;
using VkNet.AudioBypassService.Models.Auth;
using VkNet.AudioBypassService.Models.LibVerify;
using VkNet.AudioBypassService.Utils;
using VkNet.Extensions.DependencyInjection;
using VkNet.Model;
using VkNet.Utils;
using VkNet.Utils.JsonConverter;
using ICaptchaHandler = VkNet.Extensions.DependencyInjection.ICaptchaHandler;
using VkApiInvoke = VkNet.AudioBypassService.Utils.VkApiInvoke;

namespace VkNet.AudioBypassService.Flows;

internal abstract class VkAndroidAuthorizationBase(
    IVkTokenStore tokenStore,
    IDeviceIdProvider deviceIdProvider,
    IVkApiVersionManager versionManager,
    ILanguageService languageService,
    IAsyncRateLimiter rateLimiter,
    IRestClient restClient,
    ICaptchaHandler captchaHandler,
    LibVerifyClient libVerifyClient,
    IAuthCategory authCategory)
    : IAuthorizationFlow
{
    private AndroidApiAuthParams? _apiAuthParams;

    public Task<AuthorizationResult> AuthorizeAsync()
    {
        if (_apiAuthParams == null)
            throw new InvalidOperationException("Authorization parameters are not set. Call SetAuthorizationParams first.");

        return AuthorizeAsync(_apiAuthParams);
    }

    protected abstract Task<AuthorizationResult> AuthorizeAsync(AndroidApiAuthParams authParams);

    public void SetAuthorizationParams(IApiAuthParams authorizationParams)
    {
        if (authorizationParams is not AndroidApiAuthParams authParams)
            throw new ArgumentException($"Authorization parameters must be of type {nameof(AndroidApiAuthParams)}", nameof(authorizationParams));
        
        _apiAuthParams = authParams;
    }

    protected async Task<AuthorizationResult> AuthAsync(AndroidApiAuthParams authParams)
    {
        if (authParams.IsAnonymous)
        {
            var (anonymousToken, anonymousTokenExpiration) = await authCategory.GetAnonymToken();

            return new()
            {
                State = authParams.State,
                ExpiresIn = anonymousTokenExpiration,
                AccessToken = anonymousToken
            };
        }

        return await captchaHandler.Perform(async captchaResponse =>
        {
            var parameters = await BuildParameters(authParams);
            
            captchaResponse?.AddTo(parameters);
            
            await rateLimiter.WaitNextAsync();

            var response = await restClient.PostAsync(new Uri("https://api.vk.com/oauth/token"), parameters, Encoding.UTF8);

            var obj = JObject.Parse(response.Value ?? response.Message);

            if (obj.TryGetValue("error", out var error) &&
                AuthFlow.FromJsonString(error.ToString()) == AuthFlow.NeedValidation)
            {
                var (loginWay, _, mask, _, externalId) = obj.ToObject<NeedValidationAuthResponse>(VkApiInvoke.Serializer)!;

                VerifyResponse? verifyResponse = null;
                if (loginWay == LoginWay.TwoFactorLibVerify)
                {
                    verifyResponse = await libVerifyClient.VerifyAsync(externalId, authParams.Login);

                    if (verifyResponse.Status != VerifyResponseStatus.Ok)
                        throw new VerificationException("Error verifying libverify session");
                }
                    

                var state = loginWay == LoginWay.TwoFactorLibVerify
                    ? new TwoFactorAuthState(mask, verifyResponse!.Checks.Any(b => b == VerifyChecks.Sms),
                        verifyResponse.CodeLength)
                    : new AuthState();

                var code = await authParams.ActionRequestedAsync(loginWay, state);
                
                if (verifyResponse is null)
                {
                    parameters.Remove("code");
                    parameters.Add("code", code);
                }
                else
                {
                    var (status, token) = await libVerifyClient.AttemptAsync(verifyResponse.VerificationUrl, code);
                    
                    if (status != VerifyResponseStatus.Ok)
                        throw new AuthenticationException("Error attempting libverify code");

                    parameters.Remove("validate_session");
                    parameters.Remove("validate_token");
                    
                    parameters.Add("validate_session", verifyResponse.SessionId);
                    parameters.Add("validate_token", token);
                }
                
                await rateLimiter.WaitNextAsync();

                response = await restClient.PostAsync(new Uri("https://api.vk.com/oauth/token"), parameters, Encoding.UTF8);

                obj = JObject.Parse(response.Value ?? response.Message);
            }
            
            VkAuthErrors.IfErrorThrowException(obj);

            var result = obj.ToObject<AuthorizationResult>(VkApiInvoke.Serializer)!;

            result.State = authParams.State;
        
            return result;
        });
    }

    protected virtual async ValueTask<VkParameters> BuildParameters(AndroidApiAuthParams authParams)
    {
        return new()
        {
            { "grant_type", authParams.AndroidGrantType },
            { "libverify_support", false }, // TODO: test lib verify cringe
            { "sid", authParams.Sid },
            { "scope", "all" },
            { "supported_ways", authParams.SupportedWays },
            { "device_id", await deviceIdProvider.GetDeviceIdAsync() },
            { "api_id", authParams.ApplicationId },
            { "https", true },
            { "lang", languageService.GetLanguage()?.ToString() ?? "ru" },
            { "v", versionManager.Version },
            { "anonymous_token", tokenStore.Token },
        };
    }
}