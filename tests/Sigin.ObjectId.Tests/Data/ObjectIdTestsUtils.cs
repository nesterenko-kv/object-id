using System.Runtime.InteropServices;
using Sigin.ObjectId.Tests.Data.Models;

namespace Sigin.ObjectId.Tests.Data;

public class ObjectIdTestsUtils
{
    public static byte[] ConvertHexStringToByteArray(string hexString)
    {
        if (hexString.Length % 2 != 0)
        {
            throw new ArgumentException($"The binary key cannot have an odd number of digits: {hexString}", nameof(hexString));
        }

        var data = new byte[hexString.Length / 2];
        for (var index = 0; index < data.Length; index++)
        {
            var byteValue = hexString.Substring(index * 2, length: 2);
            data[index] = Convert.ToByte(byteValue, fromBase: 16);
        }

        return data;
    }

    public static string GetStringN(byte[] bytes)
    {
        // dddddddddddddddddddddddd
        if (bytes.Length != 12)
        {
            throw new ArgumentException("ObjectId bytes count should be 12", nameof(bytes));
        }

        return BitConverter
            .ToString(bytes)
            .Replace("-", string.Empty)
            .ToLowerInvariant();
    }

    public static ObjectIdStringWithBytes[] GenerateNStrings()
    {
        var resultStrings = new List<string>();
        for (int stringsToCreate = 24, itemsToFill = 1; stringsToCreate > 0; stringsToCreate >>= 1, itemsToFill <<= 1)
        for (var stringIndex = 0; stringIndex < stringsToCreate; stringIndex++)
        {
            resultStrings.Add(
                string.Create(
                    length: 24,
                    (stringIndex * itemsToFill, itmesToFill: itemsToFill),
                    (result, state) =>
                    {
                        var (startPositionToFill, itemsToFillCount) = state;
                        for (var j = 0; j < 24; j++)
                        {
                            result[j] = '0';
                        }

                        result[startPositionToFill] = '1';
                        for (var j = 0; j < itemsToFillCount; j++)
                        {
                            result[startPositionToFill + j] = '1';
                        }
                    }
                )
            );
        }

        for (int stringsToCreate = 24, itemsToFill = 1; stringsToCreate > 0; stringsToCreate >>= 1, itemsToFill <<= 1)
        for (var stringIndex = 0; stringIndex < stringsToCreate; stringIndex++)
        {
            resultStrings.Add(
                string.Create(
                    length: 24,
                    (stringIndex * itemsToFill, itmesToFill: itemsToFill),
                    (result, state) =>
                    {
                        var (startPositionToFill, itemsToFillCount) = state;
                        for (var j = 0; j < 24; j++)
                        {
                            result[j] = '1';
                        }

                        result[startPositionToFill] = '1';
                        for (var j = 0; j < itemsToFillCount; j++)
                        {
                            result[startPositionToFill + j] = '0';
                        }
                    }
                )
            );
        }

        var nStrings = resultStrings.Distinct().ToArray();
        var output = new ObjectIdStringWithBytes[nStrings.Length];
        for (var i = 0; i < nStrings.Length; i++)
        {
            var bytes = ConvertHexStringToByteArray(nStrings[i]);
            output[i] = new ObjectIdStringWithBytes(nStrings[i], bytes);
        }

        return output;
    }

    public static string[] GenerateBrokenNStringsArray()
    {
        return GenerateBrokenStringsArray(outputFormatSize: 24, GetStringN);
    }

    private static unsafe string[] GenerateBrokenStringsArray(
        int outputFormatSize,
        Func<byte[], string> formatString
        )
    {
        var count = outputFormatSize * 2;
        var rng = new ObjectIdRng(1337);
        var objectIdIntegers = stackalloc int[3];
        var charToBreakPtr = stackalloc char[1];
        var charBytesPtr = (byte*) charToBreakPtr;
        var result = new string[count];
        var breakUpperByteOnCharArray = new bool[outputFormatSize];
        for (var i = 0; i < breakUpperByteOnCharArray.Length; i++)
        {
            breakUpperByteOnCharArray[i] = false;
        }

        for (var i = 0; i < count; i++)
        {
            for (var j = 0; j < 3; j++)
            {
                objectIdIntegers[j] = rng.Next();
            }

            var bytesOfObjectId = new ReadOnlySpan<byte>(objectIdIntegers, length: 12).ToArray();
            var objectIdString = formatString(bytesOfObjectId);
            var spanOfString = MemoryMarshal.CreateSpan(
                ref MemoryMarshal.GetReference(objectIdString.AsSpan()),
                objectIdString.Length
            );
            var brokenCharIndex = i % outputFormatSize;
            var shouldBreakUpperByte = breakUpperByteOnCharArray[brokenCharIndex];
            breakUpperByteOnCharArray[brokenCharIndex] = !shouldBreakUpperByte;
            charToBreakPtr[0] = objectIdString[brokenCharIndex];
            if (shouldBreakUpperByte)
            {
                charBytesPtr[0] = 110;
            }
            else
            {
                charBytesPtr[1] = 110;
            }

            spanOfString[brokenCharIndex] = charToBreakPtr[0];
            result[i] = objectIdString;
        }

        return result;
    }
}
