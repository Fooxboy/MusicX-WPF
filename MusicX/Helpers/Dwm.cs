using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Interop;
using Windows.Win32;
using Windows.Win32.Graphics.Dwm;

namespace MusicX.Helpers;

public static class Dwm
{
    public static void SuppressTitleBarColorization(this Window window)
    {
        var windowHandle = new WindowInteropHelper(window).EnsureHandle();
        
        var value = 0xFFFFFFFE;
        
        unsafe
        {
            PInvoke.DwmSetWindowAttribute(new(windowHandle), DWMWINDOWATTRIBUTE.DWMWA_CAPTION_COLOR, Unsafe.AsPointer(ref value), sizeof(int));
        }
    }
}