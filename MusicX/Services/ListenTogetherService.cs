﻿using AsyncAwaitBestPractices;
using Microsoft.AspNetCore.SignalR.Client;
using MusicX.Models;
using MusicX.Models.Enums;
using MusicX.Shared.ListenTogether;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace MusicX.Services
{
    public class ListenTogetherService : IDisposable
    {
        private readonly Logger _logger;
        private HubConnection _connection;
        private string _host;
        private string _token;
        private IEnumerable<IDisposable> _subscriptions;

        /// <summary>
        /// Изменилась позиция трека
        /// </summary>
        public event Func<TimeSpan, bool, Task>? PlayStateChanged;

        /// <summary>
        /// Изменился трек
        /// </summary>
        public event Func<Track, Task>? TrackChanged;

        /// <summary>
        /// Текущий пользователь подключился как слушатель
        /// </summary>
        public event Func<Track, Task>? ConnectedToSession;

        /// <summary>
        /// Текущий пользователь запустил сессию
        /// </summary>
        public event Func<Task>? StartedSession;

        /// <summary>
        /// К сессии подключился слушатель
        /// </summary>
        public event Func<string, Task>? ListenerConnected;

        /// <summary>
        /// От сесси отключился слушатель
        /// </summary>
        public event Func<string, Task>? ListenerDisconnected;

        /// <summary>
        /// Владелец сессии закрыл её.
        /// </summary>
        public event Func<Task>? SessionOwnerStoped;

        /// <summary>
        /// Текущий пользователь закрыл сессию.
        /// </summary>
        public event Func<Task>? SessionStoped;

        public bool IsConnectedToServer => _connection is not null;

        public PlayerMode PlayerMode { get; set; } = PlayerMode.None;

        public string SessionId { get; set; }

        public ListenTogetherService(Logger logger)
        {
            _logger = logger;

            SessionOwnerStoped += SessionUserStoped;
        }


        public async Task<string> StartSessionAsync(long userId)
        {
            _logger.Info("Запуск сессии Слушать вместе");
            if (_connection is null)
            {
                await ConnectToServerAsync(userId);
            }

            var result = await _connection.InvokeAsync<string>(ListenTogetherMethods.StartPlaySession);

            if(result is null)
            {
                throw new Exception("Произошла ошибка при создании сессии");
            }

            PlayerMode = PlayerMode.Owner;

            SessionId = result;
            
            _logger.Info($"Сессия {SessionId} запущена");
            StartedSession?.Invoke();

            return result;
        }

        public async Task JoinToSesstionAsync(string session)
        {
            _logger.Info($"Подключение к сессии {session}");
            if (_connection is null)
            {
                throw new Exception("Невозможно подключится к сессии без подключения к серверу.");
            }

            var result = await _connection.InvokeAsync<bool>(ListenTogetherMethods.JoinPlaySession, session);

            if(!result)
            {
                throw new Exception("Ошибка при подключении к сессии.");
            }

            PlayerMode = PlayerMode.Listener;

            SessionId = session;

            var currentTrack = await GetCurrentTrackInSession();

            ConnectedToSession?.Invoke(currentTrack);

            _logger.Info($"Успешное подключение к сессии {session}");

        }

        public async Task ChangeTrackAsync(Track track)
        {
            _logger.Info($"Переключение трека в сессии {SessionId}");

            if (_connection is null)
            {
                throw new Exception("Невозможно выполнить запрос без подключения к серверу.");
            }

            var result = await _connection.InvokeAsync<bool>(ListenTogetherMethods.ChangeTrack, track);

            if (!result)
            {
                throw new Exception("Ошибка при выполнении запроса.");
            }
        }

        public async Task ChangePlayStateAsync(TimeSpan position, bool pause)
        {
            if (_connection is null)
            {
                throw new Exception("Невозможно выполнить запрос без подключения к серверу.");
            }

            var result = await _connection.InvokeAsync<bool>(ListenTogetherMethods.ChangePlayState, position, pause);

            if (!result)
            {
                throw new Exception("Ошибка при выполнении запроса.");
            }
        }

        public async Task StopPlaySessionAsync()
        {
            _logger.Info($"Остановка сессии {SessionId}");

            PlayerMode = PlayerMode.None;

            if (_connection is null)
            {
                throw new Exception("Невозможно выполнить запрос без подключения к серверу.");
            }

            var result = await _connection.InvokeAsync<bool>(ListenTogetherMethods.StopPlaySession);

            if (!result)
            {
                throw new Exception("Ошибка при выполнении запроса.");
            }

            SessionStoped?.Invoke();
        }

        public async Task LeavePlaySessionAsync()
        {
            _logger.Info($"Выход из сессии {SessionId}");

            PlayerMode = PlayerMode.None;

            if (_connection is null)
            {
                throw new Exception("Невозможно выполнить запрос без подключения к серверу.");
            }

            var result = await _connection.InvokeAsync<bool>(ListenTogetherMethods.LeavePlaySession);

            if (!result)
            {
                throw new Exception("Ошибка при выполнении запроса.");
            }
        }

        public async Task ConnectToServerAsync(long userId)
        {

            _logger.Info("Подключение к серверу Слушать вместе");

            var host = await GetListenTogetherHostAsync();
            this._host = host;

            var token = await GetTokenAsync(userId);

            this._token = token;

            _connection = new HubConnectionBuilder()
                    .WithAutomaticReconnect()
                    .WithUrl($"{_host}/hubs/listen", options =>
                                 options.AccessTokenProvider = () => Task.FromResult(_token)!)
            .Build();

            _subscriptions = new[]
            {
                _connection.On<TimeSpan, bool>(Callbacks.PlayStateChanged,
                                               (pos, pause) => PlayStateChanged?.Invoke(pos, pause) ?? Task.CompletedTask),

                _connection.On<Track>(Callbacks.TrackChanged,
                                               (track) => TrackChanged?.Invoke(track) ?? Task.CompletedTask),

                _connection.On<string>(Callbacks.ListenerConnected,
                                               (user) => ListenerConnected?.Invoke(user) ?? Task.CompletedTask),

                _connection.On(Callbacks.SessionStoped, ()=> SessionOwnerStoped?.Invoke() ?? Task.CompletedTask),

                _connection.On<string>(Callbacks.ListenerDisconnected,
                                               (user) => ListenerDisconnected?.Invoke(user) ?? Task.CompletedTask),
            };

            await _connection.StartAsync();

            _logger.Info("Успешное подключение");

        }

        public async Task<Track> GetCurrentTrackInSession()
        {
            _logger.Info("Получение текущего трека в сессии");

            if (_connection is null) throw new Exception("Сначала необходимо подключится к серверу");

            var result = await _connection.InvokeAsync<Track>(ListenTogetherMethods.GetCurrentTrack);

            return result;
        }


        private async Task<string> GetTokenAsync(long userId)
        {
            _logger.Info("Получение временного токена Слушать вместе");

            using var client = new HttpClient
            {
                BaseAddress = new(_host)
            };
            using var response = await client.PostAsJsonAsync("/token", userId);
            response.EnsureSuccessStatusCode();

            var token = await response.Content.ReadFromJsonAsync<string>() ?? throw new NullReferenceException("Got null response for token request");

            return token;
        }

        private async Task<string> GetListenTogetherHostAsync()
        {
            //return "https://localhost:5001";
            try
            {
                _logger.Info("Получение адресса сервера Послушать вместе");
                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.GetAsync("https://fooxboy.blob.core.windows.net/musicx/ListenTogetherServers.json");

                    var contents = await response.Content.ReadAsStringAsync();

                    var servers = JsonConvert.DeserializeObject<ListenTogetherServersModel>(contents);

                    //retrun "localhost";
#if DEBUG
                    return servers.Test;

#else
                return servers.Production;
#endif
                }
            }catch(Exception)
            {
                return "http://212.192.40.71:5000";
                //return "https://localhost:7253";
            }
            
        }

        private async Task SessionUserStoped()
        {
            await _connection.StopAsync();
            PlayerMode = PlayerMode.None;
            SessionId = null;
            _connection = null;
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
}
