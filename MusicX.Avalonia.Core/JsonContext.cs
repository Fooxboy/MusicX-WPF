using System.Text.Json.Serialization;
using MusicX.Avalonia.Core.Models;

namespace MusicX.Avalonia.Core;

[JsonSerializable(typeof(AuthExceptionResponse))]
[JsonSerializable(typeof(AuthTokenResponse))]
[JsonSerializable(typeof(CatalogGetAudioResponse))]
[JsonSerializable(typeof(CatalogGetSectionResponse))]
[JsonSerializable(typeof(ICollection<UsersGetUser>))]
[JsonSerializable(typeof(ExecuteGetPlaylistResponse))]
[JsonSerializable(typeof(ExecuteGetPlaylistRequest))]
[JsonSerializable(typeof(AudioGetResponse))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
                             DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class MusicXJsonContext : JsonSerializerContext
{
}