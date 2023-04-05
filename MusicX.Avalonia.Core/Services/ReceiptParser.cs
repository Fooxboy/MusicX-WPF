namespace MusicX.Avalonia.Core.Services;

public class ReceiptParser
{
	private readonly FakeSafetyNetClient _safetyNetClient;

	public ReceiptParser(FakeSafetyNetClient safetyNetClient)
	{
		_safetyNetClient = safetyNetClient;
	}

	public async Task<string> GetReceipt()
	{
		var checkinResponse = await _safetyNetClient.CheckIn().ConfigureAwait(false);

		var registerResponse = await _safetyNetClient.Register(checkinResponse).ConfigureAwait(false);

		var result = registerResponse.Split(new[] { "=" }, StringSplitOptions.None);

		var key = result.FirstOrDefault();
		var value = result.LastOrDefault();

		if (key == null || value == null || key.ToLower() == "error")
		{
			throw new InvalidOperationException($"Bad Response: {registerResponse}");
		}

		return value;
	}
}