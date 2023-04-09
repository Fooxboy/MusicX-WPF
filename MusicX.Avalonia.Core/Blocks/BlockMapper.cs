using Avalonia.Media;
using MusicX.Avalonia.Core.Models;

namespace MusicX.Avalonia.Core.Blocks;

public static class BlockMapper
{
    public static IEnumerable<BlockBase> MapBlocks(CatalogGetSectionResponse response)
    {
        return response.Section.Blocks.Select(sectionBlock => sectionBlock.DataType switch
        {
            "music_playlists" => MapPlaylistsBlock(response.Playlists, sectionBlock),
            "music_audios" => MapAudiosBlock(response.Audios, sectionBlock),
            "music_recommended_playlists" => MapRecommendedPlaylistsBlock(
                response.Playlists, response.RecommendedPlaylists, response.Audios, sectionBlock),
            "action" => MapActionBlock(sectionBlock),
            "catalog_banners" => MapCatalogBannersBlock(response.CatalogBanners, sectionBlock),
            _ => MapBlock(sectionBlock)
        }).Where(AdFilter);
    }

    private static bool AdFilter(BlockBase block)
    {
        return block switch
        {
            BannersBlock bannersBlock when bannersBlock.Banners.ElementAt(0).ClickAction.Action.Url?.Contains("subscription") is true => false,
            BannersBlock bannersBlock when bannersBlock.Banners.ElementAt(0).ClickAction.Action.Url?.Contains("combo") is true => false,
            BannersBlock bannersBlock when bannersBlock.Banners.ElementAt(0).ClickAction.Action.Url?.Contains("https://vk.com/app") is true => false,
            _ => true
        };
    }

    private static BannersBlock MapCatalogBannersBlock(ICollection<CatalogBanner> banners, SectionBlock block)
    {
        return new(block.Id,
                   block.DataType,
                   block.Layout,
                   block.NextFrom,
                   block.Url,
                   block.CatalogBannerIds.Select(b => banners.Single(c => c.Id == b)).ToArray());
    }

    private static ActionBlock MapActionBlock(SectionBlock block)
    {
        return new(block.Id,
                   block.DataType,
                   block.Layout,
                   block.NextFrom,
                   block.Url,
                   block.Actions);
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

    private static RecommendedPlaylistsBlock MapRecommendedPlaylistsBlock(ICollection<CatalogPlaylist> playlists,
                                                                          ICollection<CatalogRecommendedPlaylist>
                                                                              recommendedPlaylists,
                                                                          ICollection<CatalogAudio> audios,
                                                                          SectionBlock block)
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

                       var recommendedPlaylist = recommendedPlaylists.Single(c => c.OwnerId == ownerId && c.Id == id);
                       var playlist = playlists.Single(c => c.OwnerId == ownerId && c.Id == id);

                       return new RecommendedPlaylist(recommendedPlaylist.Id, recommendedPlaylist.OwnerId,
                                                      recommendedPlaylist.Percentage,
                                                      recommendedPlaylist.PercentageTitle, recommendedPlaylist.Audios
                                                          .Select(b =>
                                                          {
                                                              var ownerId = long.Parse(b[..b.IndexOf('_')]);
                                                              var id = int.Parse(b[(b.IndexOf('_') + 1)..]);

                                                              return audios.Single(
                                                                      c => c.OwnerId == ownerId && c.Id == id) with
                                                                  {
                                                                      ParentBlockId = block.Id
                                                                  };
                                                          }).ToArray(), Color.Parse(recommendedPlaylist.Color),
                                                      playlist);
                   }).ToArray());
    }

    private static AudiosBlock MapAudiosBlock(ICollection<CatalogAudio> audios, SectionBlock block)
    {
        return new(block.Id,
                   block.DataType,
                   block.Layout,
                   block.NextFrom,
                   block.Url,
                   block.AudiosIds.Select(b =>
                   {
                       var ownerId = long.Parse(b[..b.IndexOf('_')]);
                       var id = int.Parse(b[(b.IndexOf('_') + 1)..]);

                       return audios.Single(c => c.OwnerId == ownerId && c.Id == id) with
                       {
                           ParentBlockId = block.Id
                       };
                   }).ToArray());
    }
}