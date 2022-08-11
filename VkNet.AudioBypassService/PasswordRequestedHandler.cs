using System.Threading.Tasks;
using JetBrains.Annotations;
using VkNet.AudioBypassService.Models.Vk;
namespace VkNet.AudioBypassService;

public delegate Task<string> PasswordRequestedHandler([CanBeNull] ValidatePhoneProfile profile);