using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using MusicX.Shared.Player;

namespace MusicX.Services.Player.Playlists;

public interface IPlaylist<out TData> : IPlaylist where TData : class, IEquatable<TData>
{
    TData Data { get; }
}

[JsonDerivedType(typeof(SinglePlaylist), "single")]
[JsonDerivedType(typeof(ListPlaylist), "list")]
[JsonDerivedType(typeof(RadioPlaylist), "radio")]
[JsonDerivedType(typeof(VkBlockPlaylist), "vkBlock")]
[JsonDerivedType(typeof(VkPlaylistPlaylist), "vkPlaylist")]
public interface IPlaylist : IEquatable<IPlaylist>
{
    bool CanLoad { get; }
    IAsyncEnumerable<PlaylistTrack> LoadAsync();
}

public abstract class PlaylistBase<TData> : IPlaylist<TData> where TData : class, IEquatable<TData>
{
    public abstract IAsyncEnumerable<PlaylistTrack> LoadAsync();
    public abstract bool CanLoad { get; }
    public abstract TData Data { get; }
    
    public bool Equals(IPlaylist? other)
    {
        return other is PlaylistBase<TData> { Data: { } otherData } && Data.Equals(otherData);
    }

    public override bool Equals(object? obj) => Equals((IPlaylist?)obj);
    
    public override int GetHashCode() => Data.GetHashCode();
}

public class PlaylistJsonConverter<TPlaylist, TData> : JsonConverter<TPlaylist> where TPlaylist : class, IPlaylist<TData> where TData : class, IEquatable<TData>
{
    public override TPlaylist? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var data = JsonSerializer.Deserialize<TData>(ref reader, options);

        return data is null ? null : ActivatorUtilities.CreateInstance<TPlaylist>(StaticService.Container, data);
    }

    public override void Write(Utf8JsonWriter writer, TPlaylist value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.Data, options);
    }
}