using System;
using System.Runtime.InteropServices;
using Windows.ApplicationModel.DataTransfer;
using WinRT;

namespace MusicX.Helpers;

public static class DataTransferManagerInterop
{
    private static IDataTransferManagerInterop InteropInstance => DataTransferManager.As<IDataTransferManagerInterop>();
    
    public static DataTransferManager GetForWindow(nint appWindow) => DataTransferManager.FromAbi(InteropInstance
        .GetForWindow(appWindow, new("A5CAEE9B-8708-49D1-8D36-67D25A8DA00C") /* Guid of IDataTransferManager */));
    
    public static void ShowForWindow(nint appWindow) => InteropInstance
        .ShowShareUIForWindow(appWindow);
    
    [ComImport, Guid("3A3DCD6C-3EAB-43DC-BCDE-45671CE800C8")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IDataTransferManagerInterop
    {
        nint GetForWindow([In] nint appWindow, [In] ref Guid riid);
        void ShowShareUIForWindow(nint appWindow);
    }
}