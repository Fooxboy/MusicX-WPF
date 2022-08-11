using System;
using JetBrains.Annotations;
using VkNet.AudioBypassService.Models.Vk;
namespace VkNet.AudioBypassService;

public record PhoneConfirmationEventArgs(
    PhoneConfirmationType ValidationType, 
    int CodeLength,
    [property: CanBeNull] string PhoneTemplate, 
    PhoneConfirmationType? ValidationResend, 
    DateTimeOffset? DelayUntilResend);