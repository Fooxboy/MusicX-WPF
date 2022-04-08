using Microsoft.Extensions.DependencyInjection;
using MusicX.Core.Helpers;
using MusicX.Core.Models;
using MusicX.Core.Models.General;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VkNet;
using VkNet.AudioBypassService.Extensions;
using VkNet.Enums.Filters;
using VkNet.Model;
using VkNet.Utils;
using VkNet.Utils.AntiCaptcha;

namespace MusicX.Core.Services
{
    public class VkService
    {

        public readonly VkApi vkApi;
        private readonly Logger logger;
        private readonly string vkApiVersion = "5.178";
        private readonly string deviceId = "c3427adfd2595c73:A092cf601fef615c8b594f6ad2c63d159";

        public bool IsAuth = false;

        public VkService(Logger logger)
        {
            var services = new ServiceCollection();
            services.AddAudioBypass();

            services.ToList();

            vkApi = new VkApi(services);


            var ver = vkApiVersion.Split('.');

            vkApi.VkApiVersion.SetVersion(int.Parse(ver[0]), int.Parse(ver[1]));

            var log = LogManager.Setup().GetLogger("Common");

            if (logger == null) this.logger = log;
            else this.logger = logger;
        }

        public async Task<string> AuthAsync(string login, string password, Func<string> twoFactorAuth, ICaptchaSolver captchaSolver)
        {
            try
            {
                logger.Info("Invoke auth with login and password");
                vkApi.CaptchaSolver = captchaSolver;
                await vkApi.AuthorizeAsync(new ApiAuthParams()
                {
                    Login = login,
                    Password = password,
                    TwoFactorAuthorization = twoFactorAuth
                });


                var user = await vkApi.Users.GetAsync(new List<long>());
                vkApi.UserId = user[0].Id;

                IsAuth = true;

                logger.Info($"User '{user[0].Id}' successful sign in");


                return vkApi.Token;
            }catch(Exception ex)
            {
                logger.Error("VK API ERROR:");
                logger.Error(ex, ex.Message);
                throw;
            }
           
        }

        public async Task SetTokenAsync(string token, ICaptchaSolver captchaSolver)
        {
            try
            {
                logger.Info("Set user token");

                vkApi.CaptchaSolver = captchaSolver;
                await vkApi.AuthorizeAsync(new ApiAuthParams()
                {
                    AccessToken = token
                });

                var user = await vkApi.Users.GetAsync(new List<long>());
                vkApi.UserId = user[0].Id;

                IsAuth = true;

                logger.Info($"User '{user[0].Id}' successful sign in");
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
                    {"v", vkApiVersion},
                    {"lang", "ru"},
                    {"extended", "1"},
                    {"access_token", vkApi.Token},
                    {"device_id", deviceId}
                };

                if(url != null)
                {
                    parameters.Add("url", url);
                }

                var json = await vkApi.InvokeAsync("catalog.getAudio", parameters);
                logger.Debug("RESULT OF 'catalog.getAudio'" + json);

                var model = JsonConvert.DeserializeObject<ResponseVk>(json);

                logger.Info("Successful invoke 'catalog.getAudio' ");

                return model.Proccess().Response;
            }catch(Exception ex)
            {
                logger.Error("VK API ERROR:");
                logger.Error(ex, ex.Message);
                throw;
            }
            

        }

        public async Task<ResponseData> GetSectionAsync(string sectionId, string startFrom = null)
        {
            try
            {
                logger.Info($"Invoke 'catalog.getSection' with sectionId = '{sectionId}' ");

                var parameters = new VkParameters
                {
                    {"v", vkApiVersion},
                    {"lang", "ru"},
                    {"extended", "1"},
                    {"access_token", vkApi.Token},
                    {"device_id", deviceId},
                    {"section_id", sectionId },
                    {"need_blocks", 1 },
                };

                if (startFrom != null) parameters.Add("start_from", startFrom);


                var json = await vkApi.InvokeAsync("catalog.getSection", parameters);
                logger.Debug("RESULT OF 'catalog.getSection'" + json);


                var model = JsonConvert.DeserializeObject<ResponseVk>(json);

                logger.Info("Successful invoke 'catalog.getSection' ");


                return model.Proccess().Response;
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
                    {"v", vkApiVersion},
                    {"lang", "ru"},
                    {"extended", "1"},
                    {"device_id", deviceId},
                    {"access_token", vkApi.Token},
                    {"block_id", blockId },
                };


                var json = await vkApi.InvokeAsync("catalog.getBlockItems", parameters);
                logger.Debug("RESULT OF 'catalog.getBlockItems'" + json);


                var model = JsonConvert.DeserializeObject<ResponseVk>(json);

                logger.Info("Successful invoke 'catalog.getBlockItems' ");

                return model.Proccess().Response;
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
                    {"v", vkApiVersion},
                    {"lang", "ru"},
                    {"extended", "1"},
                    {"access_token", vkApi.Token},
                    {"device_id", deviceId},
                };

                if (query != null)
                {
                    parameters.Add("query", query);
                }
                else parameters.Add("need_blocks", "1");
                if (context != null) parameters.Add("context", context);


                var json = await vkApi.InvokeAsync("catalog.getAudioSearch", parameters);
                logger.Debug("RESULT OF 'catalog.getAudioSearch'" + json);


                var model = JsonConvert.DeserializeObject<ResponseVk>(json);
                logger.Info("Successful invoke 'catalog.getAudioSearch' ");

                return model.Proccess().Response;
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
                    {"v", vkApiVersion},
                    {"lang", "ru"},
                    {"extended", "1"},
                    {"access_token", vkApi.Token},
                    {"device_id", deviceId},
                    {"artist_id", artistId}

                };


                var json = await vkApi.InvokeAsync("catalog.getAudioArtist", parameters);
                logger.Debug("RESULT OF 'catalog.getAudioArtist'" + json);


                var model = JsonConvert.DeserializeObject<ResponseVk>(json);

                logger.Info("Successful invoke 'catalog.getAudioArtist' ");


                return model.Proccess().Response;
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
                    {"v", vkApiVersion},
                    {"lang", "ru"},
                    {"extended", "1"},
                    {"device_id", deviceId},
                    {"access_token", vkApi.Token},
                    {"curator_id", curatorId},
                    {"url", url}
                };


                var json = await vkApi.InvokeAsync("catalog.getAudioCurator", parameters);
                logger.Debug("RESULT OF 'catalog.getAudioCurator'" + json);


                var model = JsonConvert.DeserializeObject<ResponseVk>(json);

                logger.Info("Successful invoke 'catalog.getAudioCurator' ");


                return model.Proccess().Response;
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
                    {"func_v", 9 },
                    {"id", albumId},
                    {"audio_offset", offset },
                    {"access_token", vkApi.Token},
                    {"count", "10"},
                    {"need_owner", needOwner }
                };

                var json = await vkApi.InvokeAsync("execute.getPlaylist", parameters);
                logger.Debug("RESULT OF 'execute.getPlaylist'" + json);


                var model = JsonConvert.DeserializeObject<ResponseVk>(json);

                logger.Info("Successful invoke 'execute.getPlaylist' ");


                return model.Proccess().Response;
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
                    {"v", vkApiVersion},
                    {"lang", "ru"},
                    {"access_token", vkApi.Token},
                    {"audio_id", audioId},
                    {"owner_id", ownerId}
                };


                var json = await vkApi.InvokeAsync("audio.add", parameters);
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
                    {"v", vkApiVersion},
                    {"lang", "ru"},
                    {"access_token", vkApi.Token},
                    {"audio_id", audioId},
                    {"owner_id", ownerId}
                };


                var json = await vkApi.InvokeAsync("audio.delete", parameters);

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
                    {"v", vkApiVersion},
                    {"lang", "ru"},
                    {"access_token", vkApi.Token},
                    {"playlist_id", playlistId},
                    {"owner_id", ownerId},
                    
                };

                if (accessKey != null) parameters.Add("access_key", accessKey);

                var json = await vkApi.InvokeAsync("audio.followPlaylist", parameters);

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

        public async Task<ResponseData> AudioGetAsync(long playlistId, long ownerId, string assessKey)
        {
            try
            {
                logger.Info($"invoke 'audio.get' with playlistId = '{playlistId}' and ownerId = '{ownerId}' and accessKey = {assessKey} ");


                var parameters = new VkParameters
                {
                    {"v", vkApiVersion},
                    {"lang", "ru"},
                    {"access_token", vkApi.Token},
                    {"playlist_id", playlistId},
                    {"access_key", playlistId},
                    {"owner_id", ownerId},

                };


                var json = await vkApi.InvokeAsync("audio.get", parameters);

                logger.Debug("RESULT OF 'audio.get'" + json);


                var model = JsonConvert.DeserializeObject<ResponseVk>(json);

                logger.Info("Successful invoke 'audio.get' ");

                return model.Response;
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
                    {"v", vkApiVersion},
                    {"lang", "ru"},
                    {"device_id", deviceId},
                    {"access_token", vkApi.Token},
                    {"replacement_ids", replaceId},
                };



                var json = await vkApi.InvokeAsync("catalog.replaceBlocks", parameters);

                logger.Debug("RESULT OF 'catalog.replaceBlocks'" + json);


                var model = JsonConvert.DeserializeObject<ResponseVk>(json);

                logger.Info("Successful invoke 'catalog.replaceBlocks' ");


                return model.Proccess().Response;
            }catch(Exception ex)
            {
                logger.Error("VK API ERROR:");
                logger.Error(ex, ex.Message);
                throw;
            }
        }

        public async Task StatsTrackEvents(List<object> obj)
        {
            try
            {
                logger.Info($"invoke 'stats.trackEvents' ");

                var stats = JsonConvert.SerializeObject(obj);
                var parameters = new VkParameters
                {
                    {"v", vkApiVersion},
                    {"lang", "ru"},
                    {"device_id", deviceId},
                    {"access_token", vkApi.Token},
                    {"events", stats},
                };

                Debug.WriteLine($"SEND stats.trackEvents : {stats}");

                var json = await vkApi.InvokeAsync("stats.trackEvents", parameters);

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
                    {"v", vkApiVersion},
                    {"lang", "ru"},
                    {"device_id", deviceId},
                    {"access_token", vkApi.Token},
                    {"curator_id", curatorId},
                };


                var json = await vkApi.InvokeAsync("audio.followCurator", parameters);

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
                    {"v", vkApiVersion},
                    {"lang", "ru"},
                    {"device_id", deviceId},
                    {"access_token", vkApi.Token},
                    {"curator_id", curatorId},
                };


                var json = await vkApi.InvokeAsync("audio.unfollowCurator", parameters);

                logger.Debug("RESULT OF 'audio.unfollowCurator'" + json);
            }
            catch (Exception ex)
            {
                logger.Error("VK API ERROR:");
                logger.Error(ex, ex.Message);
                throw;
            }

        }

        public async Task FollowArtist(string artistId)
        {
            try
            {
                logger.Info($"invoke 'audio.followArtist' with curatorId = {artistId} ");

                var parameters = new VkParameters
                {
                    {"v", vkApiVersion},
                    {"lang", "ru"},
                    {"device_id", deviceId},
                    {"access_token", vkApi.Token},
                    {"artist_id", artistId},
                };


                var json = await vkApi.InvokeAsync("audio.followArtist", parameters);

                logger.Debug("RESULT OF 'audio.followArtist'" + json);
            }
            catch (Exception ex)
            {
                logger.Error("VK API ERROR:");
                logger.Error(ex, ex.Message);
                throw;
            }
        }

        public async Task UnfollowArtist(string artistId)
        {
            try
            {
                logger.Info($"invoke 'audio.unfollowArtist' with curatorId = {artistId} ");

                var parameters = new VkParameters
                {
                    {"v", vkApiVersion},
                    {"lang", "ru"},
                    {"device_id", deviceId},
                    {"access_token", vkApi.Token},
                    {"artist_id", artistId},
                };


                var json = await vkApi.InvokeAsync("audio.unfollowArtist", parameters);

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
                    {"v", vkApiVersion},
                    {"lang", "ru"},
                    {"device_id", deviceId},
                    {"access_token", vkApi.Token},
                    {"track_code", trackCode},
                    {"audio_id", audio }
                };


                var json = await vkApi.InvokeAsync("audio.getRestrictionPopup", parameters);

                var model = JsonConvert.DeserializeObject<ResponseRestrictionPopup>(json);


                logger.Debug("RESULT OF 'audio.getRestrictionPopup'" + json);

                return model.Response;
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
                    {"v", vkApiVersion},
                    {"lang", "ru"},
                    {"extended", "1"},
                    {"device_id", deviceId},
                    {"access_token", vkApi.Token},
                    {"url", url},
                };


                var json = await vkApi.InvokeAsync("catalog.getPodcasts", parameters);
                logger.Debug("RESULT OF 'catalog.getPodcasts'" + json);


                var model = JsonConvert.DeserializeObject<ResponseVk>(json);

                logger.Info("Successful invoke 'catalog.getAudioCurator' ");


                return model.Proccess().Response;
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
                    {"v", vkApiVersion},
                    {"lang", "ru"},
                    {"extended", "1"},
                    {"device_id", deviceId},
                    {"access_token", vkApi.Token},
                    {"code", code},
                };


            var json = await vkApi.InvokeAsync("execute", parameters);
            var model = JsonConvert.DeserializeObject<ResponseVk>(json);

            logger.Debug("RESULT OF 'execute'" + json);

            return model.Proccess().Response;
        }
    }
}
