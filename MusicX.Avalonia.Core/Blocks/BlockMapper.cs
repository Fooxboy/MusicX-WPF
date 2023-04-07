using MusicX.Avalonia.Core.Models;

namespace MusicX.Avalonia.Core.Blocks;

public static class BlockMapper
{
    public static IEnumerable<BlockBase> MapBlocks(CatalogGetSectionResponse response)
    {
        return response.Section.Blocks.Select(sectionBlock => sectionBlock.DataType switch
        {
            "music_playlists" => MapPlaylistsBlock(response.Playlists, sectionBlock),
            _ => MapBlock(sectionBlock)
        });
    }

    private static BlockBase MapBlock(SectionBlock block)
    {
        return new(block.Id,
                   block.DataType,
                   block.Layout,
                   block.NextFrom,
                   block.Url);
    }

    private static PlaylistsBlock MapPlaylistsBlock(ICollection<CatalogPlaylist> playlists, SectionBlock block)
    {
        return new(block.Id,
                   block.DataType,
                   block.Layout,
                   block.NextFrom,
                   block.Url,
                   block.PlaylistsIds.Select(b =>
                   {
                       var ownerId = long.Parse(b[..b.IndexOf('_')]);
                       var id = int.Parse(b[(b.IndexOf('_') + 1)..]);

                       return playlists.Single(c => c.OwnerId == ownerId && c.Id == id);
                   }).ToArray());
    }
}