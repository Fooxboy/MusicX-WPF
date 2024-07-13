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

[JsonConverter(typeof(PlaylistJsonConverter))]
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
        return other is PlaylistBase<TData> { Data: { } otherData } && GetType() == other.GetType() && Data.Equals(otherData);
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

public class PlaylistJsonConverter : JsonConverter<IPlaylist>
{
    public override IPlaylist? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType is not JsonTokenType.StartObject || !reader.Read())
            throw new JsonException("Unexpected end when reading playlist.");

        reader.Read();
        var type = reader.GetString();
        reader.Read();

        IPlaylist? playlist = type switch
        {
            "single" => JsonSerializer.Deserialize<SinglePlaylist>(ref reader, options),
            "list" => JsonSerializer.Deserialize<ListPlaylist>(ref reader, options),
            "radio" => JsonSerializer.Deserialize<RadioPlaylist>(ref reader, options),
            "vkBlock" => JsonSerializer.Deserialize<VkBlockPlaylist>(ref reader, options),
            "vkPlaylist" => JsonSerializer.Deserialize<VkPlaylistPlaylist>(ref reader, options),
            "shuffleVkPlaylist" => JsonSerializer.Deserialize<ShuffleVkPlaylistPlaylist>(ref reader, options),
            _ => throw new JsonException("Unsupported playlist type.")
        };
        
        if (playlist is null || !reader.Read())
            throw new JsonException("Unexpected end when reading playlist.");
        
        return playlist;
    }

    public override void Write(Utf8JsonWriter writer, IPlaylist value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        void WriteObject<T>(string type, T data)
        {
            writer.WriteString("$type", type);
            writer.WritePropertyName("Data");
            JsonSerializer.Serialize(writer, data, options);
        }
        
        switch (value)
        {
            case SinglePlaylist playlist:
                WriteObject("single", playlist);
                break;
            case ListPlaylist playlist:
                WriteObject("list", playlist);
                break;
            case RadioPlaylist playlist:
                WriteObject("radio", playlist);
                break;
            case VkBlockPlaylist playlist:
                WriteObject("vkBlock", playlist);
                break;
            case VkPlaylistPlaylist playlist:
                WriteObject("vkPlaylist", playlist);
                break;
            case ShuffleVkPlaylistPlaylist playlist:
                WriteObject("shuffleVkPlaylist", playlist);
                break;
            default:
                throw new JsonException("Unsupported playlist type.");
        }
        
        writer.WriteEndObject();
    }
}