using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MusicX.Core.Models;
using MusicX.Core.Models.General;
using MusicX.Core.Services;
using MusicX.Helpers;
using MusicX.Shared.ListenTogether.Radio;
using NLog;
using VkNet.Abstractions;
using VkNet.Enums.SafetyEnums;
using VkNet.Model;
using Button = MusicX.Core.Models.Button;

namespace MusicX.Services;

public partial class CustomSectionsService : ICustomSectionsService
{
    private readonly IVkApiCategories _vkCategories;
    private readonly IVkApiInvoke _apiInvoke;
    private readonly UserRadioService _userRadioService;
    private readonly Logger _logger;
    private readonly ConfigService _configService;

    public CustomSectionsService(IVkApiCategories vkCategories, IVkApiInvoke apiInvoke, UserRadioService userRadioService, Logger logger, ConfigService configService)
    {
        _vkCategories = vkCategories;
        _apiInvoke = apiInvoke;
        _userRadioService = userRadioService;
        _logger = logger;
        _configService = configService;
    }

    public async IAsyncEnumerable<Section> GetSectionsAsync()
    {
        /*yield return new()
        {
            Title = "Каталоги",
            Id = "profiles",
            Url = "https://vk.com/profiles"
        };*/
        yield return new()
        {
            Title = "Поиск",
            Id = "search"
        };
    }

    public async ValueTask<ResponseData?> HandleSectionRequest(string id, string? nextFrom) =>
        id switch
        {
            "profiles" => new()
            {
                Section = await GetCatalogsSectionAsync()
            },
            "attachments_full" => new()
            {
                Section = await GetAttachmentConvsSectionAsync(nextFrom)
            },
            "search" => new()
            {
                Section = await GetSearchSectionAsync()
            },
            "track_recomms_full" => await GetTrackRecommsSectionAsync(nextFrom),
            "track_popular_full" => await GetPopularTracksSectionAsync(nextFrom),
            _ when CustomLinkRegex().IsMatch(id) => await GetAttachmentsSectionAsync(id, nextFrom),
            _ => null
        };

    private async Task<ResponseData> GetTrackRecommsSectionAsync(string? startFrom)
    {
        var (count, audios) = await _apiInvoke.CallAsync<AudioGetRecommendationsResponse>("audio.getRecommendations",
            new()
            {
                ["count"] = "40",
                ["offset"] = startFrom
            });

        return MapAudios("track_recomms_full", startFrom, audios, count, () => new()
        {
            Id = Random.Shared.Next().ToString(),
            DataType = "none",
            Layout = new()
            {
                Title = "Рекомендации",
                Name = "header_extended",
                Subtitle = "Рекомендации на основе вашей библиотеки",
            }
        });
    }
    private async Task<ResponseData> GetPopularTracksSectionAsync(string? startFrom)
    {
        var audios = await _apiInvoke.CallAsync<List<Audio>>("audio.getPopular",
            new()
            {
                ["count"] = "40",
                ["offset"] = startFrom
            });

        return MapAudios("track_popular_full", startFrom, audios, audios.Count, () => new()
        {
            Id = Random.Shared.Next().ToString(),
            DataType = "none",
            Layout = new()
            {
                Title = "Популярное",
                Name = "header_extended",
                Subtitle = "Популярные треки в вашем регионе",
            }
        });
    }

    private static ResponseData MapAudios(string id, string? startFrom, List<Audio> audios, int count, Func<Block> firstBlockFactory)
    {
        if (audios.Count == 0 && startFrom is null)
            return new()
            {
                Section = new()
                {
                    Id = id,
                    Blocks = new()
                    {
                        new()
                        {
                            Id = Random.Shared.Next().ToString(),
                            DataType = "none",
                            Layout = new()
                            {
                                Name = "header_extended",
                                Title = "Ничего не найдено"
                            }
                        },
                    }
                }
            };
        
        foreach (var audio in audios)
        {
            audio.ParentBlockId = id;
        }
        
        ulong? offset = startFrom is null ? null : ulong.Parse(startFrom);

        var section = new Section
        {
            Id = id,
            NextFrom = offset.GetValueOrDefault() + (ulong)audios.Count < (ulong)count
                ? (offset.GetValueOrDefault() + (ulong)audios.Count).ToString()
                : null!,
            Blocks = new()
            {
                new()
                {
                    Id = Random.Shared.Next().ToString(),
                    DataType = "music_audios",
                    Layout = new()
                    {
                        Name = "list"
                    },
                    Audios = audios
                }
            }
        };
        
        if (startFrom is null)
            section.Blocks.Insert(0, firstBlockFactory());
        
        return new()
        {
            Section = section,
            Audios = audios
        };
    }

    private async Task<Section> GetSearchSectionAsync()
    {
        var vkService = StaticService.Container.GetRequiredService<VkService>();
        var response = await vkService.GetAudioSearchAsync();

        response.Catalog.Sections[0].Blocks[1].Suggestions = response.Suggestions;

        return response.Catalog.Sections[0];
    }

    private async Task<ResponseData> GetAttachmentsSectionAsync(string id, string? startFrom)
    {
        var peerId = long.Parse(id);

        var (attachments, nextFrom) = await _apiInvoke.CallAsync<MessagesGetAttachmentsResponse>("messages.getHistoryAttachments", new()
        {
            ["peer_id"] = peerId.ToString(),
            ["media_type"] = "audio",
            ["start_from"] = startFrom
        });

        if (attachments.Length == 0 && startFrom is null)
            return new()
            {
                Section = new()
                {
                    Id = id,
                    Blocks = new()
                    {
                        new()
                        {
                            Id = Random.Shared.Next().ToString(),
                            DataType = "none",
                            Layout = new()
                            {
                                Name = "header_extended",
                                Title = "Ничего не найдено"
                            }
                        },
                    }
                }
            };
        
        if (attachments.Length == 0)
            return new()
            {
                Section = new()
                {
                    Id = id
                }
            }; 

        var audios = attachments.Select(b =>
        {
            b.Attachment.Audio.ParentBlockId = id;
            return b.Attachment.Audio;
        }).ToList();
        
        var response = new ResponseData
        {
            Section = new()
            {
                Id = id,
                Blocks = new()
                {
                    new()
                    {
                        DataType = "music_audios",
                        Layout = new()
                        {
                            Name = "list"
                        },
                        Audios = audios
                    }
                },
                NextFrom = nextFrom!
            },
            Audios = audios
        };
        
        if (startFrom is null)
            response.Section.Blocks.InsertRange(0, new Block[]
            {
                new()
                {
                    Id = Random.Shared.Next().ToString(),
                    DataType = "none",
                    Layout = new()
                    {
                        Name = "header",
                        Title = "Треки из вложений"
                    }
                },
                new()
                {
                    Id = Random.Shared.Next().ToString(),
                    DataType = "action",
                    Layout = new()
                    {
                        Name = "horizontal"
                    },
                    Actions = new()
                    {
                        new()
                        {
                            BlockId = id,
                            Action = new()
                            {
                                Type = "create_playlist"
                            }
                        }
                    }
                }
            });

        return response;
    }

    private async Task<Section> GetAttachmentConvsSectionAsync(string? startFrom)
    {
        ulong? offset = startFrom is null ? null : ulong.Parse(startFrom);
        
        var convs = await _vkCategories.Messages.GetConversationsAsync(new()
        {
            Extended = true,
            Offset = offset
        });

        return new()
        {
            Id = "attachments_full",
            NextFrom = offset.GetValueOrDefault() + (ulong)convs.Items.Count < (ulong)convs.Count
                ? (offset.GetValueOrDefault() + (ulong)convs.Items.Count).ToString()
                : null!,
            Blocks = new()
            {
                MapLinksBlock(convs, true)
            }
        };
    }

    private async Task<Section> GetCatalogsSectionAsync()
    {
        var convs = await _vkCategories.Messages.GetConversationsAsync(new()
        {
            Extended = true,
            Count = 10
        });

        List<Station> stations = null;
        try
        {
            stations = await _userRadioService.GetStationsList();

        }catch(Exception ex)
        {
            _logger.Info($"Ошибка получения списка радиостанций: {ex}");
            _logger.Error(ex);
        }

        var buttons = convs.Count > 10
            ? new()
            {
                new()
                {
                    Title = "Показать все",
                    SectionId = "attachments_full"
                }
            }
            : new List<Button>();

        return new()
        {
            Title = "Профили",
            Id = "profiles",
            Blocks = new()
            {
                new()
                {
                    Id = Random.Shared.Next().ToString(),
                    DataType = "links",
                    Layout = new()
                    {
                        Name = "categories_list"
                    },
                    Links = new()
                    {
                        new()
                        {
                            Id = "track_recomms_full",
                            Url = "track_recomms_full",
                            Image = new()
                            {
                                new()
                                {
                                    Url = "/Assets/icons/ic_fluent_megaphone_24_regular.png"
                                }
                            },
                            Meta = new()
                            {
                                ContentType = "custom",
                                TrackCode = "track_recomms_full"
                            },
                            Title = "Рекомендации"
                        },
                        new()
                        {
                            Id = "track_popular_full",
                            Url = "track_popular_full",
                            Image = new()
                            {
                                new()
                                {
                                    Url = "/Assets/icons/ic_fluent_arrow_trending_24_regular.png"
                                }
                            },
                            Meta = new()
                            {
                                ContentType = "custom",
                                TrackCode = "track_popular_full"
                            },
                            Title = "Популярное"
                        },
                        new()
                        {
                            Id = "recoms_friends",
                            Url = $"https://vk.com/audios{_configService.Config.UserId}?section=recoms_friends",
                            Image = new()
                            {
                                new()
                                {
                                    Url = "/Assets/icons/ic_fluent_people_24_regular.png"
                                }
                            },
                            Title = "Музыка друзей"
                        },
                        new()
                        {
                            Id = "recommended_groups",
                            Url = $"https://vk.com/audios{_configService.Config.UserId}?section=recommended_groups",
                            Image = new()
                            {
                                new()
                                {
                                    Url = "/Assets/icons/ic_fluent_people_community_24_regular.png"
                                }
                            },
                            Title = "Музыка сообществ"
                        },
                        new()
                        {
                            Id = "recommended_groups",
                            Url = $"https://vk.com/audios{_configService.Config.UserId}?section=artist_recoms",
                            Image = new()
                            {
                                new()
                                {
                                    Url = "/Assets/icons/ic_fluent_headphones_sound_wave_24_regular.png"
                                }
                            },
                            Title = "Исполнители для вас"
                        },
                    }
                },
                new()
                {
                    Id = Random.Shared.Next().ToString(),
                    DataType = "none",
                    Layout = new()
                    {
                        Name = "header_extended",
                        Title = "Вложения"
                    },
                    Buttons = buttons
                },
                MapLinksBlock(convs),

                new()
                {
                    Id = Random.Shared.Next().ToString(),
                    DataType = "none",
                    Layout = new()
                    {
                        Name = "separator",
                    },
                },


                new()
                {
                    Id = Random.Shared.Next().ToString(),
                    DataType = "none",
                    Layout = new()
                    {
                        Name = "header_extended",
                        Title = "Радиостанции пользователей"
                    }
                },

                MapStationsBlock(stations),

                new()
                {
                    Id = Random.Shared.Next().ToString(),
                    DataType = "none",
                    Layout = new()
                    {
                        Name = "separator",
                    },
                },

                GetPlaceholderBlock(),
            }
        };
    }

    private Block GetPlaceholderBlock()
    {
        return new()
        {
            Id = Random.Shared.Next().ToString(),
            DataType = "placeholder",
            Placeholders = new List<Placeholder>()
                    {
                        new Placeholder()
                        {
                            Text = "Этот раздел будет пополнятся со временем :) Следите за новостями!",
                            Id = "jksdfksdkf",
                            Title = "Это ещё не все!",
                            Icons = new List<Core.Models.Image>()
                            {
                                new Core.Models.Image()
                                {
                                    Url = "https://sun2-17.userapi.com/O1eJDSj3KbMqaJMxBP46CTWtWTLytlS-4JSrEA/X8a7Q4les5o.png"
                                }
                            },
                            Buttons = new List<Button>
                            {
                                new Button()
                                {
                                    Title = "Телеграм канал",
                                    Action = new ActionButton()
                                    {
                                        Url = "https://t.me/MusicXPlayer",
                                        Type = "custom_open_browser"
                                    }
                                }
                            }
                        }
                    },
        };
    }


    private Block MapStationsBlock(List<Station> stations)
    {
        return new()
        {
            Id = Random.Shared.Next().ToString(),

            DataType = "stations",

            Layout = new()
            {
                Name = "large_slider"
            },

            Stations = stations
        };
    }

    private static Block MapLinksBlock(GetConversationsResult convs, bool full = false)
    {
        return new()
        {
            Id = Random.Shared.Next().ToString(),
            DataType = "links",
            Layout = new()
            {
                Name = full ? "list" : "large_slider"
            },
            Links = convs.Items.Where(b => b.Conversation.CanWrite.Allowed).Select(b =>
            {
                var type = b.Conversation.Peer.Type;
                var id = b.Conversation.Peer.Id.ToString();

                var name = true switch
                {
                    _ when type == ConversationPeerType.Chat => b.Conversation.ChatSettings.Title,
                    _ when type == ConversationPeerType.Group => convs.Groups.Single(g =>
                        g.Id == b.Conversation.Peer.LocalId).Name,
                    _ when type == ConversationPeerType.User => convs.Profiles.Single(g =>
                        g.Id == b.Conversation.Peer.Id).GetFullName(),
                    _ => "<unnamed>"
                };

                return new Link
                {
                    Id = id,
                    Url = $"https://vk.com/history{id}_audio",
                    Title = name,
                    Meta = new()
                    {
                        ContentType = true switch
                        {
                            _ when type == ConversationPeerType.Chat => "chat",
                            _ when type == ConversationPeerType.Group => "group",
                            _ when type == ConversationPeerType.User => "user",
                            _ => string.Empty
                        }
                    },
                    Image = true switch
                    {
                        _ when type == ConversationPeerType.Chat => b.Conversation.ChatSettings
                            .ToImageList(convs.Profiles),
                        _ when type == ConversationPeerType.Group => convs.Groups.Single(g =>
                            g.Id == b.Conversation.Peer.LocalId).ToImageList(),
                        _ when type == ConversationPeerType.User => convs.Profiles.Single(g =>
                            g.Id == b.Conversation.Peer.Id).ToImageList(),
                        _ => new()
                    }
                };
            }).ToList()
        };
    }

    [GeneratedRegex("^[c-]?\\d*$")]
    public static partial Regex CustomLinkRegex();
}

public record MessagesGetAttachmentsResponse(MessagesGetAttachments[] Items, string? NextFrom);
public record MessagesGetAttachments(MessagesGetAttachmentsAttachment Attachment);
public record MessagesGetAttachmentsAttachment(Audio Audio);
public record AudioGetRecommendationsResponse(int Count, List<Audio> Items);