﻿using System.Collections.Immutable;
using MusicX.Core.Helpers;
using MusicX.Core.Models;
using MusicX.Core.Models.General;
using Newtonsoft.Json;
using NLog;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using VkNet.Abstractions;
using VkNet.Abstractions.Core;
using VkNet.AudioBypassService.Abstractions;
using VkNet.Enums.Filters;
using VkNet.Exception;
using VkNet.Extensions.DependencyInjection;
using VkNet.Model;
using VkNet.Utils;
using Lyrics = MusicX.Core.Models.Lyrics;
using MusicX.Core.Models.Mix;
using VkNet.Abstractions.Utils;

namespace MusicX.Core.Services
{
    public class VkService
    {

        public readonly IVkApiCategories vkApi;
        private readonly IVkApiInvoke apiInvoke;
        private readonly Logger logger;
        private readonly string vkApiVersion = "5.243";

        public bool IsAuth = false;
        private readonly IVkTokenStore tokenStore;
        private readonly IVkApiAuthAsync auth;
        private readonly IVkApi _api;
        private readonly ICustomSectionsService _customSectionsService;
        private readonly IDeviceIdStore _deviceIdStore;
        private readonly ITokenRefreshHandler _tokenRefreshHandler;
        private readonly IRestClient _restClient;

        public VkService(Logger logger, IVkApiCategories vkApi, IVkApiInvoke apiInvoke, IVkApiVersionManager versionManager,
                         IVkTokenStore tokenStore, IVkApiAuthAsync auth, IVkApi api, ICustomSectionsService customSectionsService, IDeviceIdStore deviceIdStore, ITokenRefreshHandler tokenRefreshHandler, IRestClient restClient)
        {
            this.vkApi = vkApi;
            this.apiInvoke = apiInvoke;
            this.tokenStore = tokenStore;
            this.auth = auth;
            _api = api;
            _customSectionsService = customSectionsService;
            _deviceIdStore = deviceIdStore;
            _tokenRefreshHandler = tokenRefreshHandler;
            _restClient = restClient;

            var ver = vkApiVersion.Split('.');
            versionManager.SetVersion(int.Parse(ver[0]), int.Parse(ver[1]));

            var log = LogManager.Setup().GetLogger("Common");

            if (logger == null) this.logger = log;
            else this.logger = logger;
        }

        public async Task<string> AuthAsync(string login, string password, Func<string> twoFactorAuth)
        {
            try
            {
                logger.Info("Invoke auth with login and password");

                await auth.AuthorizeAsync(new ApiAuthParams()
                {
                    Login = login,
                    Password = password,
                    TwoFactorAuthorization = twoFactorAuth
                });


                var user = await vkApi.Users.GetAsync(new List<long>());
                _api.UserId = user[0].Id;
                IsAuth = true;

                logger.Info($"User '{user[0].Id}' successful sign in");


                return tokenStore.Token;
            }catch(Exception ex)
            {
                logger.Error("VK API ERROR:");
                logger.Error(ex, ex.Message);
                throw;
            }
           
        }

        public async Task SetTokenAsync(string token)
        {
            try
            {
                logger.Info("Set user token");

                await auth.AuthorizeAsync(new ApiAuthParams()
                {
                    AccessToken = token
                });

                try
                {
                    var user = await vkApi.Users.GetAsync(new List<long>());
                    _api.UserId = user[0].Id;
                    logger.Info($"User '{user[0].Id}' successful sign in");
                }
                catch (VkApiMethodInvokeException e) when (e.ErrorCode == 1117) // token has expired
                {
                    if (await _tokenRefreshHandler.RefreshTokenAsync(token) is null)
                        throw;
                }

                IsAuth = true;
            }catch(Exception ex)
            {
                logger.Error("VK API ERROR:");
                logger.Error(ex, ex.Message);
                throw;
            }
           

        }

        public async Task<ResponseData> GetAudioCatalogAsync(string url = null)
        {
            try
            {
                logger.Info("Invoke 'catalog.getAudio' ");

                var parameters = new VkParameters
                {
                    
                    {"extended", "1"},
                    
                    {"device_id", await _deviceIdStore.GetDeviceIdAsync()}
                };

                if(url != null)
                {
                    parameters.Add("url", url);
                }

                var model = await apiInvoke.CallAsync<ResponseData>("catalog.getAudio", parameters);

                logger.Info("Successful invoke 'catalog.getAudio' ");

                IIdentifiable.Process(model);
                return model;
            }catch(Exception ex)
            {
                logger.Error("VK API ERROR:");
                logger.Error(ex, ex.Message);
                throw;
            }
            

        }

        public async ValueTask<ResponseData> GetSectionAsync(string sectionId, string startFrom = null)
        {
            try
            {
                var model = await _customSectionsService.HandleSectionRequest(sectionId, startFrom);

                if (model != null)
                    return model;
                
                logger.Info($"Invoke 'catalog.getSection' with sectionId = '{sectionId}' ");

                var parameters = new VkParameters
                {
                    
                    {"extended", "1"},
                    
                    {"device_id", await _deviceIdStore.GetDeviceIdAsync()},
                    {"section_id", sectionId },
                    {"need_blocks", 1 },
                };

                if (startFrom != null) parameters.Add("start_from", startFrom);


                model = await apiInvoke.CallAsync<ResponseData>("catalog.getSection", parameters);

                 logger.Info("Successful invoke 'catalog.getSection' ");


                IIdentifiable.Process(model);
                return model;
            }catch (Exception ex)
            {
                logger.Error("VK API ERROR:");
                logger.Error(ex, ex.Message);
                throw;
            }
        }

        public async Task<ResponseData> GetBlockItemsAsync(string blockId)
        {
            try
            {
                logger.Info($"Invoke 'catalog.getBlockItems' with blockId = '{blockId}' ");

                var parameters = new VkParameters
                {
                    
                    {"extended", "1"},
                    {"device_id", await _deviceIdStore.GetDeviceIdAsync()},
                    
                    {"block_id", blockId },
                };


                var model = await apiInvoke.CallAsync<ResponseData>("catalog.getBlockItems", parameters);

                logger.Info("Successful invoke 'catalog.getBlockItems' ");

                IIdentifiable.Process(model);
                return model;
            }catch(Exception ex)
            {
                logger.Error("VK API ERROR:");
                logger.Error(ex, ex.Message);
                throw;
            }
        }

        public async Task<ResponseData> GetAudioSearchAsync(string query = null, string context= null)
        {
            try
            {
                logger.Info($"Invoke 'catalog.getAudioSearch' with query = '{query}' ");

                var parameters = new VkParameters
                {
                    
                    {"extended", "1"},
                    
                    {"device_id", await _deviceIdStore.GetDeviceIdAsync()},
                };

                if (query != null)
                {
                    parameters.Add("query", query);
                }
                else parameters.Add("need_blocks", "1");
                if (context != null) parameters.Add("context", context);


                var model = await apiInvoke.CallAsync<ResponseData>("catalog.getAudioSearch", parameters);
                logger.Info("Successful invoke 'catalog.getAudioSearch' ");

                IIdentifiable.Process(model);
                return model;
            }catch(Exception ex)
            {
                logger.Error("VK API ERROR:");
                logger.Error(ex, ex.Message);
                throw;
            }
        }

        public async Task<ResponseData> GetAudioArtistAsync(string artistId)
        {
            try
            {
                logger.Info($"Invoke 'catalog.getAudioArtist' with artistId = '{artistId}' ");

                var parameters = new VkParameters
                {
                    
                    {"extended", "1"},
                    
                    {"device_id", await _deviceIdStore.GetDeviceIdAsync()},
                    {"artist_id", artistId}

                };


                var model = await apiInvoke.CallAsync<ResponseData>("catalog.getAudioArtist", parameters);

                logger.Info("Successful invoke 'catalog.getAudioArtist' ");


                IIdentifiable.Process(model);
                return model;
            }catch (Exception ex)
            {
                logger.Error("VK API ERROR:");
                logger.Error(ex, ex.Message);
                throw;
            }
        }

        public async Task<ResponseData> GetAudioCuratorAsync(string curatorId, string url)
        {
            try
            {
                logger.Info($"Invoke 'catalog.getAudioCurator' with curatorId = '{curatorId}' ");

                var parameters = new VkParameters
                {
                    
                    {"extended", "1"},
                    {"device_id", await _deviceIdStore.GetDeviceIdAsync()},
                    
                    {"curator_id", curatorId},
                    {"url", url}
                };


                var model = await apiInvoke.CallAsync<ResponseData>("catalog.getAudioCurator", parameters);

                logger.Info("Successful invoke 'catalog.getAudioCurator' ");


                IIdentifiable.Process(model);
                return model;
            }catch(Exception ex)
            {
                logger.Error("VK API ERROR:");
                logger.Error(ex, ex.Message);
                throw;
            }
        }

        public async Task<ResponseData> GetPlaylistAsync(int count, long albumId, string accessKey, long ownerId, int offset = 0, int needOwner = 1)
        {
            try
            {
                logger.Info($"Invoke 'execute.getPlaylist' with albumId = '{albumId}' ");

                var parameters = new VkParameters
                {
                    {"v", vkApiVersion },

                    {"lang", "ru"},
                    {"audio_count", count },
                    {"need_playlist", 1 },
                    {"owner_id", ownerId},
                    {"access_key", accessKey},
                    {"func_v", 10 },
                    {"id", albumId},
                    {"audio_offset", offset },
                    
                    {"count", "10"},
                    {"need_owner", needOwner }
                };

                var model = await apiInvoke.CallAsync<ResponseData>("execute.getPlaylist", parameters);

                logger.Info("Successful invoke 'execute.getPlaylist' ");


                IIdentifiable.Process(model);
                return model;
            }catch(Exception ex)
            {
                logger.Error("VK API ERROR:");
                logger.Error(ex, ex.Message);
                throw;
            }

        }

        public async Task AudioAddAsync(long audioId, long ownerId)
        {
            try
            {
                logger.Info($"Invoke 'audio.add' with audioId = '{audioId}' and ownerId = '{ownerId}' ");

                var parameters = new VkParameters
                {
                    
                    
                    {"audio_id", audioId},
                    {"owner_id", ownerId}
                };


                var json = await apiInvoke.InvokeAsync("audio.add", parameters);
                logger.Debug("RESULT OF 'audio.add'" + json);


                logger.Info("Successful invoke 'audio.add' ");
            }catch(Exception ex)
            {
                logger.Error("VK API ERROR:");
                logger.Error(ex, ex.Message);
                throw;
            }
        }

        public async Task AudioDeleteAsync(long audioId, long ownerId)
        {
            try
            {
                logger.Info($"audio.delete' with audioId = '{audioId}' and ownerId = '{ownerId}' ");

                var parameters = new VkParameters
                {
                    
                    
                    {"audio_id", audioId},
                    {"owner_id", ownerId}
                };


                var json = await apiInvoke.InvokeAsync("audio.delete", parameters);

                logger.Debug("RESULT OF 'audio.delete'" + json);


                logger.Info("Successful invoke 'audio.delete' ");
            }catch(Exception ex)
            {
                logger.Error("VK API ERROR:");
                logger.Error(ex, ex.Message);
                throw;
            }
        }

        public async Task<User> GetCurrentUserAsync()
        {
            try
            {
                var users = await vkApi.Users.GetAsync(new List<long>(), ProfileFields.Photo200);
                var currentUser = users?.FirstOrDefault();
                return currentUser;
            }catch(Exception ex)
            {
                logger.Error("VK API ERROR:");
                logger.Error(ex, ex.Message);
                throw;
            }
           
        }

        public async Task<User> GetUserAsync(long userId)
        {
            try
            {
                var users = await vkApi.Users.GetAsync(new List<long> { userId }, ProfileFields.Photo200);
                var currentUser = users?.FirstOrDefault();
                return currentUser;
            }catch (Exception ex)
            {
                logger.Error("VK API ERROR:");
                logger.Error(ex, ex.Message);
                throw;
            }
           
        }

        public async Task<Owner> OwnerAsync(long ownerId)
        {
            try
            {
                if (ownerId > 0)
                {
                    var users = await vkApi.Users.GetAsync(new List<long>() { ownerId });
                    var user = users?.FirstOrDefault();
                    if (user != null)
                    {
                        var owner = new Owner()
                        {
                            Id = user.Id,
                            Name = user.LastName + " " + user.FirstName
                        };
                        return owner;
                    }
                    else return null;
                }
                else
                {
                    ownerId *= (-1);
                    var groups = await vkApi.Groups.GetByIdAsync(new List<string>() { ownerId.ToString() }, "",
                        GroupsFields.Description);
                    var group = groups?.FirstOrDefault();
                    if (group != null)
                    {
                        var owner = new Owner()
                        {
                            Id = ownerId,
                            Name = group.Name,
                        };
                        return owner;
                    }
                    else return null;
                }
            }catch (Exception ex)
            {
                logger.Error("VK API ERROR:");
                logger.Error(ex, ex.Message);
                throw;
            }
            

        }

        public async Task AddPlaylistAsync(long playlistId, long ownerId, string accessKey)
        {
            try
            {
                logger.Info($"audio.followPlaylist' with playlistId = '{playlistId}' and ownerId = '{ownerId}' and accessKey = {accessKey} ");

                var parameters = new VkParameters
                {
                    
                    
                    {"playlist_id", playlistId},
                    {"owner_id", ownerId},
                    
                };

                if (accessKey != null) parameters.Add("access_key", accessKey);

                var json = await apiInvoke.InvokeAsync("audio.followPlaylist", parameters);

                logger.Debug("RESULT OF 'audio.followPlaylist'" + json);


                logger.Info("Successful invoke 'audio.followPlaylist' ");
            }catch(Exception ex)
            {
                logger.Error("VK API ERROR:");
                logger.Error(ex, ex.Message);
                throw;
            }
            

        }

        public async Task DeletePlaylistAsync(long playlistId, long ownerId)
        {
            try
            {
                var result = await vkApi.Audio.DeletePlaylistAsync(ownerId, playlistId);

            }catch(Exception ex)
            {
                logger.Error("VK API ERROR:");
                logger.Error(ex, ex.Message);
                throw;
            }
        }

        public async Task<ResponseData> AudioGetAsync(long? playlistId, long? ownerId, string? assessKey, long offset = 0, long count = 100, int? shuffleSeed = null)
        {
            try
            {
                logger.Info($"invoke 'audio.get' with playlistId = '{playlistId}' and ownerId = '{ownerId}' and accessKey = {assessKey} ");


                var parameters = new VkParameters
                {
                    
                    
                    {"offset", offset },
                    {"count", count }
                };

                if(playlistId != null)
                {
                    parameters.Add("playlist_id", playlistId);
                }

                if (ownerId != null)
                {
                    parameters.Add("owner_id", ownerId);
                }

                if (assessKey != null)
                {
                    parameters.Add("access_key", assessKey);
                }

                if (shuffleSeed != null)
                {
                    parameters.Add("shuffle_seed", shuffleSeed);
                }

                var model = await apiInvoke.CallAsync<ResponseData>("audio.get", parameters);

                logger.Info("Successful invoke 'audio.get' ");

                return model;
                //vkApi.Audio.GetAsync(new VkNet.Model.RequestParams.AudioGetParams() { })
            }catch(Exception ex)
            {
                logger.Error("VK API ERROR:");
                logger.Error(ex, ex.Message);
                throw;
            }


        }

        public async Task<ResponseData> ReplaceBlockAsync(string replaceId)
        {
            try
            {
                logger.Info($"invoke 'catalog.replaceBlocks' with replaceId = {replaceId} ");

                var parameters = new VkParameters
                {
                    
                    {"device_id", await _deviceIdStore.GetDeviceIdAsync()},
                    
                    {"replacement_ids", replaceId},
                };



                var model = await apiInvoke.CallAsync<ResponseData>("catalog.replaceBlocks", parameters);

                logger.Info("Successful invoke 'catalog.replaceBlocks' ");

                IIdentifiable.Process(model);
                return model;
            }catch(Exception ex)
            {
                logger.Error("VK API ERROR:");
                logger.Error(ex, ex.Message);
                throw;
            }
        }

        public async Task StatsTrackEvents(List<TrackEvent> obj)
        {
            try
            {
                logger.Info($"invoke 'stats.trackEvents' ");

                var stats = JsonConvert.SerializeObject(obj);
                var parameters = new VkParameters
                {
                    
                    {"device_id", await _deviceIdStore.GetDeviceIdAsync()},
                    
                    {"events", stats},
                };

                Debug.WriteLine($"SEND stats.trackEvents : {stats}");

                var json = await apiInvoke.InvokeAsync("stats.trackEvents", parameters);

                logger.Info("Successful invoke 'stats.trackEvents' ");
            }catch(Exception ex)
            {
                Debug.Fail(ex.Message, ex.StackTrace);
                logger.Error("VK API ERROR:");
                logger.Error(ex, ex.Message);
                throw;
            }
        }

        public async Task FollowCurator(long curatorId)
        {
            try
            {
                logger.Info($"invoke 'audio.followCurator' with curatorId = {curatorId} ");

                var parameters = new VkParameters
                {
                    
                    {"device_id", await _deviceIdStore.GetDeviceIdAsync()},
                    
                    {"curator_id", curatorId},
                };


                var json = await apiInvoke.InvokeAsync("audio.followCurator", parameters);

                logger.Debug("RESULT OF 'audio.followCurator'" + json);
            }
            catch (Exception ex)
            {
                logger.Error("VK API ERROR:");
                logger.Error(ex, ex.Message);
                throw;
            }
        }

        public async Task UnfollowCurator(long curatorId)
        {
            try
            {
                logger.Info($"invoke 'audio.unfollowCurator' with curatorId = {curatorId} ");

                var parameters = new VkParameters
                {
                    
                    {"device_id", await _deviceIdStore.GetDeviceIdAsync()},
                    
                    {"curator_id", curatorId},
                };


                var json = await apiInvoke.InvokeAsync("audio.unfollowCurator", parameters);

                logger.Debug("RESULT OF 'audio.unfollowCurator'" + json);
            }
            catch (Exception ex)
            {
                logger.Error("VK API ERROR:");
                logger.Error(ex, ex.Message);
                throw;
            }

        }

        public async Task FollowArtist(string artistId, string referenceId)
        {
            try
            {
                logger.Info($"invoke 'audio.followArtist' with curatorId = {artistId} ");

                var parameters = new VkParameters
                {
                    
                    {"device_id", await _deviceIdStore.GetDeviceIdAsync()},
                    
                    {"artist_id", artistId},
                    {"ref", referenceId},
                };


                var json = await apiInvoke.InvokeAsync("audio.followArtist", parameters);

                logger.Debug("RESULT OF 'audio.followArtist'" + json);
            }
            catch (Exception ex)
            {
                logger.Error("VK API ERROR:");
                logger.Error(ex, ex.Message);
                throw;
            }
        }

        public async Task UnfollowArtist(string artistId, string referenceId)
        {
            try
            {
                logger.Info($"invoke 'audio.unfollowArtist' with curatorId = {artistId} ");

                var parameters = new VkParameters
                {
                    
                    {"device_id", await _deviceIdStore.GetDeviceIdAsync()},
                    
                    {"artist_id", artistId},
                    {"ref", referenceId},
                };


                var json = await apiInvoke.InvokeAsync("audio.unfollowArtist", parameters);

                logger.Debug("RESULT OF 'audio.unfollowArtist'" + json);
            }
            catch (Exception ex)
            {
                logger.Error("VK API ERROR:");
                logger.Error(ex, ex.Message);
                throw;
            }

        }

        public async Task<RestrictionPopupData> AudioGetRestrictionPopup(string trackCode, string audio)
        {
            try
            {
                logger.Info($"invoke 'audio.getRestrictionPopup' with trackCode= {trackCode} and audio = {audio} ");

                var parameters = new VkParameters
                {
                    
                    {"device_id", await _deviceIdStore.GetDeviceIdAsync()},
                    
                    {"track_code", trackCode},
                    {"audio_id", audio }
                };


                var json = await apiInvoke.InvokeAsync("audio.getRestrictionPopup", parameters);

                var model = JsonConvert.DeserializeObject<RestrictionPopupData>(json);


                logger.Debug("RESULT OF 'audio.getRestrictionPopup'" + json);

                return model;
            }
            catch (Exception ex)
            {
                logger.Error("VK API ERROR:");
                logger.Error(ex, ex.Message);
                throw;
            }

        }

        public async Task<ResponseData> GetPodcastsAsync(string url = null)
        {
            try
            {
                logger.Info($"Invoke 'catalog.getPodcasts' with curatorId ");

                var parameters = new VkParameters
                {
                    
                    {"extended", "1"},
                    {"device_id", await _deviceIdStore.GetDeviceIdAsync()},
                    
                    {"url", url},
                };


                var model = await apiInvoke.CallAsync<ResponseData>("catalog.getPodcasts", parameters);

                logger.Info("Successful invoke 'catalog.getAudioCurator' ");


                IIdentifiable.Process(model);
                return model;
            }
            catch (Exception ex)
            {
                logger.Error("VK API ERROR:");
                logger.Error(ex, ex.Message);
                throw;
            }
        }

        public async Task<ResponseData> SectionHomeAsync()
        {
            var code = "var catalogs = API.catalog.getAudio({\"need_blocks\": 0}).catalog.sections;return API.catalog.getSection({\"need_blocks\": 1, \"section_id\": catalogs[0].id});";

            var parameters = new VkParameters
                {
                    
                    {"extended", "1"},
                    {"device_id", await _deviceIdStore.GetDeviceIdAsync()},
                    
                    {"code", code},
                };


            var model = await apiInvoke.CallAsync<ResponseData>("execute", parameters);

            IIdentifiable.Process(model);
                return model;
        }

        public async Task<ResponseData> GetRecommendationsAudio(string audio)
        {
            try
            {
                var parameters = new VkParameters
                {
                    
                    {"device_id", await _deviceIdStore.GetDeviceIdAsync()},
                    
                    {"target_audio", audio},
                };


                var model = await apiInvoke.CallAsync<ResponseData>("audio.getRecommendations", parameters);

                return model;
            }catch (Exception ex)
            {
                logger.Error("VK API ERROR:");
                logger.Error(ex, ex.Message);
                throw;
            }
          
        }

        public async Task SetBroadcastAsync(Audio? audio)
        {
            try
            {
                if (audio is null)
                {
                    await vkApi.Audio.SetBroadcastAsync();
                     return;
                }

                await vkApi.Audio.SetBroadcastAsync(audio.OwnerId + "_" + audio.Id + "_" + audio.AccessKey);
            }
            catch (Exception ex)
            {
                logger.Error("VK API ERROR:");
                logger.Error(ex, ex.Message);
                throw;
            }
        }

        public async Task<List<Playlist>> GetPlaylistsAsync(long ownerId)
        {
            try
            {
                var parameters = new VkParameters
                {
                    
                    {"device_id", await _deviceIdStore.GetDeviceIdAsync()},
                    
                    {"owner_id", ownerId},
                    {"count", 100}

                };


                var json = await apiInvoke.InvokeAsync("audio.getPlaylists", parameters);


                var model = JsonConvert.DeserializeObject<OldResponseData<Playlist>>(json);

                return model.Items;
            }
            catch (Exception ex)
            {
                logger.Error("VK API ERROR:");
                logger.Error(ex, ex.Message);
                throw;
            }
        }


        public async Task AddToPlaylistAsync(Audio audio, long ownerId, long playlistId)
        {
            try
            {
                var audioId = audio.OwnerId + "_" + audio.Id;
                await vkApi.Audio.AddToPlaylistAsync(ownerId, playlistId, new List<string>() { audioId });
            }catch(Exception ex)
             {
                logger.Error("VK API ERROR:");
                logger.Error(ex, ex.Message);
                throw;
            }
         }

        public async Task<long> CreatePlaylistAsync(long ownerId, string title, string description, IEnumerable<Audio> tracks)
        {
            try
            {
                var audios = tracks.Select(t => t.OwnerId + "_" + t.Id).ToList();

                var result = await vkApi.Audio.CreatePlaylistAsync(ownerId, title, description, audios);

                return result.Id.Value;
            }catch (Exception ex)
            {
                logger.Error("VK API ERROR:");
                logger.Error(ex, ex.Message);
                throw;
            }
        }

        public async Task SetPlaylistCoverAsync(long ownerId, long playlistId, string hash, string photo)
        {
            try
            {
                var parameters = new VkParameters
                {
                    
                    {"device_id", await _deviceIdStore.GetDeviceIdAsync()},
                    
                    {"owner_id", ownerId},
                    {"playlist_id", playlistId},
                    {"hash", hash},
                    {"photo", photo}
                };


                var json = await apiInvoke.InvokeAsync("audio.setPlaylistCoverPhoto", parameters);

            }
            catch (Exception ex)
            {
                logger.Error("VK API ERROR:");
                logger.Error(ex, ex.Message);
                throw;
            }
        }

        public async Task<UploadPlaylistCoverServerResult> GetPlaylistCoverUploadServerAsync(long ownerId, long playlistId)
        {
            try
            {
                var parameters = new VkParameters
                {
                    
                    {"device_id", await _deviceIdStore.GetDeviceIdAsync()},
                    
                    {"owner_id", ownerId},
                    {"playlist_id", playlistId},
                };

                var json = await apiInvoke.InvokeAsync("photos.getAudioPlaylistCoverUploadServer", parameters);

                var model = JsonConvert.DeserializeObject<UploadPlaylistCoverServerResult>(json);

                return model;
            }
            catch(Exception ex)
            {
                logger.Error("VK API ERROR:");
                logger.Error(ex, ex.Message);
                throw;
            }   
        }

        public async Task<UploadPlaylistCoverResult> UploadPlaylistCoverAsync(string uploadUrl, string path)
        {
            using var httpClient = new HttpClient();
            using (var stream = File.OpenRead(path))
            {
                var content = new MultipartFormDataContent();
                var streamContent = new StreamContent(stream);
                streamContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                content.Add(streamContent, "photo", Path.GetFileName(path));

                var response = await httpClient.PostAsync(uploadUrl, content).ConfigureAwait(false);

                var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                var data = JsonConvert.DeserializeObject<UploadPlaylistCoverResult>(result);

                return data;

            }
        }

        public async Task EditPlaylistAsync(long ownerId, int playlistId, string title, string description, List<Audio> tracks)
        {
            try
            {
                var audios = tracks.Select(t => t.OwnerId + "_" + t.Id);

                var result = await vkApi.Audio.EditPlaylistAsync(ownerId, playlistId, title, description, audios);

            }
            catch (Exception ex)
            {
                logger.Error("VK API ERROR:");
                logger.Error(ex, ex.Message);
                throw;
            }
        }

        public async Task<BoomToken> GetBoomToken()
        {
            var uuid = Guid.NewGuid();

            try
            {
                var parameters = new VkParameters
                {

                    {"device_id", await _deviceIdStore.GetDeviceIdAsync()},
                    {"app_id", 6767438 },
                    {"app_id", 6767438 },
                    {"timestamp", DateTimeOffset.Now.ToUnixTimeSeconds() },
                    {"app_secret", "ppBOmwQYYOMGulmaiPyK" },
                    {"package", "com.uma.musicvk" },
                    {"uuid", uuid.ToString() },
                    {"digest_hash", "2D0D1nXbs2cX1/Q8wFkyv93NHts="}
                };

                var json = await apiInvoke.InvokeAsync("auth.getCredentialsForService", parameters);

                var result = JsonConvert.DeserializeObject<List<BoomToken>>(json);

                var model = result.FirstOrDefault();

                if(model != null)
                {
                    model.Uuid = uuid.ToString();
                }

                return model;
            }
            catch (Exception ex)
            {
                logger.Error("VK API ERROR:");
                logger.Error(ex, ex.Message);
                throw;
            }
        }

        public async Task FollowOwner(long ownerId)
        {
            try
            {
                logger.Info($"Invoke 'audio.followOwner' with ownerId ");
                var parameters = new VkParameters
                {
                    
                    {"device_id", await _deviceIdStore.GetDeviceIdAsync()},
                    
                    {"owner_id", ownerId},
                };

                var json = await apiInvoke.InvokeAsync("audio.followOwner", parameters);
                logger.Debug("RESULT OF 'audio.followOwner'" + json);
                
                logger.Info("Successful invoke 'audio.followOwner' ");
            }
            catch(Exception ex)
            {
                logger.Error("VK API ERROR:");
                logger.Error(ex, ex.Message);
                throw;
            }   
        }
        
        public async Task UnfollowOwner(long ownerId)
        {
            try
            {
                logger.Info($"Invoke 'audio.unfollowOwner' with ownerId ");
                var parameters = new VkParameters
                {
                    
                    {"device_id", await _deviceIdStore.GetDeviceIdAsync()},
                    
                    {"owner_id", ownerId},
                };

                var json = await apiInvoke.InvokeAsync("audio.unfollowOwner", parameters);
                logger.Debug("RESULT OF 'audio.unfollowOwner'" + json);
                
                logger.Info("Successful invoke 'audio.unfollowOwner' ");
            }
            catch(Exception ex)
            {
                logger.Error("VK API ERROR:");
                logger.Error(ex, ex.Message);
                throw;
            }   
        }

        public async Task<Lyrics> GetLyrics(string audioId)
        {
            try
            {
                logger.Info($"Invoke 'audio.getLyrics' with audioId  {audioId}");
                var parameters = new VkParameters
                {

                    {"device_id", await _deviceIdStore.GetDeviceIdAsync()},

                    {"audio_id", audioId},
                };

                var json = await apiInvoke.InvokeAsync("audio.getLyrics", parameters);
                logger.Debug("RESULT OF 'audio.getLyrics'" + json);

                var model = JsonConvert.DeserializeObject<Lyrics>(json);

                logger.Info("Successful invoke 'audio.getLyrics' ");

                return model;
            }
            catch (Exception ex)
            {
                logger.Error("VK API ERROR:");
                logger.Error(ex, ex.Message);
                throw;
            }
        }

        public async Task Dislike(long audioId, long ownerId)
        {
            try
            {
                logger.Info($"Invoke 'audio.addDislike' with audioId  {audioId}");
                var parameters = new VkParameters
                {

                    {"device_id", await _deviceIdStore.GetDeviceIdAsync()},

                    {"audio_ids", $"{ownerId}_{audioId}"},
                };

                var json = await apiInvoke.InvokeAsync("audio.addDislike", parameters);
                logger.Debug("RESULT OF 'audio.addDislike'" + json);

                logger.Info("Successful invoke 'audio.addDislike' ");

            }
            catch (Exception ex)
            {
                logger.Error("VK API ERROR:");
                logger.Error(ex, ex.Message);
                throw;
            }
        }
        
        private readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web);

        public async Task<List<Audio>> GetStreamMixAudios(string mixId = "common", int append = 0, int count = 5, ImmutableDictionary<string, ImmutableArray<string>>? options = null)
        {
            try
            {
                var parameters = new VkParameters
                {

                    {"device_id", await _deviceIdStore.GetDeviceIdAsync()},

                    {"mix_id", mixId},
                    {"append", append},
                    {"count", count},
                    {"options", options == null ? null : System.Text.Json.JsonSerializer.Serialize(options, _jsonSerializerOptions)}
                };

                var model = await apiInvoke.CallAsync<List<Audio>>("audio.getStreamMixAudios", parameters);

                return model;
            }
            catch (Exception ex)
            {
                logger.Error("VK API ERROR:");
                logger.Error(ex, ex.Message);
                throw;
            }

        }

        public async Task<MixSettingsRoot> GetStreamMixSettings(string mixId)
        {
            try
            {
                var parameters = new VkParameters
                {

                    {"device_id", await _deviceIdStore.GetDeviceIdAsync()},

                    {"mix_id", mixId},
                };

                var model = await apiInvoke.CallAsync<MixSettingsRoot>("audio.getStreamMixSettings", parameters);

                return model;
            }
            catch (Exception ex)
            {
                logger.Error("VK API ERROR:");
                logger.Error(ex, ex.Message);
                throw;
            }

        }
        
        public async Task<ResponseData> GetAudioMyAudios()
        {
            try
            {
                var parameters = new VkParameters
                {
                    
                    {"device_id", await _deviceIdStore.GetDeviceIdAsync()},
                    
                    {"need_blocks", true},
                };


                var model = await apiInvoke.CallAsync<ResponseData>("catalog.getAudioMyAudios", parameters);

                if (model.Section is null && model.Catalog?.Sections is [var firstSection, ..])
                    model.Section = firstSection;

                IIdentifiable.Process(model);
                return model;
            }catch (Exception ex)
            {
                logger.Error("VK API ERROR:");
                logger.Error(ex, ex.Message);
                throw;
            }
          
        }

        public async Task<ResolveScreenNameResponse> GetMiniApp(string appId, string url)
        {
            var parameters = new VkParameters
            {
                {"device_id", await _deviceIdStore.GetDeviceIdAsync()},
                {"screen_name", appId},
                {"url", url},
                {"func_v", 23},
                {"app_fields", [
                        "has_vk_connect",
                        "is_vkui_internal",
                        "webview_url",
                        "screen_orientation",
                        "mobile_controls_type",
                        "splash_screen",
                        "background_loader_color",
                        "placeholder_info",
                        "hide_tabbar",
                        "track_code",
                        "author_owner_id",
                        "preload_ad_types",
                        "ad_config",
                        "can_cache",
                        "icon_75",
                        "icon_139",
                        "icon_150",
                        "icon_576",
                        "need_show_unverified_screen",
                        "is_in_catalog"
                    ]
                },
            };

            try
            {
                var model = await apiInvoke.CallAsync<ResolveScreenNameResponse>("execute.resolveScreenName",
                    parameters);

                return model;
            }
            catch (Exception e)
            {
                logger.Error(e);
                throw;
            }
        }
        
        public async Task<AppLaunchParamsResponse> GetMiniAppLaunchParams(long appId)
        {
            var parameters = new VkParameters
            {
                {"device_id", await _deviceIdStore.GetDeviceIdAsync()},
                {"mini_app_id", appId},
                {"referer", "recs"}
            };

            try
            {
                var model = await apiInvoke.CallAsync<AppLaunchParamsResponse>("apps.getAppLaunchParams",
                    parameters);

                return model;
            }
            catch (Exception e)
            {
                logger.Error(e);
                throw;
            }
        }

        public async Task<CredentialResponse> GetMiniAppCredentialToken(string sourceUrl, string scope)
        {
            var parameters = new VkParameters
            {
                {"device_id", await _deviceIdStore.GetDeviceIdAsync()},
                {"display", "android"},
                {"scope", scope},
                {"response_type", "token"},
                {"redirect_uri", "https://oauth.vk.com/blank.html"},
                {"client_id", 52384530},
                {"source_url", sourceUrl},
                {"https", true},
                {"v", vkApiVersion},
                {"access_token", _api.Token}
            };

            try
            {
                var response = await _restClient.PostAsync(new("https://api.vk.com/oauth/authorize"), parameters, Encoding.UTF8);
                
                if (!response.ResponseUri.Fragment.StartsWith("#access_token="))
                    throw new VkApiException("Access token not found");

                var query = Url.ParseQueryString("?" + response.ResponseUri.Fragment[1..]);
                return new CredentialResponse(query["access_token"]);
            }
            catch (Exception e)
            {
                logger.Error(e);
                throw;
            }
        }
    }
}
