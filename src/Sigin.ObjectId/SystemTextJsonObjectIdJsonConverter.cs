using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sigin.ObjectId;

/// <summary>
///     System.Text.Json converter for <see cref="ObjectId" />.
/// </summary>
public class SystemTextJsonObjectIdJsonConverter : JsonConverter<ObjectId>
{
    /// <inheritdoc />
    public override ObjectId Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
        )
    {
        return reader.GetObjectId();
    }

    /// <inheritdoc />
    public override void Write(
        Utf8JsonWriter writer,
        ObjectId value,
        JsonSerializerOptions options
        )
    {
        // Always will be well-formatted, cuz we allocate exact buffer for output format
        Span<char> outputBuffer = stackalloc char[24];
#if NETCOREAPP3_1 || NET5_0 || NET6_0_OR_GREATER
        value.TryFormat(outputBuffer, out _, "N");
        writer.WriteStringValue(outputBuffer);
#endif
#if NETSTANDARD2_0 || NETSTANDARD2_1
        // ReSharper disable once SuggestVarOrType_Elsewhere
        Span<char> format = stackalloc char[1];
        format[0] = 'N';
        value.TryFormat(outputBuffer, out _, format);
        writer.WriteStringValue(outputBuffer);
#endif
    }
}
