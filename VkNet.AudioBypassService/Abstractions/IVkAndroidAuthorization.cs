using System.Threading.Tasks;
using VkNet.Abstractions.Authorization;
namespace VkNet.AudioBypassService.Abstractions;

public interface IVkAndroidAuthorization : IAuthorizationFlow
{
    Task<string> RefreshTokenAsync(string oldToken, string receipt);
    Task<string> AuthByExchangeTokenAsync(string oldToken);
}
