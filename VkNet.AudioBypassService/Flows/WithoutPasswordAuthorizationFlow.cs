using System.Threading.Tasks;
using VkNet.Abstractions.Core;
using VkNet.Abstractions.Utils;
using VkNet.AudioBypassService.Abstractions;
using VkNet.AudioBypassService.Models.Auth;
using VkNet.AudioBypassService.Utils;
using VkNet.Extensions.DependencyInjection;
using VkNet.Model;

namespace VkNet.AudioBypassService.Flows;

internal class WithoutPasswordAuthorizationFlow : VkAndroidAuthorizationBase
{
    public WithoutPasswordAuthorizationFlow(IVkTokenStore tokenStore, FakeSafetyNetClient safetyNetClient,
        IDeviceIdStore deviceIdStore, IVkApiVersionManager versionManager, ILanguageService languageService,
        IAsyncRateLimiter rateLimiter, IRestClient restClient, ICaptchaHandler captchaHandler,
        LibVerifyClient libVerifyClient) : base(tokenStore, safetyNetClient, deviceIdStore, versionManager,
        languageService, rateLimiter, restClient, captchaHandler, libVerifyClient)
    {
    }

    protected override Task<AuthorizationResult> AuthorizeAsync(AndroidApiAuthParams authParams)
    {
        return AuthAsync(authParams);
    }
}