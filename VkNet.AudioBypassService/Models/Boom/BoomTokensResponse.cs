namespace VkNet.AudioBypassService.Models.Boom;

public record BoomTokensResponse(string AccessToken, string RefreshToken, int ExpiresIn);
