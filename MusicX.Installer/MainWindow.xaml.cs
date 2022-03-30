using IWshRuntimeLibrary;
using Microsoft.Win32;
using MusicX.Installer.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MusicX.Installer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Release release;

        private string CachePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\musicx\\release.zip";
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            IntPtr windowHandle = new WindowInteropHelper(this).Handle;

            WPFUI.Appearance.Background.Remove(windowHandle);

            var appTheme = WPFUI.Appearance.Theme.GetAppTheme();
            var systemTheme = WPFUI.Appearance.Theme.GetSystemTheme();
            WPFUI.Appearance.Theme.Set(
            WPFUI.Appearance.ThemeType.Dark,     // Theme type
            WPFUI.Appearance.BackgroundType.Mica, // Background type
            true                                  // Whether to change accents automatically
            );

            if (WPFUI.Appearance.Theme.IsAppMatchesSystem())
            {
                this.Background = Brushes.Transparent;
                WPFUI.Appearance.Background.Apply(windowHandle, WPFUI.Appearance.BackgroundType.Mica);

            }

            this.InstallButton.Visibility = Visibility.Collapsed;


            try
            {
                release = await GetLastRelease();

                this.ErrorGrid.Visibility = Visibility.Collapsed;
                this.ContentPanel.Visibility = Visibility.Visible;
                this.LoadingGrid.Visibility = Visibility.Collapsed;

                this.FirstStackPanel.Visibility = Visibility.Visible;
                this.TwoStackPanel.Visibility = Visibility.Collapsed;
                this.InstallButton.Visibility = Visibility.Visible;
                this.Version.Text = release.TagName;
            }
            catch(Exception ex)
            {

                LoadingGrid.Visibility = Visibility.Collapsed;
                ContentPanel.Visibility = Visibility.Collapsed;
                ErrorGrid.Visibility = Visibility.Visible;
            }
            
        }

        private async Task<Release> GetLastRelease()
        {
            try
            {
                Release release;
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("user-agent", "musicxinstaller v1");
                    var res = await client.GetAsync("https://api.github.com/repos/fooxboy/musicxreleases/releases/latest");
                    var json = await res.Content.ReadAsStringAsync();

                    release = JsonSerializer.Deserialize<Release>(json);  
                }

                return release;
            }
            catch (Exception ex)
            {

                LoadingGrid.Visibility = Visibility.Collapsed;
                ContentPanel.Visibility = Visibility.Collapsed;
                ErrorGrid.Visibility = Visibility.Visible;

                return null;
            }
        }

        private async Task DownloadRelease()
        {
            try
            {
                if(System.IO.File.Exists(CachePath)) System.IO.File.Delete(CachePath);

                if(!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\musicx"))
                {
                    Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\musicx");
                }
                using (var client = new WebClient())
                {
                    client.DownloadProgressChanged += Client_DownloadProgressChanged;
                    client.DownloadFileCompleted += Client_DownloadFileCompleted;

                    await client.DownloadFileTaskAsync(release.Assets[0].BrowserDownloadUrl, CachePath);
                }
            }catch (Exception ex)
            {
                LoadingGrid.Visibility = Visibility.Collapsed;
                ContentPanel.Visibility = Visibility.Collapsed;
                ErrorGrid.Visibility = Visibility.Visible;
            }

        }

        private void Client_DownloadFileCompleted(object? sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if(e.Error != null)
            {
                LoadingGrid.Visibility = Visibility.Collapsed;
                ContentPanel.Visibility = Visibility.Collapsed;
                ErrorGrid.Visibility = Visibility.Visible;
                runTimer = false;

            }else
            {
                runTimer = false;

                UnpackRelease();
            }
        }

        double currentBytes;
        private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            try
            {
                this.currentBytes = e.BytesReceived;
                Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    this.DownloadProgress.Maximum = e.TotalBytesToReceive;
                    this.DownloadProgress.Value = e.BytesReceived;


                    double left = e.TotalBytesToReceive - e.BytesReceived;

                    left = left / 1024;

                    if (left > 1024)
                    {
                        KindLeft.Text = "МБ";
                        left /= 1024;
                        ValueLeft.Text = Math.Round(left, 2).ToString();
                    }
                    else
                    {
                        KindLeft.Text = "КБ";
                        ValueLeft.Text = Math.Round(left, 2).ToString();
                    }

                });
            }catch (Exception ex)
            {
                LoadingGrid.Visibility = Visibility.Collapsed;
                ContentPanel.Visibility = Visibility.Collapsed;
                ErrorGrid.Visibility = Visibility.Visible;
            }
           
        }

        private async void InstallButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (PathInstall.Text == string.Empty) return;

                runTimer = true;

                new Thread(async () => await Timer()).Start();


                TwoStackPanel.Visibility = Visibility.Visible;
                FirstStackPanel.Visibility = Visibility.Collapsed;
                InstallButton.Visibility = Visibility.Collapsed;
                await DownloadRelease();
            }catch(Exception ex)
            {
                LoadingGrid.Visibility = Visibility.Collapsed;
                ContentPanel.Visibility = Visibility.Collapsed;
                ErrorGrid.Visibility = Visibility.Visible;
            }
           
        }

        double lastBytes;

        bool runTimer;

        private async Task Timer()
        {
            while(runTimer)
            {
                await Task.Delay(100);

                var speed = (currentBytes - lastBytes) * 10;   

                lastBytes = currentBytes;


                speed /= 1024;

                if(speed > 1024)
                {
                    speed /= 1024;
                    await Application.Current.Dispatcher.BeginInvoke(()=>
                    {
                        SpeedValue.Text = Math.Round(speed, 2).ToString();
                        SpeedKind.Text = "МБ/сек";

                    });
                }else
                {
                    await Application.Current.Dispatcher.BeginInvoke(() =>
                    {
                        SpeedValue.Text = Math.Round(speed, 2).ToString();
                        SpeedKind.Text = "КБ/сек";

                    });
                }

            }
        }

        private void UnpackRelease()
        {
            try
            {
                Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    TextState.Text = "Выполняется распаковка файлов...";

                });

                if (!Directory.Exists(PathInstall.Text))
                {
                    var dir = Directory.CreateDirectory(PathInstall.Text);


                    DirectorySecurity dSecurity = dir.GetAccessControl();
                    dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
                    dir.SetAccessControl(dSecurity);

                }
                ZipFile.ExtractToDirectory(CachePath, PathInstall.Text, true);

                System.IO.File.Delete(CachePath);

                if(CreateDesktopLink.IsChecked.Value)
                {
                    this.AppShortcutToDesktop();
                }

                if(CreateStartLink.IsChecked.Value)
                {
                    this.AppShortcutToStart();

                }

                RegistryPath();

                CreateLogDir();


                Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    ThreeStackPanel.Visibility = Visibility.Visible;
                    TwoStackPanel.Visibility = Visibility.Collapsed;
                });
            }catch (Exception ex)
            {
                LoadingGrid.Visibility = Visibility.Collapsed;
                ContentPanel.Visibility = Visibility.Collapsed;
                ErrorGrid.Visibility = Visibility.Visible;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = PathInstall.Text+ "\\MusicX.exe",
                UseShellExecute = true
            });

            Application.Current.Shutdown();

        }

        private void CreateLogDir()
        {
            var dir = Directory.CreateDirectory(PathInstall.Text + "\\logs");

            DirectorySecurity dSecurity = dir.GetAccessControl();
            dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
            dir.SetAccessControl(dSecurity);
        }

        private void CreateTempDir()
        {
            if(!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\musicx"))
            {
                var dir = Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\musicx");

            }
        }

        private void ChangePath_Click(object sender, RoutedEventArgs e)
        {
           
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            dialog.RootFolder = Environment.SpecialFolder.CommonProgramFiles;
            if (dialog.ShowDialog(this).GetValueOrDefault())
            {
                Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    PathInstall.Text = dialog.SelectedPath + "\\MusicX";

                });
            }
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                release = await GetLastRelease();
                ErrorGrid.Visibility = Visibility.Collapsed;

                this.ContentPanel.Visibility = Visibility.Visible;
                this.LoadingGrid.Visibility = Visibility.Collapsed;

                this.FirstStackPanel.Visibility = Visibility.Visible;
                this.TwoStackPanel.Visibility = Visibility.Collapsed;
                this.InstallButton.Visibility = Visibility.Visible;
                this.Version.Text = release.TagName;
            }catch (Exception ex)
            {
                LoadingGrid.Visibility = Visibility.Collapsed;
                ContentPanel.Visibility = Visibility.Collapsed;
                ErrorGrid.Visibility = Visibility.Visible;
            }
            
        }

        //C:\ProgramData\Microsoft\Windows\Start Menu\Programs
        private void AppShortcutToDesktop()
        {
            var wsh = new IWshShell_Class();
            IWshShortcut shortcut = wsh.CreateShortcut(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\Music X.lnk") as IWshRuntimeLibrary.IWshShortcut;
            shortcut.TargetPath = PathInstall.Text + "\\MusicX.exe";
            shortcut.Save();
        }

        private void AppShortcutToStart()
        {
            var wsh = new IWshShell_Class();
            IWshShortcut shortcut = wsh.CreateShortcut(
                "C:\\ProgramData\\Microsoft\\Windows\\Start Menu\\Programs" + "\\Music X.lnk") as IWshRuntimeLibrary.IWshShortcut;
            shortcut.TargetPath = PathInstall.Text + "\\MusicX.exe";
            shortcut.Save();
        }

        private void RegistryPath()
        {
            var subKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\MusicX", true);
            if(subKey == null)
            {
                var unistall = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall", true);
                var musicX = unistall.CreateSubKey("MusicX");
                
                this.CreateRegistryValues(musicX);
            }else
            {
                var unistall = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall", true);
                unistall.DeleteSubKeyTree("MusicX");

                var musicX = unistall.CreateSubKey("MusicX");

                this.CreateRegistryValues(musicX);
            }
        }

        private void CreateRegistryValues(RegistryKey musicX)
        {
            musicX.SetValue("Comments", "Music player for VKontakte", RegistryValueKind.String);
            musicX.SetValue("Contact", "Fooxboy", RegistryValueKind.String);
            musicX.SetValue("DisplayIcon", PathInstall.Text + "\\MusicX.exe", RegistryValueKind.String);
            musicX.SetValue("DisplayName", "Music X", RegistryValueKind.String);
            musicX.SetValue("DisplayVersion", Version.Text, RegistryValueKind.String);

            int memory = 0;
            DirectoryInfo di = new DirectoryInfo(PathInstall.Text);

            foreach (FileInfo file in di.GetFiles())
            {
                memory += (int)(file.Length / 1024);
            }
            musicX.SetValue("EstimatedSize", (int)memory, RegistryValueKind.DWord);
            musicX.SetValue("InstallLocation", PathInstall.Text, RegistryValueKind.String);
            musicX.SetValue("InstallSource", PathInstall.Text, RegistryValueKind.String);
            musicX.SetValue("Publisher", "Fooxboy", RegistryValueKind.String);
            musicX.SetValue("UninstallString", PathInstall.Text + "\\MusicX.Uninstaller.exe", RegistryValueKind.String);
            musicX.SetValue("URLInfoAbout", "https://t.me/MusicXPlayer", RegistryValueKind.String);
            musicX.SetValue("NoRepair", 1, RegistryValueKind.DWord);
            musicX.SetValue("NoModify", 1, RegistryValueKind.DWord);
            musicX.SetValue("HelpLink", "https://t.me/MusicXPlayer", RegistryValueKind.String);
        }
    }
}
