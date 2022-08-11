using System;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Caching.Distributed;
using VkNet.AudioBypassService.Abstractions;

namespace VkNet.AudioBypassService.Utils
{
	[UsedImplicitly]
	internal class ReceiptParser : IReceiptParser
	{
		private readonly FakeSafetyNetClient _safetyNetClient;
		private readonly IDistributedCache _cache;

		public ReceiptParser([NotNull] FakeSafetyNetClient safetyNetClient, IDistributedCache cache)
		{
			_safetyNetClient = safetyNetClient;
			_cache = cache;
		}

		public async Task<string> GetReceiptAsync()
		{
			const string cacheKey = "safetyNetReceipt";

			if (await _cache.GetStringAsync(cacheKey) is { } value)
				return value;
			
			var checkinResponse = await _safetyNetClient.CheckIn().ConfigureAwait(false);

			var registerResponse = await _safetyNetClient.Register(checkinResponse).ConfigureAwait(false);

			var result = registerResponse.Split(new[] { "=" }, StringSplitOptions.None);

			var key = result.FirstOrDefault();
			value = result.LastOrDefault();

			if (key == null || value == null || key.ToLower() == "error")
			{
				throw new InvalidOperationException($"Bad Response: {registerResponse}");
			}

			await _cache.SetStringAsync(cacheKey, value);

			return value;
		}
	}
}