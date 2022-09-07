using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VkNet.Abstractions;
using VkNet.Abstractions.Authorization;
using VkNet.Abstractions.Category;
using VkNet.Abstractions.Core;
using VkNet.Enums;
using VkNet.Model;
using VkNet.Utils;
using VkNet.Utils.AntiCaptcha;

namespace VkNet.Extensions.DependencyInjection;

public class VkApi : IVkApi
{
    private readonly IVkApiAuthAsync _auth;
    private readonly IVkApiInvoke _invoke;
    private readonly ILanguageService _languageService;
    private readonly IVkTokenStore _tokenStore;
    private readonly ICaptchaHandler _captchaHandler;
    private readonly IAuthorizationFlow _authorizationFlow;
    private readonly INeedValidationHandler _needValidationHandler;
    private readonly IVkApiVersionManager _vkApiVersion;

    public VkApi(IVkApiAuthAsync auth, IVkApiInvoke invoke, ILanguageService languageService, IUsersCategory users,
                 IFriendsCategory friends, IStatusCategory status, IMessagesCategory messages, IGroupsCategory groups,
                 IAudioCategory audio, IDatabaseCategory database, IUtilsCategory utils, IWallCategory wall,
                 IBoardCategory board, IFaveCategory fave, IVideoCategory video, IAccountCategory account,
                 IPhotoCategory photo, IDocsCategory docs, ILikesCategory likes, IPagesCategory pages,
                 IAppsCategory apps, INewsFeedCategory newsFeed, IStatsCategory stats, IGiftsCategory gifts,
                 IMarketsCategory markets, IAuthCategory authCategory, IExecuteCategory execute,
                 IPollsCategory pollsCategory,
                 ISearchCategory search, IStorageCategory storage, IAdsCategory ads,
                 INotificationsCategory notifications, IWidgetsCategory widgets, ILeadsCategory leads,
                 IStreamingCategory streaming, IPlacesCategory places, INotesCategory notes,
                 IAppWidgetsCategory appWidgets, IOrdersCategory orders, ISecureCategory secure,
                 IStoriesCategory stories, ILeadFormsCategory leadForms, IPrettyCardsCategory prettyCards,
                 IPodcastsCategory podcasts, IDonutCategory donut, IDownloadedGamesCategory downloadedGames,
                 IAuthorizationFlow authorizationFlow,
                 INeedValidationHandler needValidationHandler, IVkApiVersionManager vkApiVersion,
                 IVkTokenStore tokenStore, ICaptchaHandler captchaHandler,
                 ICaptchaSolver? captchaSolver = null)
    {
        _auth = auth;
        _invoke = invoke;
        _languageService = languageService;
        Users = users;
        Friends = friends;
        Status = status;
        Messages = messages;
        Groups = groups;
        Audio = audio;
        Database = database;
        Utils = utils;
        Wall = wall;
        Board = board;
        Fave = fave;
        Video = video;
        Account = account;
        Photo = photo;
        Docs = docs;
        Likes = likes;
        Pages = pages;
        Apps = apps;
        NewsFeed = newsFeed;
        Stats = stats;
        Gifts = gifts;
        Markets = markets;
        Auth = authCategory;
        Execute = execute;
        PollsCategory = pollsCategory;
        Search = search;
        Storage = storage;
        Ads = ads;
        Notifications = notifications;
        Widgets = widgets;
        Leads = leads;
        Streaming = streaming;
        Places = places;
        Notes = notes;
        AppWidgets = appWidgets;
        Orders = orders;
        Secure = secure;
        Stories = stories;
        LeadForms = leadForms;
        PrettyCards = prettyCards;
        Podcasts = podcasts;
        Donut = donut;
        DownloadedGames = downloadedGames;
        CaptchaSolver = captchaSolver;
        _authorizationFlow = authorizationFlow;
        _needValidationHandler = needValidationHandler;
        _vkApiVersion = vkApiVersion;
        _tokenStore = tokenStore;
        _captchaHandler = captchaHandler;
    }

    public void Dispose()
    {
    }

    public void Authorize(IApiAuthParams @params) =>
        _auth.Authorize(@params);

    public void Authorize(ApiAuthParams @params) =>
        _auth.Authorize(@params);

    public void RefreshToken(Func<string>? code = null) =>
        _auth.RefreshToken(code);

    public void LogOut() => _auth.LogOut();

    public bool IsAuthorized => _auth.IsAuthorized;

    public Task AuthorizeAsync(IApiAuthParams @params) =>
        _auth.AuthorizeAsync(@params);

    public Task RefreshTokenAsync(Func<string>? code = null) =>
        _auth.RefreshTokenAsync(code);

    public Task LogOutAsync() => _auth.LogOutAsync();

    public IUsersCategory Users { get; }
    public IFriendsCategory Friends { get; }
    public IStatusCategory Status { get; }
    public IMessagesCategory Messages { get; }
    public IGroupsCategory Groups { get; }
    public IAudioCategory Audio { get; }
    public IDatabaseCategory Database { get; }
    public IUtilsCategory Utils { get; }
    public IWallCategory Wall { get; }
    public IBoardCategory Board { get; }
    public IFaveCategory Fave { get; }
    public IVideoCategory Video { get; }
    public IAccountCategory Account { get; }
    public IPhotoCategory Photo { get; }
    public IDocsCategory Docs { get; }
    public ILikesCategory Likes { get; }
    public IPagesCategory Pages { get; }
    public IAppsCategory Apps { get; }
    public INewsFeedCategory NewsFeed { get; }
    public IStatsCategory Stats { get; }
    public IGiftsCategory Gifts { get; }
    public IMarketsCategory Markets { get; }
    public IAuthCategory Auth { get; }
    public IExecuteCategory Execute { get; }
    public IPollsCategory PollsCategory { get; }
    public ISearchCategory Search { get; }
    public IStorageCategory Storage { get; }
    public IAdsCategory Ads { get; }
    public INotificationsCategory Notifications { get; }
    public IWidgetsCategory Widgets { get; }
    public ILeadsCategory Leads { get; }
    public IStreamingCategory Streaming { get; }
    public IPlacesCategory Places { get; }
    public INotesCategory Notes { get; set; }
    public IAppWidgetsCategory AppWidgets { get; set; }
    public IOrdersCategory Orders { get; set; }
    public ISecureCategory Secure { get; set; }
    public IStoriesCategory Stories { get; set; }
    public ILeadFormsCategory LeadForms { get; set; }
    public IPrettyCardsCategory PrettyCards { get; set; }
    public IPodcastsCategory Podcasts { get; set; }
    public IDonutCategory Donut { get; }
    public IDownloadedGamesCategory DownloadedGames { get; }
    public ICaptchaSolver? CaptchaSolver { get; }

    public int MaxCaptchaRecognitionCount
    {
        get => _captchaHandler.MaxCaptchaRecognitionCount;
        set => _captchaHandler.MaxCaptchaRecognitionCount = value;
    }

    public VkResponse Call(string methodName, VkParameters parameters, bool skipAuthorization = false) =>
        _invoke.Call(methodName, parameters, skipAuthorization);

    public T Call<T>(string methodName, VkParameters parameters, bool skipAuthorization = false,
                     params JsonConverter[] jsonConverters) =>
        _invoke.Call<T>(methodName, parameters, skipAuthorization, jsonConverters);

    public Task<VkResponse> CallAsync(string methodName, VkParameters parameters, bool skipAuthorization = false) =>
        _invoke.CallAsync(methodName, parameters, skipAuthorization);

    public Task<T> CallAsync<T>(string methodName, VkParameters parameters, bool skipAuthorization = false) =>
        _invoke.CallAsync<T>(methodName, parameters, skipAuthorization);

    public string Invoke(string methodName, IDictionary<string, string> parameters, bool skipAuthorization = false) =>
        _invoke.Invoke(methodName, parameters, skipAuthorization);

    public Task<string> InvokeAsync(string methodName, IDictionary<string, string> parameters,
                                    bool skipAuthorization = false) =>
        _invoke.InvokeAsync(methodName, parameters, skipAuthorization);

    public DateTimeOffset? LastInvokeTime => _invoke.LastInvokeTime;
    public TimeSpan? LastInvokeTimeSpan => _invoke.LastInvokeTimeSpan;
    
    public VkResponse CallLongPoll(string server, VkParameters parameters)
    {
        throw new NotImplementedException();
    }

    public Task<VkResponse> CallLongPollAsync(string server, VkParameters parameters)
    {
        throw new NotImplementedException();
    }

    public string InvokeLongPoll(string server, Dictionary<string, string> parameters)
    {
        throw new NotImplementedException();
    }

    public JObject InvokeLongPollExtended(string server, Dictionary<string, string> parameters)
    {
        throw new NotImplementedException();
    }

    public Task<string> InvokeLongPollAsync(string server, Dictionary<string, string> parameters)
    {
        throw new NotImplementedException();
    }

    public Task<JObject> InvokeLongPollExtendedAsync(string server, Dictionary<string, string> parameters)
    {
        throw new NotImplementedException();
    }

    public void SetLanguage(Language language) =>
        _languageService.SetLanguage(language);

    public Language? GetLanguage() =>
        _languageService.GetLanguage();

    public void Validate(string validateUrl) =>
        _needValidationHandler.ValidateAsync(validateUrl).GetAwaiter().GetResult();

    public int RequestsPerSecond { get; set; }

    [Obsolete]
    public IBrowser Browser
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public IAuthorizationFlow AuthorizationFlow
    {
        get => _authorizationFlow;
        set => throw new NotSupportedException("Container is immutable");
    }

    public INeedValidationHandler NeedValidationHandler
    {
        get => _needValidationHandler;
        set => throw new NotSupportedException("Container is immutable");
    }

    public IVkApiVersionManager VkApiVersion
    {
        get => _vkApiVersion;
        set => throw new NotSupportedException("Container is immutable");
    }

    public string Token => _tokenStore.Token;
    public long? UserId { get; set; }
    public event VkApiDelegate? OnTokenExpires
    {
        add => throw new NotImplementedException();
        remove => throw new NotImplementedException();
    }

    public event VkApiDelegate? OnTokenUpdatedAutomatically
    {
        add => throw new NotImplementedException();
        remove => throw new NotImplementedException();
    }
}