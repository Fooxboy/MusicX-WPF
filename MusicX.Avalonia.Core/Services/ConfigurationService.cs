using Microsoft.Win32;
using ReactiveUI;

namespace MusicX.Avalonia.Core.Services;

public class ConfigurationService
{
    private const string RootKey = "Software\\MusicX";

    public ConfigurationService()
    {
        MessageBus.Current.Listen<LoginState>().Subscribe(state => LoginState = state);
    }

    public LoginState? LoginState
    {
        get
        {
            var key = Registry.CurrentUser.OpenSubKey($"{RootKey}\\Login");
            if (key is null)
                return null;

            return new((string)key.GetValue("AccessToken")!, (long)key.GetValue("UserId")!);
        }
        set
        {
            if (value is null)
            {
                Registry.CurrentUser.DeleteSubKeyTree($"{RootKey}\\Login", false);
                return;
            }

            var key = Registry.CurrentUser.OpenSubKey($"{RootKey}\\Login", true) ??
                      Registry.CurrentUser.CreateSubKey($"{RootKey}\\Login",
                                                        RegistryKeyPermissionCheck.ReadWriteSubTree);
            
            key.SetValue("AccessToken", value.AccessToken, RegistryValueKind.String);
            key.SetValue("UserId", value.UserId, RegistryValueKind.QWord);
        }
    }
}

public record LoginState(string AccessToken, long UserId);