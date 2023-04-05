using System.Net.Http.Headers;
using Google.Protobuf;
using MusicX.Avalonia.Core.Models.Google;

namespace MusicX.Avalonia.Core.Services;

public class FakeSafetyNetClient : IDisposable
{
	private const string GcmUserAgent = "Android-GCM/1.5 (generic_x86 KK)";

	private readonly HttpClient _httpClient;

	public FakeSafetyNetClient()
	{
		_httpClient = new();
		_httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(GcmUserAgent);
	}

	internal async Task<AndroidCheckinResponse> CheckIn()
	{
		var androidRequest = new AndroidCheckinRequest
		{
			Checkin = new()
			{
				CellOperator = "310260",
				Roaming = "mobile:LTE:",
				SimOperator = "310260",
				Type = DeviceType.DeviceAndroidOs
			},
			Digest = "1-929a0dca0eee55513280171a8585da7dcd3700f8",
			Locale = "en_US",
			LoggingId = -8212629671123625360,
			Meid = "358240051111110",
			OtaCert =
			{
				"71Q6Rn2DDZl1zPDVaaeEHItd+Yg="
			},
			TimeZone = "America/New_York",
			Version = 3
		};

		var requestStream = new MemoryStream();
		using (var codecStream = new CodedOutputStream(requestStream))
			androidRequest.WriteTo(codecStream);

		var content = new ByteArrayContent(requestStream.ToArray());
		content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-protobuffer");

		var response = await _httpClient.PostAsync("https://android.clients.google.com/checkin", content).ConfigureAwait(false);

		response.EnsureSuccessStatusCode();

		var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

		var androidCheckinResponse = AndroidCheckinResponse.Parser.ParseFrom(responseStream);

		return androidCheckinResponse;
	}

	internal async Task<string> Register(AndroidCheckinResponse credentials)
	{
		var requestParams = GetRegisterRequestParams(RandomString.Generate(22), credentials.AndroidId.ToString());

		var content = new FormUrlEncodedContent(requestParams);

		var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "https://android.clients.google.com/c2dm/register3")
		{
			Content = content
		};
		httpRequestMessage.Headers.TryAddWithoutValidation("Authorization", $"AidLogin {credentials.AndroidId}:{credentials.SecurityToken}");

		var response = await _httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);

		response.EnsureSuccessStatusCode();

		var registerResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

		return registerResponse;
	}

	private Dictionary<string, string> GetRegisterRequestParams(string appId, string device)
	{
		return new()
		{
			{ "X-scope", "*" },
			{ "X-subtype", "841415684880" },
			{ "sender", "841415684880" },
			{ "X-appid", appId },
			{ "app", "com.vkontakte.android" },
			{ "device", device },
		};
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		_httpClient?.Dispose();
	}
}