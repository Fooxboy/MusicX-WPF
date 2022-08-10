namespace VkNet.AudioBypassService.Models.Oauth;

public record SilentTokenResponse(string SilentToken, string SilentTokenUuid, int SilentTokenTtl);
