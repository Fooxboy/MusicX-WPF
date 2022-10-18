using System.Collections.Generic;
using System.Threading.Tasks;
using MusicX.Core.Models;
using MusicX.Core.Models.General;
using MusicX.Core.Services;
namespace MusicX.Helpers;

public static class VkServiceExtensions
{
    public static async Task<ResponseData> LoadFullPlaylistAsync(this VkService service, long playlistId, long ownerId, string accessKey)
    {
        await Task.Delay(1000);

        var data = await service.GetPlaylistAsync(100, playlistId, accessKey, ownerId);
        
        var offset = data.Audios.Count;
        while (offset < data.Playlist.Count)
        {
            await Task.Delay(1000);
            var response = await service.AudioGetAsync(playlistId, ownerId, accessKey, offset);
            
            data.Audios.AddRange(response.Items);
            offset += response.Items.Count;
        }

        data.Items = data.Audios;
        return data;
    }

    public static async IAsyncEnumerable<Audio> LoadFullAudiosAsync(this VkService service, string blockId)
    {
        string? next = null;
        do
        {
            var data = await service.GetSectionAsync(blockId, next);

            if (data.Audios is null)
                yield break;
            
            foreach (var item in data.Audios)
            {
                yield return item;
            }
            
            next = data.Section.NextFrom;
        } while (!string.IsNullOrEmpty(next));
    }
}
