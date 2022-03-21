using Microsoft.Extensions.DependencyInjection;
using MusicX.Core.Helpers;
using MusicX.Core.Models;
using MusicX.Core.Models.General;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
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
        private readonly string vkApiVersion = "5.135";

        public bool IsAuth = false;

        public VkService(Logger logger)
        {
            var services = new ServiceCollection();
            services.AddAudioBypass();

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

        public async Task<ResponseData> GetAudioCatalogAsync()
        {
            try
            {
                logger.Info("Invoke 'catalog.getAudio' ");

                var parameters = new VkParameters
            {
                {"v", vkApiVersion},
                {"lang", "ru"},
                {"extended", "1"},
                {"access_token", vkApi.Token}
            };

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
                {"section_id", sectionId },
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

        public async Task<ResponseData> GetAudioSearchAsync(string query, string context= null)
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
                    {"query", query}
                };

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
    }
}
