using System;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VkNet.Abstractions.Authorization;
using VkNet.Abstractions.Utils;
using VkNet.AudioBypassService;
using VkNet.AudioBypassService.Abstractions;
using VkNet.AudioBypassService.Utils;

namespace VkNet.AudioBypassService.Extensions
{
	public static class AudioBypassServiceCollection
	{
		public static IServiceCollection AddAudioBypass([NotNull] this IServiceCollection services) =>
			services.AddAudioBypass(builder => builder.UseAndroidAuthorization());

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

			configure(new(services));

			return services;
		}
	}
}

public class AudioBypassBuilder
{
	private readonly IServiceCollection _services;
	internal AudioBypassBuilder(IServiceCollection services)
	{
		_services = services;
	}

	public AudioBypassBuilder UseAndroidAuthorization()
	{
		_services.TryAddSingleton<IAuthorizationFlow, VkAndroidAuthorization>();
		return this;
	}

	public AudioBypassBuilder UseBoomAuthorization()
	{
		_services.TryAddSingleton<IBoomAuthorizationFlow, BoomAuthorization>();
		_services.TryAddSingleton<IAuthorizationFlow>(s => s.GetRequiredService<IBoomAuthorizationFlow>());
		return this;
	}
}