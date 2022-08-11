using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VkNet.Abstractions.Authorization;
using VkNet.AudioBypassService;
using VkNet.AudioBypassService.Abstractions;
namespace VkNet.AudioBypassService.Extensions;
public class AudioBypassBuilder
{
    private readonly IServiceCollection _services;
    internal AudioBypassBuilder(IServiceCollection services)
    {
        _services = services;
    }

    public AudioBypassBuilder UseAndroid()
    {
        _services.TryAddSingleton<IAuthorizationFlow, VkAndroidAuthorization>();
        return this;
    }

    public AudioBypassBuilder UseBoom()
    {
        _services.TryAddSingleton<IBypassAuthorizationFlow, BoomAuthorization>();
        _services.TryAddSingleton<IAuthorizationFlow>(s => s.GetRequiredService<IBypassAuthorizationFlow>());
        return this;
    }
	
    public AudioBypassBuilder UseAndroidNoPassword()
    {
        _services.TryAddSingleton<IBypassAuthorizationFlow, AndroidWithoutPasswordAuthorization>();
        _services.TryAddSingleton<IAuthorizationFlow>(s => s.GetRequiredService<IBypassAuthorizationFlow>());
        return this;
    }
}
