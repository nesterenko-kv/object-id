using System;
using System.Buffers.Binary;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using System.Threading;
using Sigin.ObjectId.Internal;
#if NET7_0_OR_GREATER
using System.Numerics;
#endif

namespace Sigin.ObjectId
{
    /// <summary>
    ///     Represents BSON ObjectID is a 12-byte value consisting of:
    ///     - a 4-byte timestamp (seconds since epoch)
    ///     - a 3-byte machine id
    ///     - a 2-byte process id
    ///     - a 3-byte counter
    ///     0123 456     78  91011
    ///     time machine pid inc
    ///     https://github.com/mongodb/specifications/blob/master/source/objectid.rst
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [DebuggerDisplay("{ToString(),nq}")]
    [TypeConverter(typeof(ObjectIdTypeConverter))]
    [JsonConverter(typeof(SystemTextJsonObjectIdJsonConverter))]
    public readonly unsafe struct ObjectId :
        IComparable, IComparable<ObjectId>, IEquatable<ObjectId>
#if NET6_0_OR_GREATER
    , ISpanFormattable
#else
        , IFormattable
#endif
#if NET7_0_OR_GREATER
    , IMinMaxValue<ObjectId>
    , ISpanParsable<ObjectId>
    , IComparisonOperators<ObjectId, ObjectId, bool>
#endif
    {
        private const ushort MaximalChar = InternalHexTables.MaximalChar;
        private static readonly uint* TableToHex = InternalHexTables.TableToHex;
        private static readonly byte* TableFromHexToBytes = InternalHexTables.TableFromHexToBytes;

        /// <summary>
        ///     Represents the minimum possible value of an <see cref="ObjectId"/>.
        /// </summary>
        public static readonly ObjectId Min = NewObjectId(timestamp: 0, random: 0, increment: 0);

        /// <summary>
        ///     Represents the maximum possible value of an <see cref="ObjectId"/>.
        /// </summary>
        public static readonly ObjectId Max = NewObjectId(timestamp: 0xFFFFFFFF, random: 0xFFFFFFFFFF, increment: 0xFFFFFF);

        //DateTime.UnixEpoch
        private static readonly DateTime _unixEpoch = new(year: 1970, month: 1, day: 1, hour: 0, minute: 0, second: 0, DateTimeKind.Utc);
        private static readonly long _unixEpochTicks = _unixEpoch.Ticks;
        private const long UnixEpochSeconds = 62_135_596_800;
        private const double TicksPerSecond = 1E7;

        // timestamp (32 bits)
        private readonly byte _timestamp0;
        private readonly byte _timestamp1;
        private readonly byte _timestamp2;
        private readonly byte _timestamp3;

        // machine id (24 bits)
        private readonly byte _machine0;
        private readonly byte _machine1;
        private readonly byte _machine2;

        // process id (16 bits)
        private readonly byte _pid0;
        private readonly byte _pid1;

        // increment (24 bits)
        private readonly byte _increment0;
        private readonly byte _increment1;
        private readonly byte _increment2;

        /// <summary>
        ///     A read-only instance of the <see cref="ObjectId" /> structure whose value is all zeros.
        /// </summary>
        // ReSharper disable once RedundantDefaultMemberInitializer
        // ReSharper disable once MemberCanBePrivate.Global
        public static readonly ObjectId Empty = default;

        /// <summary>
        ///     Gets the random.
        /// </summary>
        // public long Random2 => ((long) _machine0 << 0x20) + ((long) _machine1 << 0x18) + ((long) _machine2 << 0x10) + (_pid0 << 0x8) + _pid1;
        public long Random => BinaryPrimitives.ReverseEndianness(Unsafe.ReadUnaligned<long>(ref Unsafe.AsRef(in _timestamp1))) &
                              0xFFFFFFFFFF;

        /// <summary>
        ///     Gets the increment.
        /// </summary>
        // public int Increment2 => (_increment0 << 0x10) + (_increment1 << 0x8) + _increment2;
        public int Increment => BinaryPrimitives.ReverseEndianness(Unsafe.ReadUnaligned<int>(ref Unsafe.AsRef(in _pid1))) & 0xFFFFFF;

        /// <summary>
        ///     Gets the timestamp.
        /// </summary>
        // public long Timestamp2 => ((long)_timestamp0 << 0x18) + (_timestamp1 << 0x10) + (_timestamp2 << 0x8) + _timestamp3;
        public long Timestamp =>
            BinaryPrimitives.ReverseEndianness(Unsafe.ReadUnaligned<int>(ref Unsafe.AsRef(in _timestamp0))) & 0xFFFFFFFF;

        /// <summary>
        ///     Gets the creation time (derived from the timestamp).
        /// </summary>
        public DateTimeOffset CreationTime => DateTimeOffset.FromUnixTimeSeconds(Timestamp);

        /// <summary>
        ///     Initializes a new instance of the <see cref="ObjectId" /> structure by using the specified array of bytes.
        /// </summary>
        /// <param name="bytes">A 12-element byte array containing values with which to initialize the <see cref="ObjectId" />.</param>
        /// <exception cref="ArgumentNullException"><paramref name="bytes" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException"><paramref name="bytes" /> is not 12 bytes long.</exception>
        // ReSharper disable once UnusedMember.Global
        public ObjectId(
            byte[] bytes
            )
        {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(bytes);
#else
            if (bytes is null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }
#endif

            if (bytes.Length != 12)
            {
                throw new ArgumentException("Byte array for ObjectId must be exactly 12 bytes long.", nameof(bytes));
            }

            this = Unsafe.ReadUnaligned<ObjectId>(ref MemoryMarshal.GetReference(new ReadOnlySpan<byte>(bytes)));
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ObjectId" /> structure by using the specified byte pointer.
        /// </summary>
        /// <param name="bytes">A byte pointer containing bytes which used to initialize the <see cref="ObjectId" />.</param>
        // ReSharper disable once UnusedMember.Global
        public ObjectId(
            byte* bytes
            )
        {
            this = Unsafe.ReadUnaligned<ObjectId>(bytes);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ObjectId" /> structure by using the value represented by the specified
        ///     read-only span of bytes.
        /// </summary>
        /// <param name="bytes">
        ///     A read-only span containing the bytes representing the <see cref="ObjectId" />. The span must be
        ///     exactly 12 bytes long.
        /// </param>
        /// <exception cref="ArgumentException"><paramref name="bytes" /> is not 12 bytes long.</exception>
        // ReSharper disable once UnusedMember.Global
        public ObjectId(
            ReadOnlySpan<byte> bytes
            )
        {
            if (bytes.Length != 12)
            {
                throw new ArgumentException("Byte array for ObjectId must be exactly 12 bytes long.", nameof(bytes));
            }

            this = Unsafe.ReadUnaligned<ObjectId>(ref MemoryMarshal.GetReference(bytes));
        }

        private ObjectId(
            long timestamp,
            long random,
            int increment
            )
        {
            ref var timestampPtr = ref Unsafe.As<long, byte>(ref timestamp);
            _timestamp0 = Unsafe.Add(ref timestampPtr, elementOffset: 3);
            _timestamp1 = Unsafe.Add(ref timestampPtr, elementOffset: 2);
            _timestamp2 = Unsafe.Add(ref timestampPtr, elementOffset: 1);
            _timestamp3 = Unsafe.Add(ref timestampPtr, elementOffset: 0);

            ref var randomPtr = ref Unsafe.As<long, byte>(ref random);
            _machine0 = Unsafe.Add(ref randomPtr, elementOffset: 4);
            _machine1 = Unsafe.Add(ref randomPtr, elementOffset: 3);
            _machine2 = Unsafe.Add(ref randomPtr, elementOffset: 2);
            _pid0 = Unsafe.Add(ref randomPtr, elementOffset: 1);
            _pid1 = Unsafe.Add(ref randomPtr, elementOffset: 0);

            ref var incrementPtr = ref Unsafe.As<int, byte>(ref increment);
            _increment0 = Unsafe.Add(ref incrementPtr, elementOffset: 2);
            _increment1 = Unsafe.Add(ref incrementPtr, elementOffset: 1);
            _increment2 = Unsafe.Add(ref incrementPtr, elementOffset: 0);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ObjectId" /> structure by using the value represented by the specified
        ///     values for the timestamp, random, and increment fields.
        /// </summary>
        /// <param name="timestamp">A 4-byte value representing the timestamp.</param>
        /// <param name="random">A 5-byte value representing the random value.</param>
        /// <param name="increment">A 3-byte value representing the increment value.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if any of the input values are invalid.</exception>
        public static ObjectId NewObjectId(long timestamp, long random, int increment)
        {
            if (timestamp < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(timestamp), timestamp, $"'{timestamp}' must be a non-negative value.");
            }

            if (timestamp > 0xFFFFFFFF)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(timestamp),
                    timestamp,
                    $"'{timestamp}' must be less than or equal to '{0xFFFFFFFF}'."
                );
            }

            if (random < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(random), random, $"'{random}' must be a non-negative value.");
            }

            if (random > 0xFFFFFFFFFF)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(random),
                    random,
                    $"'{random}' must be less than or equal to '{0xFFFFFFFFFF}'."
                );
            }

            if (increment < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(increment), increment, $"'{increment}' must be a non-negative value.");
            }

            if (increment > 0xFFFFFF)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(increment),
                    increment,
                    $"'{increment}' must be less than or equal to '{0xFFFFFF}'."
                );
            }

            return new ObjectId(timestamp, random, increment);
        }

        /// <summary>
        ///     Returns a 12-element byte array that contains the value of this instance.
        /// </summary>
        /// <returns>A 12-element byte array.</returns>
        public byte[] ToByteArray()
        {
            var result = new byte[12];

            Unsafe.WriteUnaligned(ref result[0], this);

            return result;
        }

        /// <summary>
        ///     Tries to write the current <see cref="ObjectId" /> instance into a span of bytes.
        /// </summary>
        /// <param name="destination">
        ///     When this method returns <see langword="true" />, the <see cref="ObjectId" /> as a span of
        ///     bytes.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if the <see cref="ObjectId" /> is successfully written to the specified span;
        ///     <see langword="false" />
        ///     otherwise.
        /// </returns>
        public bool TryWriteBytes(Span<byte> destination)
        {
            if (destination.Length < 12)
            {
                return false;
            }

            Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(destination), this);
            return true;
        }

        /// <summary>
        ///     Compares this instance to a specified object or <see cref="ObjectId" /> and returns an indication of their relative
        ///     values.
        /// </summary>
        /// <param name="obj">An object to compare, or <see langword="null" />.</param>
        /// <returns>A signed number indicating the relative values of this instance and <paramref name="obj" />.</returns>
        /// <exception cref="ArgumentException"><paramref name="obj" /> must be of type <see cref="ObjectId" />.</exception>
        public int CompareTo(
            object? obj
            )
        {
            if (obj is null)
            {
                return 1;
            }

            if (!(obj is ObjectId))
            {
                throw new ArgumentException("Object must be of type ObjectId.", nameof(obj));
            }

            var other = (ObjectId) obj;
            if (other._timestamp0 != _timestamp0)
            {
                return _timestamp0 < other._timestamp0 ? -1 : 1;
            }

            if (other._timestamp1 != _timestamp1)
            {
                return _timestamp1 < other._timestamp1 ? -1 : 1;
            }

            if (other._timestamp2 != _timestamp2)
            {
                return _timestamp2 < other._timestamp2 ? -1 : 1;
            }

            if (other._timestamp3 != _timestamp3)
            {
                return _timestamp3 < other._timestamp3 ? -1 : 1;
            }

            if (other._machine0 != _machine0)
            {
                return _machine0 < other._machine0 ? -1 : 1;
            }

            if (other._machine1 != _machine1)
            {
                return _machine1 < other._machine1 ? -1 : 1;
            }

            if (other._machine2 != _machine2)
            {
                return _machine2 < other._machine2 ? -1 : 1;
            }

            if (other._pid0 != _pid0)
            {
                return _pid0 < other._pid0 ? -1 : 1;
            }

            if (other._pid1 != _pid1)
            {
                return _pid1 < other._pid1 ? -1 : 1;
            }

            if (other._increment0 != _increment0)
            {
                return _increment0 < other._increment0 ? -1 : 1;
            }

            if (other._increment1 != _increment1)
            {
                return _increment1 < other._increment1 ? -1 : 1;
            }

            if (other._increment2 != _increment2)
            {
                return _increment2 < other._increment2 ? -1 : 1;
            }

            return 0;
        }

        /// <summary>
        ///     Compares this instance to a specified <see cref="ObjectId" /> object and returns an indication of their relative
        ///     values.
        /// </summary>
        /// <param name="other">An <see cref="ObjectId" /> object to compare to this instance.</param>
        /// <returns>A signed number indicating the relative values of this instance and <paramref name="other" />.</returns>
        public int CompareTo(
            ObjectId other
            )
        {
            if (other._timestamp0 != _timestamp0)
            {
                return _timestamp0 < other._timestamp0 ? -1 : 1;
            }

            if (other._timestamp1 != _timestamp1)
            {
                return _timestamp1 < other._timestamp1 ? -1 : 1;
            }

            if (other._timestamp2 != _timestamp2)
            {
                return _timestamp2 < other._timestamp2 ? -1 : 1;
            }

            if (other._timestamp3 != _timestamp3)
            {
                return _timestamp3 < other._timestamp3 ? -1 : 1;
            }

            if (other._machine0 != _machine0)
            {
                return _machine0 < other._machine0 ? -1 : 1;
            }

            if (other._machine1 != _machine1)
            {
                return _machine1 < other._machine1 ? -1 : 1;
            }

            if (other._machine2 != _machine2)
            {
                return _machine2 < other._machine2 ? -1 : 1;
            }

            if (other._pid0 != _pid0)
            {
                return _pid0 < other._pid0 ? -1 : 1;
            }

            if (other._pid1 != _pid1)
            {
                return _pid1 < other._pid1 ? -1 : 1;
            }

            if (other._increment0 != _increment0)
            {
                return _increment0 < other._increment0 ? -1 : 1;
            }

            if (other._increment1 != _increment1)
            {
                return _increment1 < other._increment1 ? -1 : 1;
            }

            if (other._increment2 != _increment2)
            {
                return _increment2 < other._increment2 ? -1 : 1;
            }

            return 0;
        }

        /// <summary>
        ///     Returns a value that indicates whether two instances of <see cref="ObjectId" /> represent the same value.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if <paramref name="obj" /> is <see cref="ObjectId" /> that has the same value as this
        ///     instance; otherwise,
        ///     <see langword="false" />.
        /// </returns>
        public override bool Equals(
            object? obj
            )
        {
            if (obj is ObjectId other)
            {
                ref var l = ref Unsafe.As<ObjectId, int>(ref Unsafe.AsRef(in this));
                ref var r = ref Unsafe.As<ObjectId, int>(ref Unsafe.AsRef(in other));

                return l == r
                       && Unsafe.Add(ref l, elementOffset: 1) == Unsafe.Add(ref r, elementOffset: 1)
                       && Unsafe.Add(ref l, elementOffset: 2) == Unsafe.Add(ref r, elementOffset: 2);
            }

            return false;
        }

        /// <summary>
        ///     Returns a value indicating whether this instance and a specified <see cref="ObjectId" /> object represent the same
        ///     value.
        /// </summary>
        /// <param name="other">An object to compare to this instance.</param>
        /// <returns>
        ///     <see langword="true" /> if <paramref name="other" /> is equal to this instance; otherwise,
        ///     <see langword="false" />.
        /// </returns>
        public bool Equals(
            ObjectId other
            )
        {
            ref var l = ref Unsafe.As<ObjectId, int>(ref Unsafe.AsRef(in this));
            ref var r = ref Unsafe.As<ObjectId, int>(ref Unsafe.AsRef(in other));

            return l == r
                   && Unsafe.Add(ref l, elementOffset: 1) == Unsafe.Add(ref r, elementOffset: 1)
                   && Unsafe.Add(ref l, elementOffset: 2) == Unsafe.Add(ref r, elementOffset: 2);
        }

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code for this instance.</returns>
        public override int GetHashCode()
        {
            ref var r = ref Unsafe.As<ObjectId, int>(ref Unsafe.AsRef(in this));
            return r ^ Unsafe.Add(ref r, elementOffset: 1) ^ Unsafe.Add(ref r, elementOffset: 2);
        }

        /// <summary>
        ///     Indicates whether the values of two specified <see cref="ObjectId" /> objects are equal.
        /// </summary>
        /// <param name="left">The first object to compare.</param>
        /// <param name="right">The second object to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are equal; otherwise,
        ///     <see langword="false" />.
        /// </returns>
        public static bool operator ==(
            ObjectId left,
            ObjectId right
            )
        {
            ref var l = ref Unsafe.As<ObjectId, int>(ref Unsafe.AsRef(in left));
            ref var r = ref Unsafe.As<ObjectId, int>(ref Unsafe.AsRef(in right));

            return l == r
                   && Unsafe.Add(ref l, elementOffset: 1) == Unsafe.Add(ref r, elementOffset: 1)
                   && Unsafe.Add(ref l, elementOffset: 2) == Unsafe.Add(ref r, elementOffset: 2);
        }

        /// <summary>
        ///     Indicates whether the values of two specified <see cref="ObjectId" /> objects are not equal.
        /// </summary>
        /// <param name="left">The first object to compare.</param>
        /// <param name="right">The second object to compare.</param>
        /// <returns>
        ///     <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise,
        ///     <see langword="false" />.
        /// </returns>
        public static bool operator !=(
            ObjectId left,
            ObjectId right
            )
        {
            ref var l = ref Unsafe.As<ObjectId, int>(ref Unsafe.AsRef(in left));
            ref var r = ref Unsafe.As<ObjectId, int>(ref Unsafe.AsRef(in right));

            return l != r
                   || Unsafe.Add(ref l, elementOffset: 1) != Unsafe.Add(ref r, elementOffset: 1)
                   || Unsafe.Add(ref l, elementOffset: 2) != Unsafe.Add(ref r, elementOffset: 2);
        }

        /// <summary>
        ///     Tries to format the value of the current instance into the provided span of characters.
        /// </summary>
        /// <param name="destination">
        ///     When this method returns <see langword="true" />, the <see cref="ObjectId" /> as a span of
        ///     characters.
        /// </param>
        /// <param name="charsWritten">
        ///     When this method returns <see langword="true" />, the number of characters written in
        ///     <paramref name="destination" />.
        /// </param>
        /// <param name="format">
        ///     A read-only span containing the character representing one of the following specifiers that indicates how to format
        ///     the value of this <see cref="ObjectId" />. The format parameter can be "N". If format is <see langword="null" /> or
        ///     an empty string (""), "N" is used.
        /// </param>
        /// <returns><see langword="true" /> if the formatting operation was successful; <see langword="false" /> otherwise.</returns>
        public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default)
        {
            if (format.Length == 0)
            {
#if NETSTANDARD2_0
                format = "N".AsSpan();
#else
            format = "N";
#endif
            }

            if (format.Length != 1)
            {
                charsWritten = 0;
                return false;
            }

            switch ((char) (format[0] | 0x20))
            {
                case 'n':
                {
                    if (destination.Length < 24)
                    {
                        charsWritten = 0;
                        return false;
                    }

                    fixed (char* objectIdChars = &destination.GetPinnableReference())
                    {
                        FormatN(objectIdChars);
                    }

                    charsWritten = 24;
                    return true;
                }
                default:
                {
                    charsWritten = 0;
                    return false;
                }
            }
        }

        // ReSharper disable once CommentTypo
        /// <summary>
        ///     Returns a string representation of the value of this instance.
        /// </summary>
        /// <returns>
        ///     The value of this <see cref="ObjectId" />, formatted by using the "N" format specifier as follows:
        ///     xxxxxxxxxxxxxxxxxxxxxxxx
        /// </returns>
        public override string ToString()
        {
            return ToString("N", formatProvider: null);
        }

        /// <summary>
        ///     Returns a string representation of the value of this <see cref="ObjectId" /> instance, according to the provided
        ///     format specifier.
        /// </summary>
        /// <param name="format">
        ///     A single format specifier that indicates how to format the value of this <see cref="ObjectId" />. The format
        ///     parameter can
        ///     be "N". If format is <see langword="null" /> or an empty string (""), "N" is used.
        /// </param>
        /// <returns>
        ///     The value of this <see cref="ObjectId" />, represented as a series of lowercase hexadecimal digits in the
        ///     specified format.
        /// </returns>
        // ReSharper disable once UnusedMember.Global
        public string ToString(string? format)
        {
            // ReSharper disable once IntroduceOptionalParameters.Global
            return ToString(format, formatProvider: null);
        }

        /// <summary>
        ///     Returns a string representation of the value of this <see cref="ObjectId" /> instance, according to the provided
        ///     format specifier and culture-specific format information.
        /// </summary>
        /// <param name="format">
        ///     A single format specifier that indicates how to format the value of this <see cref="ObjectId" />. The format
        ///     parameter can
        ///     be "N". If format is <see langword="null" /> or an empty string (""), "N" is used.
        /// </param>
        /// <param name="formatProvider">An object that supplies culture-specific formatting information.</param>
        /// <returns>
        ///     The value of this <see cref="ObjectId" />, represented as a series of lowercase hexadecimal digits in the
        ///     specified format.
        /// </returns>
        /// <exception cref="FormatException">Thrown if the format string is not a valid value ("N" or "n").</exception>
        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            format ??= "N";

            if (string.IsNullOrEmpty(format))
            {
                format = "N";
            }

            if (format.Length != 1)
            {
                throw new FormatException("Format string can be only \"N\", \"n\".");
            }

            switch ((char) (format[0] | 0x20))
            {
                case 'n':
                {
                    var objectIdString = new string(c: '\0', count: 24);
#if NETCOREAPP3_1 || NET5_0 || NET6_0_OR_GREATER
                    fixed (char* objectIdChars = &objectIdString.GetPinnableReference())
#endif
#if NETSTANDARD2_0 || NETSTANDARD2_1
                    fixed (char* objectIdChars = objectIdString)
#endif
                    {
                        FormatN(objectIdChars);
                    }

                    return objectIdString;
                }
                default:
                    throw new FormatException(
                        "Format string can be only \"N\", \"n\"."
                    );
            }
        }

#if NET6_0_OR_GREATER
    /// <summary>
    ///     Tries to format the value of the current instance into the provided span of characters.
    /// </summary>
    /// <param name="destination">
    ///     When this method returns <see langword="true" />, the <see cref="ObjectId" /> as a span of
    ///     characters.
    /// </param>
    /// <param name="charsWritten">
    ///     When this method returns <see langword="true" />, the number of characters written in
    ///     <paramref name="destination" />.
    /// </param>
    /// <param name="format">
    ///     A read-only span containing the character representing one of the following specifiers that indicates how to format
    ///     the value of this <see cref="ObjectId" />. The format parameter can be "N". If format is <see langword="null" /> or
    ///     an empty string (""), "N" is used.
    /// </param>
    /// <param name="provider">
    ///     An optional object that supplies culture-specific formatting information for
    ///     <paramref name="destination" />.
    /// </param>
    /// <returns><see langword="true" /> if the formatting operation was successful; <see langword="false" /> otherwise.</returns>
    bool ISpanFormattable.TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        if (format.Length == 0)
        {
            format = "N";
        }

        if (format.Length != 1)
        {
            charsWritten = 0;
            return false;
        }

        switch ((char)(format[0] | 0x20))
        {
            case 'n':
                {
                    if (destination.Length < 24)
                    {
                        charsWritten = 0;
                        return false;
                    }

                    fixed (char* objectIdChars = &destination.GetPinnableReference())
                    {
                        FormatN(objectIdChars);
                    }

                    charsWritten = 24;
                    return true;
                }
            default:
                {
                    charsWritten = 0;
                    return false;
                }
        }
    }
#endif

#if NETCOREAPP3_1_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#endif
#if NETSTANDARD2_0 || NETSTANDARD2_1
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private void FormatN(char* dest)
        {
            // ReSharper disable once CommentTypo
            // dddddddddddddddddddddddd
            var destUints = (uint*) dest;
            destUints[0] = TableToHex[_timestamp0];
            destUints[1] = TableToHex[_timestamp1];
            destUints[2] = TableToHex[_timestamp2];
            destUints[3] = TableToHex[_timestamp3];
            destUints[4] = TableToHex[_machine0];
            destUints[5] = TableToHex[_machine1];
            destUints[6] = TableToHex[_machine2];
            destUints[7] = TableToHex[_pid0];
            destUints[8] = TableToHex[_pid1];
            destUints[9] = TableToHex[_increment0];
            destUints[10] = TableToHex[_increment1];
            destUints[11] = TableToHex[_increment2];
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ObjectId" /> structure by using the value represented by the specified
        ///     string.
        /// </summary>
        /// <param name="input">A string that contains a ObjectId.</param>
        /// <exception cref="ArgumentNullException"><paramref name="input" /> is <see langword="null" />.</exception>
        public ObjectId(string input)
        {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(input);
#else
            if (input is null)
            {
                throw new ArgumentNullException(nameof(input));
            }
#endif

            var result = new ObjectId();
            var resultPtr = (byte*) &result;

#if NETCOREAPP3_1 || NET5_0_OR_GREATER
        fixed (char* objectIdStringPtr = &input.GetPinnableReference())
#else
            fixed (char* objectIdStringPtr = input)
#endif
            {
                ParseWithExceptions(new ReadOnlySpan<char>(objectIdStringPtr, input.Length), objectIdStringPtr, resultPtr);
            }

            this = result;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ObjectId" /> structure by using the value represented by the specified
        ///     read-only span of
        ///     characters.
        /// </summary>
        /// <param name="input">A read-only span of characters that contains a ObjectId.</param>
        /// <exception cref="FormatException">
        ///     <paramref name="input" /> is empty or contains unrecognized <see cref="ObjectId" />
        ///     format.
        /// </exception>
        // ReSharper disable once UnusedMember.Global
        public ObjectId(ReadOnlySpan<char> input)
        {
            if (input.IsEmpty)
            {
                throw new FormatException("Unrecognized ObjectId format.");
            }

            var result = new ObjectId();
            var resultPtr = (byte*) &result;
            fixed (char* objectIdStringPtr = &input.GetPinnableReference())
            {
                ParseWithExceptions(input, objectIdStringPtr, resultPtr);
            }

            this = result;
        }

        /// <summary>
        ///     Converts the string representation of a ObjectId to the equivalent <see cref="ObjectId" /> structure.
        /// </summary>
        /// <param name="input">The string to convert.</param>
        /// <returns>A structure that contains the value that was parsed.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="input" /> is <see langword="null" />.</exception>
        public static ObjectId Parse(
            string input
            )
        {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(input);
#else
            if (input is null)
            {
                throw new ArgumentNullException(nameof(input));
            }
#endif

            var result = new ObjectId();
            var resultPtr = (byte*) &result;

#if NETCOREAPP3_1 || NET5_0_OR_GREATER
        fixed (char* objectIdStringPtr = &input.GetPinnableReference())
#else
            fixed (char* objectIdStringPtr = input)
#endif
            {
                ParseWithExceptions(new ReadOnlySpan<char>(objectIdStringPtr, input.Length), objectIdStringPtr, resultPtr);
            }

            return result;
        }

        /// <summary>
        ///     Converts a read-only character span that represents a ObjectId to the equivalent <see cref="ObjectId" /> structure.
        /// </summary>
        /// <param name="input">A read-only span containing the bytes representing a <see cref="ObjectId" />.</param>
        /// <returns>A structure that contains the value that was parsed.</returns>
        /// <exception cref="FormatException"><paramref name="input" /> is not in a recognized format.</exception>
        public static ObjectId Parse(ReadOnlySpan<char> input)
        {
            if (input.IsEmpty)
            {
                throw new FormatException("Unrecognized ObjectId format.");
            }

            var result = new ObjectId();
            var resultPtr = (byte*) &result;
            fixed (char* objectIdStringPtr = &input.GetPinnableReference())
            {
                ParseWithExceptions(input, objectIdStringPtr, resultPtr);
            }

            return result;
        }

        /// <summary>
        ///     Converts the string representation of a <see cref="ObjectId" /> to the equivalent <see cref="ObjectId" />
        ///     structure, provided that the string
        ///     is in the specified format.
        /// </summary>
        /// <param name="input">The <see cref="ObjectId" /> to convert.</param>
        /// <param name="format">
        ///     One of the following specifiers that indicates the exact format to use when interpreting <paramref name="input" />:
        ///     "N".
        /// </param>
        /// <returns>A structure that contains the value that was parsed.</returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="input" /> or <paramref name="format" /> is
        ///     <see langword="null" />.
        /// </exception>
        /// <exception cref="FormatException">
        ///     <paramref name="input" /> is not in the format specified by
        ///     <paramref name="format" />.
        /// </exception>
        public static ObjectId ParseExact(string input, string format)
        {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(format);
#else
            if (input is null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (format is null)
            {
                throw new ArgumentNullException(nameof(format));
            }
#endif

            var result = new ObjectId();
            var resultPtr = (byte*) &result;
            switch ((char) (format[0] | 0x20))
            {
                case 'n':
                {
#if NETCOREAPP3_1 || NET5_0_OR_GREATER
                fixed (char* objectIdStringPtr = &input.GetPinnableReference())
#else
                    fixed (char* objectIdStringPtr = input)
#endif
                    {
                        ParseWithExceptionsN((uint) input.Length, objectIdStringPtr, resultPtr);
                    }

                    return result;
                }
                default:
                {
                    throw new FormatException(
                        "Format string can be only \"N\", \"n\"."
                    );
                }
            }
        }

        /// <summary>
        ///     Converts the character span representation of a <see cref="ObjectId" /> to the equivalent <see cref="ObjectId" />
        ///     structure, provided that the
        ///     string is in the specified format.
        /// </summary>
        /// <param name="input">A read-only span containing the characters representing the <see cref="ObjectId" /> to convert.</param>
        /// <param name="format">
        ///     A read-only span of characters representing one of the following specifiers that indicates the exact format to use
        ///     when interpreting <paramref name="input" />: "N".
        /// </param>
        /// <returns>A structure that contains the value that was parsed.</returns>
        /// <exception cref="FormatException">
        ///     <paramref name="input" /> is not in the format specified by
        ///     <paramref name="format" />.
        /// </exception>
        public static ObjectId ParseExact(ReadOnlySpan<char> input, ReadOnlySpan<char> format)
        {
            if (input.IsEmpty)
            {
                throw new FormatException("Unrecognized ObjectId format.");
            }

            if (format.Length != 1)
            {
                throw new FormatException(
                    "Format string can be only \"N\", \"n\"."
                );
            }

            var result = new ObjectId();
            var resultPtr = (byte*) &result;
            switch ((char) (format[0] | 0x20))
            {
                case 'n':
                {
                    fixed (char* objectIdStringPtr = &input.GetPinnableReference())
                    {
                        ParseWithExceptionsN((uint) input.Length, objectIdStringPtr, resultPtr);
                    }

                    return result;
                }
                default:
                {
                    throw new FormatException(
                        "Format string can be only \"N\", \"n\"."
                    );
                }
            }
        }

        /// <summary>
        ///     Converts the string representation of a ObjectId to the equivalent <see cref="ObjectId" /> structure.
        /// </summary>
        /// <param name="input">A string containing the ObjectId to convert.</param>
        /// <param name="output">
        ///     A <see cref="ObjectId" /> instance to contain the parsed value. If the method returns <see langword="true" />,
        ///     <paramref name="output" /> contains a valid <see cref="ObjectId" />.
        ///     If the method returns <see langword="false" />, <paramref name="output" /> equals <see cref="Empty" />.
        /// </param>
        /// <returns><see langword="true" /> if the parse operation was successful; otherwise, <see langword="false" />.</returns>
        public static bool TryParse(
            string? input,
            out ObjectId output
            )
        {
            if (input is null)
            {
                output = default;
                return false;
            }

            var result = new ObjectId();
            var resultPtr = (byte*) &result;

#if NETCOREAPP3_1 || NET5_0_OR_GREATER
        fixed (char* objectIdStringPtr = &input.GetPinnableReference())
#else
            fixed (char* objectIdStringPtr = input)
#endif
            {
                if (ParseWithoutExceptions(input.AsSpan(), objectIdStringPtr, resultPtr))
                {
                    output = result;
                    return true;
                }
            }

            output = default;
            return false;
        }

        /// <summary>
        ///     Converts the specified read-only span of characters containing the representation of a ObjectId to the equivalent
        ///     <see cref="ObjectId" />
        ///     structure.
        /// </summary>
        /// <param name="input">A span containing the characters representing the ObjectId to convert.</param>
        /// <param name="output">
        ///     A <see cref="ObjectId" /> instance to contain the parsed value. If the method returns <see langword="true" />,
        ///     <paramref name="output" /> contains a valid <see cref="ObjectId" />.
        ///     If the method returns <see langword="false" />, <paramref name="output" /> equals <see cref="Empty" />.
        /// </param>
        /// <returns><see langword="true" /> if the parse operation was successful; otherwise, <see langword="false" />.</returns>
        public static bool TryParse(
            ReadOnlySpan<char> input,
            out ObjectId output
            )
        {
            if (input.IsEmpty)
            {
                output = default;
                return false;
            }

            var result = new ObjectId();
            var resultPtr = (byte*) &result;
            fixed (char* objectIdStringPtr = &input.GetPinnableReference())
            {
                if (ParseWithoutExceptions(input, objectIdStringPtr, resultPtr))
                {
                    output = result;
                    return true;
                }
            }

            output = default;
            return false;
        }

        /// <summary>
        ///     Converts the specified read-only span bytes of UTF-8 characters containing the representation of a ObjectId to the
        ///     equivalent
        ///     <see cref="ObjectId" /> structure.
        /// </summary>
        /// <param name="objectIdUtf8String">A span containing the bytes of UTF-8 characters representing the ObjectId to convert.</param>
        /// <param name="output">
        ///     A <see cref="ObjectId" /> instance to contain the parsed value. If the method returns <see langword="true" />,
        ///     <paramref name="output" /> contains a valid <see cref="ObjectId" />.
        ///     If the method returns <see langword="false" />, <paramref name="output" /> equals <see cref="Empty" />.
        /// </param>
        /// <returns><see langword="true" /> if the parse operation was successful; otherwise, <see langword="false" />.</returns>
        public static bool TryParse(ReadOnlySpan<byte> objectIdUtf8String, out ObjectId output)
        {
            if (objectIdUtf8String.IsEmpty)
            {
                output = default;
                return false;
            }

            var result = new ObjectId();
            var resultPtr = (byte*) &result;
            fixed (byte* objectIdUtf8StringPtr = &objectIdUtf8String.GetPinnableReference())
            {
                if (ParseWithoutExceptionsUtf8(objectIdUtf8String, objectIdUtf8StringPtr, resultPtr))
                {
                    output = result;
                    return true;
                }
            }

            output = default;
            return false;
        }

        /// <summary>
        ///     Converts the string representation of a ObjectId to the equivalent <see cref="ObjectId" /> structure, provided that
        ///     the string is in the
        ///     specified format.
        /// </summary>
        /// <param name="input">The ObjectId to convert.</param>
        /// <param name="format">
        ///     One of the following specifiers that indicates the exact format to use when interpreting <paramref name="input" />:
        ///     "N".
        /// </param>
        /// <param name="output">
        ///     A <see cref="ObjectId" /> instance to contain the parsed value. If the method returns <see langword="true" />,
        ///     <paramref name="output" /> contains a valid <see cref="ObjectId" />.
        ///     If the method returns <see langword="false" />, <paramref name="output" /> equals <see cref="Empty" />.
        /// </param>
        /// <returns><see langword="true" /> if the parse operation was successful; otherwise, <see langword="false" />.</returns>
        public static bool TryParseExact(string? input, string? format, out ObjectId output)
        {
            if (input is null || format?.Length != 1)
            {
                output = default;
                return false;
            }

            var result = new ObjectId();
            var resultPtr = (byte*) &result;
            var parsed = false;

            switch ((char) (format[0] | 0x20))
            {
                case 'n':
                {
#if NETCOREAPP3_1 || NET5_0 || NET6_0_OR_GREATER
                fixed (char* objectIdStringPtr = &input.GetPinnableReference())
#endif
#if NETSTANDARD2_0 || NETSTANDARD2_1
                    fixed (char* objectIdStringPtr = input)
#endif
                    {
                        parsed = ParseWithoutExceptionsN((uint) input.Length, objectIdStringPtr, resultPtr);
                    }

                    break;
                }
            }

            if (parsed)
            {
                output = result;
                return true;
            }

            output = default;
            return false;
        }

        /// <summary>
        ///     Converts span of characters representing the ObjectId to the equivalent <see cref="ObjectId" /> structure, provided
        ///     that the string is in the
        ///     specified format.
        /// </summary>
        /// <param name="input">A read-only span containing the characters representing the ObjectId to convert.</param>
        /// <param name="format">
        ///     A read-only span containing a character representing one of the following specifiers that indicates the exact
        ///     format
        ///     to use when interpreting <paramref name="input" />: "N".
        /// </param>
        /// <param name="output">
        ///     A <see cref="ObjectId" /> instance to contain the parsed value. If the method returns <see langword="true" />,
        ///     <paramref name="output" /> contains a valid <see cref="ObjectId" />.
        ///     If the method returns <see langword="false" />, <paramref name="output" /> equals <see cref="Empty" />.
        /// </param>
        /// <returns><see langword="true" /> if the parse operation was successful; otherwise, <see langword="false" />.</returns>
        public static bool TryParseExact(ReadOnlySpan<char> input, ReadOnlySpan<char> format, out ObjectId output)
        {
            if (format.Length != 1)
            {
                output = default;
                return false;
            }

            var result = new ObjectId();
            var resultPtr = (byte*) &result;
            var parsed = false;

            switch ((char) (format[0] | 0x20))
            {
                case 'n':
                {
                    fixed (char* objectIdStringPtr = &input.GetPinnableReference())
                    {
                        parsed = ParseWithoutExceptionsN((uint) input.Length, objectIdStringPtr, resultPtr);
                    }

                    break;
                }
            }

            if (parsed)
            {
                output = result;
                return true;
            }

            output = default;
            return false;
        }

        private static bool ParseWithoutExceptions(ReadOnlySpan<char> objectIdString, char* objectIdStringPtr, byte* resultPtr)
        {
            var length = (uint) objectIdString.Length;
            return length != 0u && ParseWithoutExceptionsN(length, objectIdStringPtr, resultPtr);
        }

        private static bool ParseWithoutExceptionsN(uint objectIdStringLength, char* objectIdStringPtr, byte* resultPtr)
        {
            return objectIdStringLength == 24u && TryParsePtrN(objectIdStringPtr, resultPtr);
        }

#if NETCOREAPP3_0_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#else
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static bool TryParsePtrN(
            char* value,
            byte* resultPtr
            )
        {
            // e.g. "54759eb3c090d83494e2d804"
            byte hi;
            byte lo;
            // 0 byte
            if (value[0] < MaximalChar
                && (hi = TableFromHexToBytes[value[0]]) != 0xFF
                && value[1] < MaximalChar
                && (lo = TableFromHexToBytes[value[1]]) != 0xFF)
            {
                resultPtr[0] = (byte) ((hi << 4) | lo);

                // 1 byte
                if (value[2] < MaximalChar
                    && (hi = TableFromHexToBytes[value[2]]) != 0xFF
                    && value[3] < MaximalChar
                    && (lo = TableFromHexToBytes[value[3]]) != 0xFF)
                {
                    resultPtr[1] = (byte) ((hi << 4) | lo);

                    // 2 byte
                    if (value[4] < MaximalChar
                        && (hi = TableFromHexToBytes[value[4]]) != 0xFF
                        && value[5] < MaximalChar
                        && (lo = TableFromHexToBytes[value[5]]) != 0xFF)
                    {
                        resultPtr[2] = (byte) ((hi << 4) | lo);

                        // 3 byte
                        if (value[6] < MaximalChar
                            && (hi = TableFromHexToBytes[value[6]]) != 0xFF
                            && value[7] < MaximalChar
                            && (lo = TableFromHexToBytes[value[7]]) != 0xFF)
                        {
                            resultPtr[3] = (byte) ((hi << 4) | lo);

                            // 4 byte
                            if (value[8] < MaximalChar
                                && (hi = TableFromHexToBytes[value[8]]) != 0xFF
                                && value[9] < MaximalChar
                                && (lo = TableFromHexToBytes[value[9]]) != 0xFF)
                            {
                                resultPtr[4] = (byte) ((hi << 4) | lo);

                                // 5 byte
                                if (value[10] < MaximalChar
                                    && (hi = TableFromHexToBytes[value[10]]) != 0xFF
                                    && value[11] < MaximalChar
                                    && (lo = TableFromHexToBytes[value[11]]) != 0xFF)
                                {
                                    resultPtr[5] = (byte) ((hi << 4) | lo);

                                    // 6 byte
                                    if (value[12] < MaximalChar
                                        && (hi = TableFromHexToBytes[value[12]]) != 0xFF
                                        && value[13] < MaximalChar
                                        && (lo = TableFromHexToBytes[value[13]]) != 0xFF)
                                    {
                                        resultPtr[6] = (byte) ((hi << 4) | lo);

                                        // 7 byte
                                        if (value[14] < MaximalChar
                                            && (hi = TableFromHexToBytes[value[14]]) != 0xFF
                                            && value[15] < MaximalChar
                                            && (lo = TableFromHexToBytes[value[15]]) != 0xFF)
                                        {
                                            resultPtr[7] = (byte) ((hi << 4) | lo);

                                            // 8 byte
                                            if (value[16] < MaximalChar
                                                && (hi = TableFromHexToBytes[value[16]]) != 0xFF
                                                && value[17] < MaximalChar
                                                && (lo = TableFromHexToBytes[value[17]]) != 0xFF)
                                            {
                                                resultPtr[8] = (byte) ((hi << 4) | lo);

                                                // 9 byte
                                                if (value[18] < MaximalChar
                                                    && (hi = TableFromHexToBytes[value[18]]) != 0xFF
                                                    && value[19] < MaximalChar
                                                    && (lo = TableFromHexToBytes[value[19]]) != 0xFF)
                                                {
                                                    resultPtr[9] = (byte) ((hi << 4) | lo);

                                                    // 10 byte
                                                    if (value[20] < MaximalChar
                                                        && (hi = TableFromHexToBytes[value[20]]) != 0xFF
                                                        && value[21] < MaximalChar
                                                        && (lo = TableFromHexToBytes[value[21]]) != 0xFF)
                                                    {
                                                        resultPtr[10] = (byte) ((hi << 4) | lo);

                                                        // 11 byte
                                                        if (value[22] < MaximalChar
                                                            && (hi = TableFromHexToBytes[value[22]]) != 0xFF
                                                            && value[23] < MaximalChar
                                                            && (lo = TableFromHexToBytes[value[23]]) != 0xFF)
                                                        {
                                                            resultPtr[11] = (byte) ((hi << 4) | lo);

                                                            return true;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

#if NETCOREAPP3_0_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#else
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static bool TryParsePtrNUtf8(byte* value, byte* resultPtr)
        {
            // e.g. "54759eb3c090d83494e2d804"
            byte hi;
            byte lo;
            // 0 byte
            if (value[0] < MaximalChar
                && (hi = TableFromHexToBytes[value[0]]) != 0xFF
                && value[1] < MaximalChar
                && (lo = TableFromHexToBytes[value[1]]) != 0xFF)
            {
                resultPtr[0] = (byte) ((hi << 4) | lo);
                // 1 byte
                if (value[2] < MaximalChar
                    && (hi = TableFromHexToBytes[value[2]]) != 0xFF
                    && value[3] < MaximalChar
                    && (lo = TableFromHexToBytes[value[3]]) != 0xFF)
                {
                    resultPtr[1] = (byte) ((hi << 4) | lo);
                    // 2 byte
                    if (value[4] < MaximalChar
                        && (hi = TableFromHexToBytes[value[4]]) != 0xFF
                        && value[5] < MaximalChar
                        && (lo = TableFromHexToBytes[value[5]]) != 0xFF)
                    {
                        resultPtr[2] = (byte) ((hi << 4) | lo);
                        // 3 byte
                        if (value[6] < MaximalChar
                            && (hi = TableFromHexToBytes[value[6]]) != 0xFF
                            && value[7] < MaximalChar
                            && (lo = TableFromHexToBytes[value[7]]) != 0xFF)
                        {
                            resultPtr[3] = (byte) ((hi << 4) | lo);
                            // 4 byte
                            if (value[8] < MaximalChar
                                && (hi = TableFromHexToBytes[value[8]]) != 0xFF
                                && value[9] < MaximalChar
                                && (lo = TableFromHexToBytes[value[9]]) != 0xFF)
                            {
                                resultPtr[4] = (byte) ((hi << 4) | lo);
                                // 5 byte
                                if (value[10] < MaximalChar
                                    && (hi = TableFromHexToBytes[value[10]]) != 0xFF
                                    && value[11] < MaximalChar
                                    && (lo = TableFromHexToBytes[value[11]]) != 0xFF)
                                {
                                    resultPtr[5] = (byte) ((hi << 4) | lo);
                                    // 6 byte
                                    if (value[12] < MaximalChar
                                        && (hi = TableFromHexToBytes[value[12]]) != 0xFF
                                        && value[13] < MaximalChar
                                        && (lo = TableFromHexToBytes[value[13]]) != 0xFF)
                                    {
                                        resultPtr[6] = (byte) ((hi << 4) | lo);
                                        // 7 byte
                                        if (value[14] < MaximalChar
                                            && (hi = TableFromHexToBytes[value[14]]) != 0xFF
                                            && value[15] < MaximalChar
                                            && (lo = TableFromHexToBytes[value[15]]) != 0xFF)
                                        {
                                            resultPtr[7] = (byte) ((hi << 4) | lo);
                                            // 8 byte
                                            if (value[16] < MaximalChar
                                                && (hi = TableFromHexToBytes[value[16]]) != 0xFF
                                                && value[17] < MaximalChar
                                                && (lo = TableFromHexToBytes[value[17]]) != 0xFF)
                                            {
                                                resultPtr[8] = (byte) ((hi << 4) | lo);
                                                // 9 byte
                                                if (value[18] < MaximalChar
                                                    && (hi = TableFromHexToBytes[value[18]]) != 0xFF
                                                    && value[19] < MaximalChar
                                                    && (lo = TableFromHexToBytes[value[19]]) != 0xFF)
                                                {
                                                    resultPtr[9] = (byte) ((hi << 4) | lo);
                                                    // 10 byte
                                                    if (value[20] < MaximalChar
                                                        && (hi = TableFromHexToBytes[value[20]]) != 0xFF
                                                        && value[21] < MaximalChar
                                                        && (lo = TableFromHexToBytes[value[21]]) != 0xFF)
                                                    {
                                                        resultPtr[10] = (byte) ((hi << 4) | lo);
                                                        // 11 byte
                                                        if (value[22] < MaximalChar
                                                            && (hi = TableFromHexToBytes[value[22]]) != 0xFF
                                                            && value[23] < MaximalChar
                                                            && (lo = TableFromHexToBytes[value[23]]) != 0xFF)
                                                        {
                                                            resultPtr[11] = (byte) ((hi << 4) | lo);

                                                            return true;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        private static bool ParseWithoutExceptionsUtf8(ReadOnlySpan<byte> objectIdUtf8String, byte* objectIdUtf8StringPtr, byte* resultPtr)
        {
            var length = (uint) objectIdUtf8String.Length;

            return ParseWithoutExceptionsNUtf8(length, objectIdUtf8StringPtr, resultPtr);
        }

        private static bool ParseWithoutExceptionsNUtf8(uint objectIdStringLength, byte* objectIdUtf8StringPtr, byte* resultPtr)
        {
            return objectIdStringLength == 24u && TryParsePtrNUtf8(objectIdUtf8StringPtr, resultPtr);
        }

        private static void ParseWithExceptions(
            ReadOnlySpan<char> objectIdString,
            char* objectIdStringPtr,
            byte* resultPtr
            )
        {
            var length = (uint) objectIdString.Length;
            if (length == 0u)
            {
                throw new FormatException("Unrecognized ObjectId format.");
            }

            ParseWithExceptionsN(length, objectIdStringPtr, resultPtr);
        }

        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        private static void ParseWithExceptionsN(uint objectIdStringLength, char* objectIdStringPtr, byte* resultPtr)
        {
            if (objectIdStringLength != 24u)
            {
                // ReSharper disable once StringLiteralTypo
                throw new FormatException("ObjectId should contain only 24 digits xxxxxxxxxxxxxxxxxxxxxxxx.");
            }

            if (!TryParsePtrN(objectIdStringPtr, resultPtr))
            {
                throw new FormatException("ObjectId string should only contain hexadecimal characters.");
            }
        }

        /// <summary>
        ///     Compares two values to determine which is less.
        /// </summary>
        /// <param name="left">The value to compare with <paramref name="right" />.</param>
        /// <param name="right">The value to compare with <paramref name="left" />.</param>
        /// <returns>
        ///     <see langword="true" /> if <paramref name="left" /> is less than <paramref name="right" />; otherwise,
        ///     <see langword="false" />.
        /// </returns>
        public static bool operator <(ObjectId left, ObjectId right)
        {
            if (left._timestamp0 != right._timestamp0)
            {
                return left._timestamp0 < right._timestamp0;
            }

            if (left._timestamp1 != right._timestamp1)
            {
                return left._timestamp1 < right._timestamp1;
            }

            if (left._timestamp2 != right._timestamp2)
            {
                return left._timestamp2 < right._timestamp2;
            }

            if (left._timestamp3 != right._timestamp3)
            {
                return left._timestamp3 < right._timestamp3;
            }

            if (left._machine0 != right._machine0)
            {
                return left._machine0 < right._machine0;
            }

            if (left._machine1 != right._machine1)
            {
                return left._machine1 < right._machine1;
            }

            if (left._machine2 != right._machine2)
            {
                return left._machine2 < right._machine2;
            }

            if (left._pid0 != right._pid0)
            {
                return left._pid0 < right._pid0;
            }

            if (left._pid1 != right._pid1)
            {
                return left._pid1 < right._pid1;
            }

            if (left._increment0 != right._increment0)
            {
                return left._increment0 < right._increment0;
            }

            if (left._increment1 != right._increment1)
            {
                return left._increment1 < right._increment1;
            }

            if (left._increment2 != right._increment2)
            {
                return left._increment2 < right._increment2;
            }

            return false;
        }

        /// <summary>
        ///     Compares two values to determine which is less or equal.
        /// </summary>
        /// <param name="left">The value to compare with <paramref name="right" />.</param>
        /// <param name="right">The value to compare with <paramref name="left" />.</param>
        /// <returns>
        ///     <see langword="true" /> if <paramref name="left" /> is less than or equal to <paramref name="right" />; otherwise,
        ///     <see langword="false" />.
        /// </returns>
        public static bool operator <=(ObjectId left, ObjectId right)
        {
            if (left._timestamp0 != right._timestamp0)
            {
                return left._timestamp0 < right._timestamp0;
            }

            if (left._timestamp1 != right._timestamp1)
            {
                return left._timestamp1 < right._timestamp1;
            }

            if (left._timestamp2 != right._timestamp2)
            {
                return left._timestamp2 < right._timestamp2;
            }

            if (left._timestamp3 != right._timestamp3)
            {
                return left._timestamp3 < right._timestamp3;
            }

            if (left._machine0 != right._machine0)
            {
                return left._machine0 < right._machine0;
            }

            if (left._machine1 != right._machine1)
            {
                return left._machine1 < right._machine1;
            }

            if (left._machine2 != right._machine2)
            {
                return left._machine2 < right._machine2;
            }

            if (left._pid0 != right._pid0)
            {
                return left._pid0 < right._pid0;
            }

            if (left._pid1 != right._pid1)
            {
                return left._pid1 < right._pid1;
            }

            if (left._increment0 != right._increment0)
            {
                return left._increment0 < right._increment0;
            }

            if (left._increment1 != right._increment1)
            {
                return left._increment1 < right._increment1;
            }

            if (left._increment2 != right._increment2)
            {
                return left._increment2 < right._increment2;
            }

            return true;
        }

        /// <summary>
        ///     Compares two values to determine which is greater.
        /// </summary>
        /// <param name="left">The value to compare with <paramref name="right" />.</param>
        /// <param name="right">The value to compare with <paramref name="left" />.</param>
        /// <returns>
        ///     <see langword="true" /> if <paramref name="left" /> is greater than <paramref name="right" />;
        ///     otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator >(ObjectId left, ObjectId right)
        {
            if (left._timestamp0 != right._timestamp0)
            {
                return left._timestamp0 > right._timestamp0;
            }

            if (left._timestamp1 != right._timestamp1)
            {
                return left._timestamp1 > right._timestamp1;
            }

            if (left._timestamp2 != right._timestamp2)
            {
                return left._timestamp2 > right._timestamp2;
            }

            if (left._timestamp3 != right._timestamp3)
            {
                return left._timestamp3 > right._timestamp3;
            }

            if (left._machine0 != right._machine0)
            {
                return left._machine0 > right._machine0;
            }

            if (left._machine1 != right._machine1)
            {
                return left._machine1 > right._machine1;
            }

            if (left._machine2 != right._machine2)
            {
                return left._machine2 > right._machine2;
            }

            if (left._pid0 != right._pid0)
            {
                return left._pid0 > right._pid0;
            }

            if (left._pid1 != right._pid1)
            {
                return left._pid1 > right._pid1;
            }

            if (left._increment0 != right._increment0)
            {
                return left._increment0 > right._increment0;
            }

            if (left._increment1 != right._increment1)
            {
                return left._increment1 > right._increment1;
            }

            if (left._increment2 != right._increment2)
            {
                return left._increment2 > right._increment2;
            }

            return false;
        }

        /// <summary>
        ///     Compares two values to determine which is greater or equal.
        /// </summary>
        /// <param name="left">The value to compare with <paramref name="right" />.</param>
        /// <param name="right">The value to compare with <paramref name="left" />.</param>
        /// <returns>
        ///     <see langword="true" /> if <paramref name="left" /> is greater than or equal to <paramref name="right" />;
        ///     otherwise, <see langword="false" />.
        /// </returns>
        public static bool operator >=(ObjectId left, ObjectId right)
        {
            if (left._timestamp0 != right._timestamp0)
            {
                return left._timestamp0 > right._timestamp0;
            }

            if (left._timestamp1 != right._timestamp1)
            {
                return left._timestamp1 > right._timestamp1;
            }

            if (left._timestamp2 != right._timestamp2)
            {
                return left._timestamp2 > right._timestamp2;
            }

            if (left._timestamp3 != right._timestamp3)
            {
                return left._timestamp3 > right._timestamp3;
            }

            if (left._machine0 != right._machine0)
            {
                return left._machine0 > right._machine0;
            }

            if (left._machine1 != right._machine1)
            {
                return left._machine1 > right._machine1;
            }

            if (left._machine2 != right._machine2)
            {
                return left._machine2 > right._machine2;
            }

            if (left._pid0 != right._pid0)
            {
                return left._pid0 > right._pid0;
            }

            if (left._pid1 != right._pid1)
            {
                return left._pid1 > right._pid1;
            }

            if (left._increment0 != right._increment0)
            {
                return left._increment0 > right._increment0;
            }

            if (left._increment1 != right._increment1)
            {
                return left._increment1 > right._increment1;
            }

            if (left._increment2 != right._increment2)
            {
                return left._increment2 > right._increment2;
            }

            return true;
        }

        /// <summary>
        ///     Parses a string into a value.
        /// </summary>
        /// <param name="s">The string to parse.</param>
        /// <param name="provider">An object that provides culture-specific formatting information about <paramref name="s" />.</param>
        /// <exception cref="ArgumentNullException"><paramref name="s" /> is <see langword="null" />.</exception>
        /// <exception cref="FormatException"><paramref name="s" /> is not in the correct format.</exception>
        public static ObjectId Parse(string s, IFormatProvider? provider)
        {
            return Parse(s);
        }

        /// <summary>
        ///     Tries to parses a string into a value.
        /// </summary>
        /// <param name="s">The string to parse.</param>
        /// <param name="provider">An object that provides culture-specific formatting information about <paramref name="s" />.</param>
        /// <param name="result">
        ///     On return, contains the result of successfully parsing <paramref name="s" /> or an undefined value
        ///     on failure.
        /// </param>
        /// <returns><see langword="true" /> if <paramref name="s" /> was successfully parsed; otherwise, <see langword="false" />.</returns>
        public static bool TryParse(string? s, IFormatProvider? provider, out ObjectId result)
        {
            return TryParse(s, out result);
        }

        /// <summary>
        ///     Parses a span of characters into a value.
        /// </summary>
        /// <param name="s">The span of characters to parse.</param>
        /// <param name="provider">An object that provides culture-specific formatting information about <paramref name="s" />.</param>
        /// <exception cref="FormatException"><paramref name="s" /> is not in a recognized format.</exception>
        public static ObjectId Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
        {
            return Parse(s);
        }

        /// <summary>
        ///     Tries to parses a span of characters into a value.
        /// </summary>
        /// <param name="s">The span of characters to parse.</param>
        /// <param name="provider">An object that provides culture-specific formatting information about <paramref name="s" />.</param>
        /// <param name="result">
        ///     On return, contains the result of successfully parsing <paramref name="s" /> or an undefined value
        ///     on failure.
        /// </param>
        /// <returns><see langword="true" /> if <paramref name="s" /> was successfully parsed; otherwise, <see langword="false" />.</returns>
        public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out ObjectId result)
        {
            return TryParse(s, out result);
        }

        private static readonly long _random = CalculateRandomValue();
        private static readonly int _random24 = (int) (_random << 24);
        private static readonly int _random8Reverse = BinaryPrimitives.ReverseEndianness((int) (_random >> 8));
        private static int __increment;

        static ObjectId()
        {
#if NETFRAMEWORK || NETSTANDARD2_0
            // Fall back to the old Random create function
            var bytes = new byte[sizeof(int)]; // 4 bytes
            using var rng = RandomNumberGenerator.Create();
            rng.GetNonZeroBytes(bytes);
            __increment = (BitConverter.ToInt32(bytes, startIndex: 0) % 500000 + 500000) % 500000;
#else
		// Use the RandomNumberGenerator static function where available
		__increment = RandomNumberGenerator.GetInt32(500000);
#endif
        }

        private static long CalculateRandomValue()
        {
            var seed = (int) DateTime.UtcNow.Ticks ^ GetMachineHash() ^ GetPid();
            var random = new Random(seed);
            var high = random.Next();
            var low = random.Next();
            var combined = (long) (((ulong) (uint) high << 32) | (uint) low);

            // low order 5 bytes
            return combined & 0x00ffffffffff;
        }

        private static short GetPid()
        {
            try
            {
                return (short) GetCurrentProcessId(); // use low order two bytes only
            }
            catch (SecurityException)
            {
                return 0;
            }
        }

        /// <summary>
        ///     Gets the current process id.  This method exists because of how CAS operates on the call stack, checking
        ///     for permissions before executing the method.  Hence, if we inlined this call, the calling method would not execute
        ///     before throwing an exception requiring the try/catch at an even higher level that we don't necessarily control.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static int GetCurrentProcessId()
        {
#if NET5_0_OR_GREATER
        return Environment.ProcessId;
#else
            return Process.GetCurrentProcess().Id;
#endif
        }

        private static int GetMachineHash()
        {
            // use instead of Dns.HostName so it will work offline
            var machineName = Environment.MachineName;
            return 0x00ffffff & machineName.GetHashCode(); // use first 3 bytes of hash
        }

#if NET7_0_OR_GREATER
        /// <inheritdoc cref="IMinMaxValue{TSelf}.MaxValue" />
        static ObjectId IMinMaxValue<ObjectId>.MaxValue => Max;

        /// <inheritdoc cref="IMinMaxValue{TSelf}.MinValue" />
        static ObjectId IMinMaxValue<ObjectId>.MinValue => Min;

#endif

        /// <summary>
        ///     Generates a new, unique <see cref="ObjectId" /> by setting the timestamp to current UTC time (
        ///     <see cref="DateTime.UtcNow" />) and generating a random value and increment.
        /// </summary>
        /// <returns>A new unique <see cref="ObjectId" />.</returns>
        public static ObjectId NewObjectId()
        {
            ObjectId result = default;
            ref var b = ref Unsafe.As<ObjectId, byte>(ref result);

            var timestamp = (int) (uint) (long) Math.Floor(DateTime.UtcNow.Ticks / TicksPerSecond - UnixEpochSeconds);

            // increments the value atomically and then masks out the upper 8 bits of the result, keeping only the lower 3 bytes
            var increment = Interlocked.Increment(ref __increment) & 0x00ffffff;

            Unsafe.WriteUnaligned(ref b, BinaryPrimitives.ReverseEndianness(timestamp));

            Unsafe.WriteUnaligned(ref Unsafe.Add(ref b, elementOffset: 4), _random8Reverse);

            Unsafe.WriteUnaligned(ref Unsafe.Add(ref b, elementOffset: 8), BinaryPrimitives.ReverseEndianness(_random24 | increment));

            return result;
        }
    }
}
