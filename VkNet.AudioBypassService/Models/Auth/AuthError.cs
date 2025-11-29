using System;

namespace VkNet.AudioBypassService.Models.Auth;

public record AuthError(string Error, ulong? CaptchaSid, Uri? CaptchaImg, Uri? RedirectUri);