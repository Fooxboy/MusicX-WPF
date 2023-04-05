using System.Security.Cryptography;
using System.Text;

namespace MusicX.Avalonia.Core.Services;

internal static class RandomString
{
	private const string Chars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_-";

	public static string Generate(int length)
	{
		Span<byte> bytes = stackalloc byte[length];
		RandomNumberGenerator.Fill(bytes);
		
		Span<char> result = stackalloc char[length];
		for (var i = 0; i < bytes.Length; i++)
		{
			result[i] = Chars[bytes[i] % Chars.Length];
		}

		return new(result);
	}
}