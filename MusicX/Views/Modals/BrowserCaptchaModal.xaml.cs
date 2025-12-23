using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json.Nodes;
using System.Windows;
using Microsoft.Web.WebView2.Core;
using MusicX.Controls;
using MusicX.ViewModels.Modals;

namespace MusicX.Views.Modals;

public partial class BrowserCaptchaModal : ModalPage
{
    public BrowserCaptchaModal()
    {
        InitializeComponent();
    }
    
    private void CaptchaModal_OnClosed(object sender, RoutedEventArgs e)
    {
        ((BrowserCaptchaModalViewModel)DataContext).CloseCommand.Execute(null);
    }

    private void WebView_OnCoreWebView2InitializationCompleted(object? sender, CoreWebView2InitializationCompletedEventArgs e)
    {
        if (!e.IsSuccess) return;

        var settings = WebView.CoreWebView2.Settings;

        settings.AreBrowserAcceleratorKeysEnabled = false;
        settings.AreDefaultContextMenusEnabled = false;
        settings.AreDefaultScriptDialogsEnabled = false;
#if !DEBUG
        settings.AreDevToolsEnabled = false;
        settings.IsBuiltInErrorPageEnabled = false;
#endif
        settings.IsGeneralAutofillEnabled = false;
        settings.IsPasswordAutosaveEnabled = false;
        settings.IsStatusBarEnabled = false;
        settings.IsWebMessageEnabled = false;
        settings.IsZoomControlEnabled = false;
        settings.UserAgent = "Mozilla/5.0 (Linux; Android 14; MusicX Build/UE1A.230829.036.A2; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/113.0.5672.136 Mobile Safari/537.36";

        WebView.CoreWebView2.WebResourceRequested += CoreWebView2OnWebResourceRequested;
        WebView.CoreWebView2.WebResourceResponseReceived += CoreWebView2OnWebResourceResponseReceived;  
        WebView.CoreWebView2.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.All,
            CoreWebView2WebResourceRequestSourceKinds.All);
    }

    private async void CoreWebView2OnWebResourceResponseReceived(object? sender, CoreWebView2WebResourceResponseReceivedEventArgs e)
    {
        try
        {
            Debug.WriteLine(
                $"Response {e.Request.Method} {e.Request.Uri} - {e.Response.StatusCode} {e.Response.ReasonPhrase}");
            if (e.Request.Uri.StartsWith("https://api.vk.com/method/captchaNotRobot.check",
                    StringComparison.OrdinalIgnoreCase))
            {
                await using var content = await e.Response.GetContentAsync();
                if (content is null) return;
                // cant pass stream directly to json parser 🎉
                // System.NotImplementedException: The method or operation is not implemented.
                //     at Microsoft.Web.WebView2.Core.COMStreamWrapper.Read(Byte[] buffer, Int32 offset, Int32 count)
                using var reader = new StreamReader(content);
                
                var node = JsonNode.Parse(await reader.ReadToEndAsync())?["response"];
                if (node?["status"]?.AsValue().TryGetValue(out string? status) is true && status == "OK" &&
                    node["success_token"]?.AsValue().TryGetValue(out string? successToken) is true)
                    _ = Dispatcher.InvokeAsync(() =>
                        ((BrowserCaptchaModalViewModel)DataContext).Complete(successToken));
            }
        }
        catch (Exception exception)
        {
            Debug.WriteLine(exception);
        }
    }

    private static void CoreWebView2OnWebResourceRequested(object? sender, CoreWebView2WebResourceRequestedEventArgs e)
    {
#if DEBUG
        if (e.ResourceContext == CoreWebView2WebResourceContext.XmlHttpRequest && e.Request.Content is not null)
        {
            using var reader = new StreamReader(e.Request.Content, leaveOpen: true);
            Debug.WriteLine($"Request {e.Request.Method} {e.Request.Uri} -- {reader.ReadToEnd()}");
        }
        else Debug.WriteLine($"Request {e.Request.Method} {e.Request.Uri}");
#endif
        e.Request.Headers.SetHeader("X-Requested-With", "com.vkontakte.android");
    }

    private void WebView_OnNavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        LoadingRing.Visibility = Visibility.Collapsed;
    }
}