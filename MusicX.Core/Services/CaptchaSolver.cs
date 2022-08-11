using VkNet.Utils.AntiCaptcha;
namespace MusicX.Core.Services;

public delegate Task<string> CaptchaHandler(string url);
public class CaptchaSolver : ICaptchaSolver
{
    public event CaptchaHandler? CaptchaRequested;
    public string? Solve(string url)
    {
        return CaptchaRequested?.Invoke(url).ConfigureAwait(false).GetAwaiter().GetResult();
    }
    public void CaptchaIsFalse()
    {
    }
}
