using System;
using System.Net;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Navigation;
using AsyncAwaitBestPractices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using MusicX.Services;
using MusicX.ViewModels;
using NavigationService = System.Windows.Navigation.NavigationService;

namespace MusicX.Views;

public partial class MiniAppView : Page, IProvideCustomContentState
{
    private MiniAppViewModel ViewModel { get; }
    
    public MiniAppView(MiniAppViewModel viewModel)
    {
        DataContext = ViewModel = viewModel;
        InitializeComponent();
    }

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);
        
        LoadAsync().SafeFireAndForget();
    }

    private async Task LoadAsync()
    {
        await WebView.EnsureCoreWebView2Async();

        var settings = WebView.CoreWebView2.Settings;
#if !DEBUG
        settings.AreDevToolsEnabled = false;
        settings.IsStatusBarEnabled = false;
        settings.AreDefaultContextMenusEnabled = false;
        settings.AreBrowserAcceleratorKeysEnabled = false;
        settings.AreDefaultScriptDialogsEnabled = false;
#endif
        settings.IsPasswordAutosaveEnabled = false;
        settings.IsGeneralAutofillEnabled = false;
        settings.IsZoomControlEnabled = false;
        settings.IsBuiltInErrorPageEnabled = false;

        settings.UserAgent = "VKAndroidApp/8.99-23423 (Android 12; SDK 32; arm64-v8a; MusicX; ru; 2960x1440)";

        var bridgeService = StaticService.Container.GetRequiredService<VkBridgeService>();

        bridgeService.Load(ViewModel.AppId, ViewModel.Url);
        
        WebView.CoreWebView2.AddHostObjectToScript("bridge", bridgeService);

        await WebView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(
"""
const requestPropsMap = {
  VKWebAppInit: () => [],
  VKWebAppAddToCommunity: () => [],
  VKWebAppAddToHomeScreen: () => [],
  VKWebAppAddToHomeScreenInfo: () => [],
  VKWebAppAllowMessagesFromGroup: ({group_id, key}) => [group_id, key], // { group_id: number; key?: string }
  VKWebAppAllowNotifications: () => [],
  OKWebAppCallAPIMethod: () => [], // { method: string; params: OKCallApiParams };
  VKWebAppCallAPIMethod: ({method, params}) => [method, JSON.stringify(params)], /* {
    method: string;
    params: Record<'access_token' | 'v', string> & Record<string, string | number>;
  }; */
  VKWebAppCopyText: ({text}) => [text], // { text: string };
  VKWebAppCreateHash: ({payload}) => [payload], // { payload: string };
  VKWebAppDownloadFile: ({url, filename}) => [url, filename], // { url: string; filename: string };
  VKWebAppGetAuthToken: ({app_id, scope}) => [app_id, scope], // { app_id: number; scope: PersonalAuthScope | string };
  VKWebAppClose: () => [status, JSON.stringify(payload)], // { status: AppCloseStatus; payload?: any };
  VKWebAppOpenApp: ({app_id, location}) => [app_id, location], // { app_id: number; location?: string };
  VKWebAppDenyNotifications: () => [],
  VKWebAppFlashGetInfo: () => [],
  VKWebAppFlashSetLevel: ({level}) => [level], // { level: number };
  VKWebAppGetClientVersion: () => [],
  VKWebAppGetCommunityToken: ({app_id, group_id, scope}) => [app_id, group_id, scope], // { app_id: number, group_id: number, scope: CommunityAuthScope | string };
  VKWebAppGetConfig: () => [],
  VKWebAppGetLaunchParams: () => [],
  VKWebAppAudioPause: () => [],
  VKWebAppGetEmail: () => [],
  VKWebAppGetFriends: ({multi}) => [multi], // { multi?: boolean };
  VKWebAppGetGeodata: () => [],
  VKWebAppGetGrantedPermissions: () => [],
  VKWebAppGetPersonalCard: ({type}) => [type], // { type: PersonalCardType[] };
  VKWebAppGetPhoneNumber: () => [],
  VKWebAppGetUserInfo: ({user_id, user_ids}) => [user_id, user_ids], // { user_id?: number; user_ids?: string };
  VKWebAppJoinGroup: ({group_id}) => [group_id], // { group_id: number };
  VKWebAppLeaveGroup: ({group_id}) => [group_id], // { group_id: number };
  VKWebAppAddToMenu: () => [],
  VKWebAppOpenCodeReader: () => [],
  VKWebAppOpenContacts: () => [],
  VKWebAppOpenPayForm: () => [], // VKPayProps<VKPayActionType>; // not planned
  VKWebAppOpenQR: () => [],
  VKWebAppResizeWindow: ({width, height}) => [width, height], // { width: number; height?: number };
  VKWebAppScroll: ({top, speed}) => [top, speed], // { top: number; speed?: number };
  VKWebAppSendToClient: ({fragment}) => [fragment], // { fragment?: string };
  VKWebAppSetLocation: ({location, replace_state}) => [location, replace_state], // { location: string; replace_state?: boolean };
  VKWebAppSetViewSettings: ({status_bar_style, action_bar_color, navigation_bar_color}) => [status_bar_style, action_bar_color, navigation_bar_color], // { status_bar_style: AppearanceType; /** Android only */ action_bar_color?: 'none' | string; /** Android only */ navigation_bar_color?: string; };
  VKWebAppShare: ({link}) => [link], // { link?: string };
  VKWebAppShowCommunityWidgetPreviewBox: ({type, group_id, code}) => [type, group_id, code], // { type: CommunityWidgetType | string, group_id: number, /* execute method code */ code: string };
  VKWebAppShowImages: ({images, start_index}) => [images, start_index], // { images: string[]; start_index?: number };
  VKWebAppShowInviteBox: () => [],
  VKWebAppShowLeaderBoardBox: ({user_result}) => [user_result], // { user_result: number };
  VKWebAppShowMessageBox: () => [], // MessageRequestOptions; // not supported rn
  VKWebAppCheckBannerAd: () => [],
  VKWebAppHideBannerAd: () => [],
  VKWebAppShowBannerAd: () => [], // ShowBannerAdRequest;
  VKWebAppShowNativeAds: () => [], // ShowNativeAdsRequest;
  VKWebAppCheckNativeAds:() => [], // CheckNativeAdsRequest;
  VKWebAppShowOrderBox:() => [], // OrderRequestOptions;
  VKWebAppShowRequestBox:() => [], // RequestForRequestOptions;
  VKWebAppShowWallPostBox:() => [], // WallPostRequestOptions;
  VKWebAppShowSubscriptionBox:() => [], // ShowSubscriptionBoxRequest;
  VKWebAppOpenWallPost: ({post_id, owner_id}) => [post_id, owner_id], // { post_id: number; owner_id: number };
  VKWebAppStorageGet: ({keys}) => [keys], // { keys: string[] };
  VKWebAppStorageGetKeys: ({count, offset}) => [count, offset], // { count: number; offset: number };
  VKWebAppStorageSet: ({key, value}) => [key, value], // { key: string; value: string };
  VKWebAppTapticImpactOccurred: ({style}) => [style], // { style: TapticVibrationPowerType };
  VKWebAppTapticNotificationOccurred: ({type}) => [type], // { type: TapticNotificationType };
  VKWebAppTapticSelectionChanged: () => [],
  VKWebAppAddToFavorites: () => [],
  VKWebAppSendPayload: ({group_id, payload}) => [group_id, payload], // { group_id: number; payload: any };
  VKWebAppDisableSwipeBack: () => [],
  VKWebAppEnableSwipeBack: () => [],
  VKWebAppSetSwipeSettings: ({history}) => [history], // { history: boolean };
  VKWebAppShowStoryBox: () => [], // ShowStoryBoxOptions; // not supported rn
  VKWebAppAccelerometerStart: ({refresh_rate}) => [refresh_rate], // { refresh_rate?: string };
  VKWebAppAccelerometerStop: () => [],
  VKWebAppGyroscopeStart: () => [],
  VKWebAppGyroscopeStop: () => [],
  VKWebAppDeviceMotionStart: () => [],
  VKWebAppDeviceMotionStop: () => [],
  VKWebAppSubscribeStoryApp: () => [], // SubscribeStoryAppOptions; // not supported rn
  VKWebAppGetGroupInfo: ({group_id}) => [group_id], // { group_id: number };
  VKWebAppLibverifyRequest: () => [], // { phone: string }; // not supported rn
  VKWebAppLibverifyCheck: () => [], // { code: string }; // not supported rn
  VKWebAppRetargetingPixel: () => [], // RetargetingPixelOptions; // ???
  VKWebAppCheckAllowedScopes: ({scopes}) => [scopes], // { scopes: string };
  VKWebAppConversionHit: () => [], // ConversionHitRequest;
  VKWebAppCheckSurvey: () => [],
  VKWebAppShowSurvey: () => [],
  VKWebAppScrollTop: () => [],
  VKWebAppScrollTopStart: () => [],
  VKWebAppScrollTopStop: () => [],
  VKWebAppShowSlidesSheet: () => [], // ShowSlidesSheetRequest;
  VKWebAppTranslate: () => [], // TranslateRequest;
  VKWebAppCallStart: () => [],
  VKWebAppCallJoin: () => [], // CallJoinRequest;
  VKWebAppCallGetStatus: () => [],
  VKWebAppRecommend: () => [],
  VKWebAppAddToProfile: () => [], // AddToProfileRequest;
  SetSupportedHandlers: () => [],
  VKWebAppTrackEvent: () => [], // TrackEventRequest;
}

window.ReactNativeWebView = {
    postMessage: async (message) => {
        try {
            const { handler, params } = JSON.parse(message)

            const argsMapper = requestPropsMap[handler]

            if (!argsMapper) return

            if ('request_id' in params) {
                await window.chrome.webview.hostObjects.bridge.SetNextRequestId(params.request_id)
            }
            
            const fn = await window.chrome.webview.hostObjects.bridge.getHostProperty(handler)
            return await fn.applyHostFunction(argsMapper(params))
        } catch (e) {
            console.error(`Error calling bridge ${handler} with params ${JSON.stringify(params)}`)
            console.error(e)
        }
    }
}

document.addEventListener('message', (event) => {
    console.log(`VKWebAppEvent ${event?.data}`)
})

window.chrome.webview.hostObjects.bridge.addEventListener('VKWebAppEvent', (event) => {
    const ev = new Event('message')
    ev.data = event
    document.dispatchEvent(ev)
})                                                                            
""");

#if DEBUG
        await WebView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync("chrome.webview.hostObjects.options.log = console.log.bind(console);");
#endif

        WebView.SetBinding(WebView2.SourceProperty, new Binding("Url"));
    }
    
    [Serializable]
    private class MiniAppState : CustomContentState, ISerializable, IDisposable
    {
        private const string AppIdKey = "AppId";
        private const string UrlKey = "Url";

        public override string JournalEntryName => _viewModel.Url;

        private MiniAppViewModel _viewModel;
        public MiniAppState(MiniAppViewModel viewModel)
        {
            _viewModel = viewModel;
        }
        
        public MiniAppState(SerializationInfo info, StreamingContext context)
        {
            var appId = info.GetString(AppIdKey) ?? throw new SerializationException($"{AppIdKey} is required value");
            var url = info.GetString(UrlKey) ?? throw new SerializationException($"{UrlKey} is required value");
            
            _viewModel = new(appId, url);
        }
        
        public override void Replay(NavigationService navigationService, NavigationMode mode)
        {
            if (navigationService.Content is MiniAppView section)
                section.DataContext = _viewModel;
            else
                navigationService.Navigate(new MiniAppView(_viewModel));
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(AppIdKey, _viewModel.AppId);
            info.AddValue(UrlKey, _viewModel.Url);
        }

        public void Dispose()
        {
        }
    }

    public CustomContentState GetContentState()
    {
        return new MiniAppState((MiniAppViewModel)DataContext);
    }

    private void WebView_OnNavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e)
    {
        ViewModel.IsLoading = true;
    }

    private void WebView_OnNavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        ViewModel.IsLoading = false;

        if (e.IsSuccess || e.WebErrorStatus == CoreWebView2WebErrorStatus.OperationCanceled) return;
        
        ViewModel.ErrorMessage = "Произошла ошибка при загрузке страницы";
        ViewModel.ErrorDetails = e.HttpStatusCode == 0
            ? e.WebErrorStatus.ToString()
            : $"Код ошибки сервера: {(HttpStatusCode)e.HttpStatusCode}";
    }

    private void WebView_OnWebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        
    }
}