using System;
using JetBrains.Annotations;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VkNet.Abstractions.Utils;
using VkNet.AudioBypassService.Abstractions;
using VkNet.AudioBypassService.Utils;

namespace VkNet.AudioBypassService.Extensions;

public static class AudioBypassServiceCollection
{
	public static IServiceCollection AddAudioBypass([NotNull] this IServiceCollection services) =>
		services.AddAudioBypass(builder => builder.UseAndroid());

	public static IServiceCollection AddAudioBypass([NotNull] this IServiceCollection services, Action<AudioBypassBuilder> configure)
	{
		if (services == null)
		{
			throw new ArgumentNullException(nameof(services));
		}

		services.TryAddSingleton<FakeSafetyNetClient>();
		services.TryAddSingleton<IVkApiInvoker, VkApiInvoker>();
		services.TryAddSingleton<IRestClient, RestClientWithUserAgent>();
		services.TryAddSingleton<IReceiptParser, ReceiptParser>();
		services.TryAddTransient<IDeviceIdProvider, RandomDeviceIdProvider>();
		services.TryAddTransient<BypassAuthCategory>();
		services.TryAddTransient<BypassOauthService>();
		services.TryAddTransient<LibVerifyService>();
		services.AddDistributedMemoryCache();

		configure(new(services));

		return services;
	}
}