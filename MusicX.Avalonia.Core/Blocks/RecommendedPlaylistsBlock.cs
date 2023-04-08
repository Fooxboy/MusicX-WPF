using Avalonia.Media;
using MusicX.Avalonia.Core.Models;

namespace MusicX.Avalonia.Core.Blocks;

public record RecommendedPlaylistsBlock(string Id,
                                        string DataType,
                                        SectionBlockLayout Layout,
                                        string? NextFrom,
                                        string? Url,
                                        ICollection<RecommendedPlaylist> RecommendedPlaylists) : BlockBase(Id, DataType, Layout, NextFrom, Url);

public record RecommendedPlaylist(int Id,
                                  int OwnerId,
                                  double Percentage,
                                  string PercentageTitle,
                                  ICollection<CatalogAudio> Audios,
                                  Color Color,
                                  CatalogPlaylist Playlist);