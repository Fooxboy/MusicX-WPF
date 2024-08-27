using MusicX.Shared.ListenTogether.Radio;
using Newtonsoft.Json;
using MusicX.Core.Helpers;

namespace MusicX.Core.Models
{
    public class Block : IIdentifiable
    {
        string IIdentifiable.Identifier => Id;

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("data_type")]
        public string DataType { get; set; }

        [JsonProperty("layout")]
        public Layout Layout { get; set; }

        [JsonProperty("catalog_banner_ids")]
        public List<int> CatalogBannerIds { get; set; } = [];

        [JsonProperty("links_ids")]
        public List<string> LinksIds { get; set; } = [];

        [JsonProperty("buttons")]
        public List<Button> Buttons { get; set; } = [];

        [JsonProperty("actions")]
        public List<Button> Actions { get; set; } = [];

        [JsonProperty("next_from")]
        public string NextFrom { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("listen_events")]
        public List<string> ListenEvents { get; set; } = [];

        [JsonProperty("playlists_ids")]
        public List<string> PlaylistsIds { get; set; } = [];

        [JsonProperty("suggestions_ids")]
        public List<string> SuggestionsIds { get; set; } = [];

        [JsonProperty("artists_ids")]
        public List<string> ArtistsIds { get; set; } = [];

        [JsonProperty("badge")]
        public Badge Badge { get; set; }

        [JsonProperty("audios_ids")]
        public List<string> AudiosIds { get; set; } = [];

        [JsonProperty("curators_ids")]
        public List<long> CuratorsIds { get; set; } = [];

        [JsonProperty("group_ids")]
        public List<long> GroupIds { get; set; } = [];

        [JsonProperty("text_ids")]
        public List<string> TextIds { get; set; } = [];

        [JsonProperty("podcast_episodes_ids")]
        public List<string> PodcastEpisodesIds { get; set; } = [];

        [JsonProperty("podcast_slider_items_ids")]
        public List<string> PodcastSliderItemsIds { get; set; } = [];

        [JsonProperty("longreads_ids")]
        public List<string> LongreadsIds { get; set; } = [];

        [JsonProperty("videos_ids")]
        public List<string> VideosIds { get; set; } = [];

        [JsonProperty("artist_videos_ids")]
        public List<string> ArtistVideosIds { get; set; } = [];

        [JsonProperty("placeholder_ids")]
        public List<string> PlaceholdersIds { get; set; } = [];

        [JsonProperty("music_owners_ids")]
        public List<string> MusicOwnerIds { get; set; } = [];

        [JsonProperty("audio_followings_update_info_ids")]
        public List<string> FollowingUpdateInfoIds { get; set; } = [];

        [JsonProperty("group_items")]
        public List<CuratorGroup> GroupsItemsIds { get; set; } = [];

        public List<Curator> Curators { get; set; } = [];
        public List<Text> Texts { get; set; } = [];
        public List<Audio> Audios { get; set; } = [];
        public List<Playlist> Playlists { get; set; } = [];
        public List<CatalogBanner> Banners { get; set; } = [];
        public List<Link> Links { get; set; } = [];
        public List<Suggestion> Suggestions { get; set; } = [];
        public List<Artist> Artists { get; set; } = [];
        public List<Group> Groups { get; set; } = [];
        public List<PodcastSliderItem> PodcastSliderItems { get; set; } = [];
        public List<PodcastEpisode> PodcastEpisodes { get; set; } = [];
        public List<Longread> Longreads { get; set; } = [];
        public List<RecommendedPlaylist> RecommendedPlaylists { get; set; } = [];
        public List<Video> Videos { get; set; } = [];
        public List<Video> ArtistVideos { get; set; } = [];

        public List<Placeholder> Placeholders { get; set; } = [];

        public List<MusicOwner> MusicOwners { get; set; } = [];
        
        public List<AudioFollowingsUpdateInfo> FollowingsUpdateInfos { get; set; } = [];

        public List<Station> Stations { get; set; } = [];

        public Block Clone()
        {
            return (Block)MemberwiseClone();
        }

        public void Merge(Block block)
        {
            CatalogBannerIds.AddRange(block.CatalogBannerIds);
            LinksIds.AddRange(block.LinksIds);
            Buttons.AddRange(block.Buttons);
            Actions.AddRange(block.Actions);
            NextFrom = block.NextFrom;
            Url = block.Url;
            ListenEvents.AddRange(block.ListenEvents);
            PlaylistsIds.AddRange(block.PlaylistsIds);
            ArtistsIds.AddRange(block.ArtistsIds);
            Badge = block.Badge;
            AudiosIds.AddRange(block.AudiosIds);
            CuratorsIds.AddRange(block.CuratorsIds);
            GroupIds.AddRange(block.GroupIds);
            TextIds.AddRange(block.TextIds);
            PodcastEpisodesIds.AddRange(block.PodcastEpisodesIds);
            PodcastSliderItemsIds.AddRange(block.PodcastSliderItemsIds);
            LongreadsIds.AddRange(block.LongreadsIds);
            VideosIds.AddRange(block.VideosIds);
            ArtistVideosIds.AddRange(block.ArtistVideosIds);
            PlaceholdersIds.AddRange(block.PlaceholdersIds);
            MusicOwnerIds.AddRange(block.MusicOwnerIds);
            FollowingUpdateInfoIds.AddRange(block.FollowingUpdateInfoIds);
            GroupsItemsIds.AddRange(block.GroupsItemsIds);
            Curators.AddRange(block.Curators);
            Texts.AddRange(block.Texts);
            Audios.AddRange(block.Audios);
            Playlists.AddRange(block.Playlists);
            Banners.AddRange(block.Banners);
            Links.AddRange(block.Links);
            Suggestions.AddRange(block.Suggestions);
            Artists.AddRange(block.Artists);
            Groups.AddRange(block.Groups);
            PodcastSliderItems.AddRange(block.PodcastSliderItems);
            PodcastEpisodes.AddRange(block.PodcastEpisodes);
            Longreads.AddRange(block.Longreads);
            Videos.AddRange(block.Videos);
            ArtistVideos.AddRange(block.ArtistVideos);
            Placeholders.AddRange(block.Placeholders);
            MusicOwners.AddRange(block.MusicOwners);
            FollowingsUpdateInfos.AddRange(block.FollowingsUpdateInfos);
            RecommendedPlaylists.AddRange(block.RecommendedPlaylists);
            Stations.AddRange(block.Stations);
        }
    }
}
