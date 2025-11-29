using VkNet.Exception;
using VkNet.Model;

namespace VkNet.Extensions.DependencyInjection;

public class CaptchaRequiredException(VkError error) : VkApiMethodInvokeException(error)
{
    public VkError Error { get; } = error;
}