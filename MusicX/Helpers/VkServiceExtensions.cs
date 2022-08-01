using System.Threading.Tasks;
using MusicX.Core.Models.General;
using MusicX.Core.Services;
namespace MusicX.Helpers;

public static class VkServiceExtensions
{
    public static async Task<ResponseData> LoadFullPlaylistAsync(this VkService service, long playlistId, long ownerId, string accessKey)
    {
        var data = await service.GetPlaylistAsync(100, playlistId, accessKey, ownerId);
        
        var offset = data.Audios.Count;
        while (offset < data.Playlist.Count)
        {
            var response = await service.AudioGetAsync(playlistId, ownerId, accessKey, offset);
            
            data.Audios.AddRange(response.Items);
            offset += response.Items.Count;
        }

        data.Items = data.Audios;
        return data;
    }
}
