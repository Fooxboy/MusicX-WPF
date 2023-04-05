namespace MusicX.Avalonia.Core.Models;

public record AuthExceptionResponse(string Error, string ErrorType, string? ErrorDescription, ulong? CaptchaSid, string? CaptchaImg);