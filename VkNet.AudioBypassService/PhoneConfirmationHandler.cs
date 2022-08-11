using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
namespace VkNet.AudioBypassService;

public delegate Task<string> PhoneConfirmationHandler(PhoneConfirmationEventArgs args, [CanBeNull] Func<Task<PhoneConfirmationEventArgs>> resend);