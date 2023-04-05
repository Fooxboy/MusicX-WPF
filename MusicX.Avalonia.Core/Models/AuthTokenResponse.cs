namespace MusicX.Avalonia.Core.Models;

public record AuthTokenResponse(string AccessToken, int ExpiresIn, long UserId);