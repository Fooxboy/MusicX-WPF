using System.Buffers.Text;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MusicX.Avalonia.Core.Converters;

public class DateTimeConverterForCustomStandardFormatR : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return new DateTime(1970, 1, 1).AddSeconds(reader.GetInt64());
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        // The "R" standard format will always be 29 bytes.
        Span<byte> utf8Date = stackalloc byte[29];

        var result = Utf8Formatter.TryFormat(value, utf8Date, out _, new('R'));
        Debug.Assert(result);

        writer.WriteStringValue(utf8Date);
    }
}