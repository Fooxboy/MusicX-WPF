using System;
using System.Threading.Tasks;
using VkNet.Abstractions.Core;
using VkNet.Abstractions.Utils;
using VkNet.AudioBypassService.Abstractions;
using VkNet.AudioBypassService.Models.Auth;
using VkNet.AudioBypassService.Utils;
using VkNet.Extensions.DependencyInjection;
using VkNet.Model;

namespace VkNet.AudioBypassService.Flows;

internal class PasswordAuthorizationFlow : VkAndroidAuthorizationBase
{
    protected override Task<AuthorizationResult> AuthorizeAsync(AndroidApiAuthParams authParams)
    {
        if (string.IsNullOrEmpty(authParams.Password) && !authParams.IsAnonymous)
            throw new ArgumentException("Password is required for this flow type", nameof(authParams));
        
        return AuthAsync(authParams);
    }

    public PasswordAuthorizationFlow(IVkTokenStore tokenStore, FakeSafetyNetClient safetyNetClient,
        IDeviceIdStore deviceIdStore, IVkApiVersionManager versionManager, ILanguageService languageService,
        IAsyncRateLimiter rateLimiter, IRestClient restClient, ICaptchaHandler captchaHandler,
        LibVerifyClient libVerifyClient) : base(tokenStore, safetyNetClient, deviceIdStore,
        versionManager, languageService, rateLimiter, restClient, captchaHandler, libVerifyClient)
    {
    }
}