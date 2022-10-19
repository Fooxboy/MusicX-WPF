using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using Microsoft.AspNetCore.SignalR.Client;
using MusicX.Services.Player.Playlists;
using MusicX.Shared.Player;

namespace MusicX.Services;

public class ServerService : IDisposable
{
    private const string RootUrl = "https://localhost:7253";
    private readonly HubConnection _connection;
    private readonly IEnumerable<IDisposable> _subscriptions;

    private CancellationTokenSource? _source;

    public event Func<TimeSpan, bool, Task>? PlayStateChanged;
    private string _token = null!;

    public ServerService()
    {
        _connection = new HubConnectionBuilder()
                      .WithAutomaticReconnect()
                      .WithUrl($"{RootUrl}/hubs/listen", options => 
                                   options.AccessTokenProvider = () => Task.FromResult(_token)!)
                      .Build();

        _subscriptions = new[]
        {
            _connection.On<TimeSpan, bool>("PlayStateChanged",
                                           (pos, pause) => PlayStateChanged?.Invoke(pos, pause) ?? Task.CompletedTask)
        };
    }

    public async Task StartAsync(long userId)
    {
        using var client = new HttpClient
        {
            BaseAddress = new(RootUrl)
        };
        using var response = await client.PostAsJsonAsync("/token", userId);
        response.EnsureSuccessStatusCode();
        
        _token = await response.Content.ReadFromJsonAsync<string>() ?? throw new NullReferenceException("Got null response for token request");
        
        await _connection.StartAsync();
    }

    public Task StartSessionAsync()
    {
        return _connection.InvokeAsync("StartPlaySession");
    }

    public Task StopSessionAsync()
    {
        return _connection.InvokeAsync("StopPlaySession");
    }

    public async Task<IPlaylist> JoinSessionAsync(string sessionId)
    {
        if (_source is not null)
        {
            _source.Cancel();
            _source.Dispose();
        }

        _source = new();

        var playlist = new RemotePlaylist(_connection, _source.Token);
        
        await _connection.InvokeAsync("JoinPlaySession", sessionId);
        
        return playlist;
    }

    public Task LeaveSessionAsync(string sessionId)
    {
        if (_source is not null)
        {
            _source.Cancel();
            _source.Dispose();
        }

        _source = null;
        
        return _connection.InvokeAsync("LeavePlaySession", sessionId);
    }

    public Task OnTrackChangedAsync(PlaylistTrack track)
    {
        return _connection.InvokeAsync("ChangeTrack", track);
    }

    public Task OnPlayStateChangedAsync(TimeSpan position, bool pause)
    {
        return _connection.InvokeAsync("ChangePlayState", position, pause);
    }

    public void Dispose()
    {
        foreach (var subscription in _subscriptions)
        {
            subscription.Dispose();
        }
        _connection.StopAsync().SafeFireAndForget();
    }
}

public class RemotePlaylist : IPlaylist, IDisposable
{
    private readonly CancellationToken _cancellationToken;
    private readonly IDisposable _subscription;
    private readonly ChannelReader<PlaylistTrack> _reader;
    private readonly ChannelWriter<PlaylistTrack> _writer;

    public RemotePlaylist(HubConnection connection, CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
        var channel = Channel.CreateUnbounded<PlaylistTrack>();
        _reader = channel;
        _writer = channel;

        _subscription = connection.On<PlaylistTrack>("TrackChanged", track => _writer.WriteAsync(track).AsTask());
    }

    public bool CanLoad { get; private set; } = true;
    public async IAsyncEnumerable<PlaylistTrack> LoadAsync()
    {
        yield return await _reader.ReadAsync(_cancellationToken);
    }

    public void Dispose()
    {
        CanLoad = false;
        _subscription.Dispose();
        _writer.Complete();
    }
}