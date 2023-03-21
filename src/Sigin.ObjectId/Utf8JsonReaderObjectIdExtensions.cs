using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Sigin.ObjectId;

/// <summary>
///     Extension methods for <see cref="Utf8JsonReader" />, that used to work with <see cref="ObjectId" /> values.
/// </summary>
[SuppressMessage("ReSharper", "RedundantNameQualifier")]
public static class Utf8JsonReaderObjectIdExtensions
{
    // https://github.com/dotnet/runtime/blob/v6.0.0/src/libraries/System.Text.Json/src/System/Text/Json/ThrowHelper.cs#L13
    private const string ExceptionSourceValueToRethrowAsJsonException = "System.Text.Json.Rethrowable";

    /// <summary>
    ///     Parses the current JSON token value from the source as a <see cref="ObjectId" />. Returns the value if the entire
    ///     UTF-8 encoded token value
    ///     can be successfully parsed to a <see cref="ObjectId" /> value. Throws exceptions otherwise.
    /// </summary>
    /// <param name="reader">Instance of <see cref="Utf8JsonReader" />.</param>
    /// <returns></returns>
    /// <exception cref="ObjectId">
    ///     Thrown if the JSON token value is of an unsupported format for a
    ///     <see cref="FormatException" />.
    /// </exception>
    public static ObjectId GetObjectId(this ref Utf8JsonReader reader)
    {
        if (!reader.TryGetObjectId(out var value))
        {
            throw new FormatException("The JSON value is not in a supported ObjectId format.")
            {
                Source = ExceptionSourceValueToRethrowAsJsonException
            };
        }

        return value;
    }

    /// <summary>
    ///     Parses the current JSON token value from the source as a <see cref="ObjectId" />. Returns <see langword="true" />
    ///     if the entire UTF-8
    ///     encoded token value can be successfully parsed to a <see cref="ObjectId" /> value. Returns <see langword="false" />
    ///     otherwise.
    /// </summary>
    /// <param name="reader">Instance of <see cref="Utf8JsonReader" />.</param>
    /// <param name="value">Output <see cref="ObjectId" /> value.</param>
    /// <returns></returns>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public static bool TryGetObjectId(this ref Utf8JsonReader reader, out ObjectId value)
    {
        var possibleObjectIdString = reader.GetString();
        if (ObjectId.TryParse(possibleObjectIdString, out value))
        {
            return true;
        }

        value = default;
        return false;
    }
}
