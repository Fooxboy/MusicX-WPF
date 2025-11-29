using Newtonsoft.Json.Linq;
using VkNet.AudioBypassService.Models.Auth;
using VkNet.Exception;
using VkNet.Model;

namespace VkNet.AudioBypassService.Utils
{
	public static class VkAuthErrors
	{
		/// <summary>
		/// Выбрасывает ошибку, если есть в json.
		/// </summary>
		/// <param name="json"> JSON. </param>
		/// <exception cref="VkApiException">
		/// Неправильные данные JSON.
		/// </exception>
		public static void IfErrorThrowException(JObject json)
		{
			var error = json["error"];

			if (error == null || error.Type == JTokenType.Null)
			{
				return;
			}

			if (error.Type != JTokenType.String)
			{
				return;
			}

			var vkAuthError = json.ToObject<AuthError>(VkApiInvoke.Serializer)!;

			throw VkAuthErrorFactory.Create(vkAuthError);
		}
	}
}