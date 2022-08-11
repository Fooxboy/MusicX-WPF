using VkNet.Utils.AntiCaptcha;
namespace MusicX.Core.Services;

public delegate Task<string> CaptchaRequestedHandler(string url);
public class CaptchaSolver : ICaptchaSolver
{
    public event CaptchaRequestedHandler? CaptchaRequested;
    public string? Solve(string url)
    {
        return CaptchaRequested?.Invoke(url).ConfigureAwait(false).GetAwaiter().GetResult();
    }
    public void CaptchaIsFalse()
    {
    }
}
