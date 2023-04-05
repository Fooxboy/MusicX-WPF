using Microsoft.Win32;

namespace MusicX.Avalonia.Core.Services;

public class ConfigurationService
{
    private const string RootKey = "Software\\MusicX";

    public LoginState? LoginState
    {
        get
        {
            var key = Registry.CurrentUser.OpenSubKey($"{RootKey}\\Login");
            if (key is null)
                return null;

            return new((string)key.GetValue("AccessToken")!, (long)key.GetValue("UserId")!);
        }
    }
}

public record LoginState(string AccessToken, long UserId);