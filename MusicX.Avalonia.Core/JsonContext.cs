using System.Text.Json.Serialization;
using MusicX.Avalonia.Core.Models;

namespace MusicX.Avalonia.Core;

[JsonSerializable(typeof(AuthExceptionResponse))]
[JsonSerializable(typeof(AuthTokenResponse))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
internal partial class JsonContext : JsonSerializerContext
{
}