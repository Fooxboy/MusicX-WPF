using Microsoft.Win32;
using System;

namespace MusicX.RegistryPatches
{
    public class ListenTogetherPatch : IRegistryPatch
    {
        public bool CanRun()
        {
            var check = Registry.CurrentUser.OpenSubKey("Software", true).OpenSubKey("Classes", true).OpenSubKey("musicxshare");

            return check is null;
        }
        public int Number => 1;
        public bool RequiredAdmin => false;

        public void Run()
        {
            var classes = Registry.CurrentUser.OpenSubKey("Software", true).OpenSubKey("Classes", true);
            RegistryKey key = classes.CreateSubKey("musicxshare");
            key.SetValue("", "URL:musicxshare");
            key.SetValue("URL Protocol", "");
            var path = AppDomain.CurrentDomain.BaseDirectory + "\\MusicX.exe";
            key.CreateSubKey(@"shell\open\command").SetValue("", $"\"{path}\" \"%1\"");
        }
    }
}
