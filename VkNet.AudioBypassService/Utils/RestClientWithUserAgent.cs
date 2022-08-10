using System.Collections.Generic;
using System.Net.Http;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using VkNet.Abstractions.Utils;
using VkNet.Utils;

namespace VkNet.AudioBypassService.Utils
{
	/// <inheritdoc cref="IRestClient" />
	[UsedImplicitly]
	public class RestClientWithUserAgent : RestClient
	{
		public const string SakVersion = "1.76";
		
		private static readonly IDictionary<string, string> VkHeaders = new Dictionary<string, string>
		{
			{ "User-Agent", $"SAK_{SakVersion}(com.uma.musicvk)/7.37-13617 (Android 12; SDK 32; armeabi-v7a; MusicX; ru; 2960x1440)" },
			// { "X-VK-Android-Client", "new" }
		};

		public RestClientWithUserAgent(HttpClient httpClient, ILogger<RestClient> logger) : base(httpClient, logger)
		{
			foreach (var header in VkHeaders)
			{
				httpClient.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
			}
		}
	}
}