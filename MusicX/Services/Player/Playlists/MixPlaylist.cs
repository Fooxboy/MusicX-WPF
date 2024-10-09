using MusicX.Core.Services;
using MusicX.Shared.Player;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace MusicX.Services.Player.Playlists;

[JsonConverter(typeof(PlaylistJsonConverter<MixPlaylist, MixOptions>))]
public class MixPlaylist(MixOptions data, VkService vkService) : PlaylistBase<MixOptions>
{
    public override bool CanLoad => true;
    public override MixOptions Data => data;

    public override async IAsyncEnumerable<PlaylistTrack> LoadAsync()
    {
        var tracks = await vkService.GetStreamMixAudios(data.Id, data.Append, options: data.Options);

        foreach (var track in tracks)
        {
            yield return track.ToTrack();
        }

        data = data with { Append = data.Append + 1 };
    }
}

public record MixOptions(string Id, int Append = 0, ImmutableDictionary<string, ImmutableArray<string>>? Options = null)
{
    public override int GetHashCode()
    {
        var hashCode = new HashCode();

        hashCode.Add(Id);
        hashCode.Add(Append);
        if (Options is not null)
            foreach (var (key, values) in Options)
            {
                hashCode.Add(key);
                foreach (var item in values)
                {
                    hashCode.Add(item);
                }
            }

        return hashCode.ToHashCode();
    }
}
