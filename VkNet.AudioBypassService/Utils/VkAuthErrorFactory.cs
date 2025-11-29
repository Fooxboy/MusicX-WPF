using VkNet.AudioBypassService.Exceptions;
using VkNet.AudioBypassService.Models.Auth;
using VkNet.Extensions.DependencyInjection;
using VkNet.Model;

namespace VkNet.AudioBypassService.Utils
{
	public static class VkAuthErrorFactory
	{
		public static System.Exception Create(AuthError error)
		{
			switch (error.Error)
			{
				case "need_captcha":
					return new CaptchaRequiredException(new VkError
					{
						ErrorCode = 14,
						ErrorMessage = "Captcha needed",
						CaptchaImg = error.CaptchaImg,
						CaptchaSid = error.CaptchaSid.GetValueOrDefault(),
						RedirectUri = error.RedirectUri
					});
				default:
					return new VkAuthException(new()
					{
						Error = error.Error,
						ErrorDescription = error.ErrorDescription ?? "",
						CaptchaImg = error.CaptchaImg,
						CaptchaSid = error.CaptchaSid
					});
			}
		}
	}
}