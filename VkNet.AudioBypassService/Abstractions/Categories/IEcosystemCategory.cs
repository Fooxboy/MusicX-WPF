using System.Threading.Tasks;
using VkNet.AudioBypassService.Models.Auth;
using VkNet.AudioBypassService.Models.Ecosystem;

namespace VkNet.AudioBypassService.Abstractions.Categories;

public interface IEcosystemCategory
{
    Task<EcosystemSendOtpSmsResponse> SendOtpSmsAsync(string sid);
    Task<EcosystemCheckOtpResponse> CheckOtpAsync(string sid, LoginWay verificationMethod, string code);
}