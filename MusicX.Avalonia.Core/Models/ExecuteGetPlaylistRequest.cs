namespace MusicX.Avalonia.Core.Models;

public record ExecuteGetPlaylistRequest(long AlbumId, string AccessKey, long OwnerId, int AudioOffset = 0,
                                        int AudioCount = 100, bool NeedPlaylist = true, int FuncV = 9, int Count = 10,
                                        bool NeedOwner = true);