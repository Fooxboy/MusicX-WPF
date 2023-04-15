namespace MusicX.Avalonia.Core.Models;

public record CatalogGetSectionResponse(Section Section,
                                        ICollection<CatalogProfile>? Profiles,
                                        ICollection<CatalogGroup>? Groups,
                                        ICollection<object> Albums,
                                        ICollection<CatalogAudio> Audios,
                                        ICollection<CatalogRecommendedPlaylist> RecommendedPlaylists,
                                        ICollection<CatalogPlaylist> Playlists,
                                        ICollection<CatalogAudioFollowingsUpdateInfo> AudioFollowingsUpdateInfo,
                                        ICollection<CatalogBanner> CatalogBanners,
                                        ICollection<object> NavigationTabs,
                                        ICollection<object> ReactionSets,
                                        ICollection<CatalogMainArtist> Artists,
                                        ICollection<CatalogLink> Links,
                                        ICollection<CatalogVideo> Videos,
                                        ICollection<CatalogVideo> ArtistVideos);