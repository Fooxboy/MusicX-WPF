using System;
using VkNet.Abstractions.Authorization;
namespace VkNet.AudioBypassService.Abstractions;

public interface IBypassAuthorizationFlow : IAuthorizationFlow, IDisposable
{
    event PhoneConfirmationHandler PhoneConfirmationRequested;
    event PasswordRequestedHandler PasswordRequested;
    event TwoFactorRequestedHandler TwoFactorRequested;
}
