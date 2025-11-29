using System.Threading.Tasks;
using VkNet.Abstractions.Core;
using VkNet.Abstractions.Utils;
using VkNet.AudioBypassService.Abstractions;
using VkNet.AudioBypassService.Models.Auth;
using VkNet.AudioBypassService.Utils;
using VkNet.Extensions.DependencyInjection;
using VkNet.Model;
using VkNet.Utils;
using ICaptchaHandler = VkNet.Extensions.DependencyInjection.ICaptchaHandler;

namespace VkNet.AudioBypassService.Flows;

internal class WithoutPasswordAuthorizationFlow(
    IVkTokenStore tokenStore,
    IDeviceIdProvider deviceIdProvider,
    IDeviceIdStore deviceIdStore,
    IVkApiVersionManager versionManager,
    ILanguageService languageService,
    IAsyncRateLimiter rateLimiter,
    IRestClient restClient,
    ICaptchaHandler captchaHandler,
    LibVerifyClient libVerifyClient)
    : VkAndroidAuthorizationBase(tokenStore, deviceIdProvider, deviceIdStore, versionManager,
        languageService, rateLimiter, restClient, captchaHandler, libVerifyClient)
{
    protected override Task<AuthorizationResult> AuthorizeAsync(AndroidApiAuthParams authParams)
    {
        return AuthAsync(authParams);
    }

    protected override async ValueTask<VkParameters> BuildParameters(AndroidApiAuthParams authParams)
    {
        var parameters = await base.BuildParameters(authParams);
        
        parameters.Add("username", authParams.Login);
        parameters.Add("flow_type", "auth_without_password");
        parameters.Add("2fa_supported", true);
        parameters.Add("vk_connect_auth", true);
        
        return parameters;
    }
}