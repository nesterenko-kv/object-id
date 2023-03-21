using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Sigin.ObjectId;

namespace PerfBenchmark.Benchmarks;

[GcServer(true)]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Naming is fine")]
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public unsafe class TryParseBenchmarks
{
    private static readonly string[] _sometimesBrokenRandomObjectIdsN_1_000_000;

    static TryParseBenchmarks()
    {
        _sometimesBrokenRandomObjectIdsN_1_000_000 = GenerateSometimesBrokenGuidsNStringsArray(1_000_000);
    }

    public static IEnumerable<object> ArgsN()
    {
        yield return _sometimesBrokenRandomObjectIdsN_1_000_000;
    }

    // N
    [Benchmark(OperationsPerInvoke = 1_000_000)]
    [BenchmarkCategory("TryParseN")]
    [ArgumentsSource(nameof(ArgsN))]
    public void objectId_TryParse_N(string[] possibleBrokenStrings)
    {
        foreach (var possibleBrokenString in possibleBrokenStrings)
        {
            var _ = ObjectId.TryParse(possibleBrokenString, out var _);
        }
    }

    private static string[] GenerateSometimesBrokenGuidsNStringsArray(int count)
    {
        var random = new Random();
        var objectIdIntegers = stackalloc int[3];
        var charToBreakPtr = stackalloc char[1];
        var charBytesPtr = (byte*) charToBreakPtr;
        var result = new string[count];
        var breakUpperByteOnCharArray = new bool[24];
        for (var i = 0; i < breakUpperByteOnCharArray.Length; i++)
        {
            breakUpperByteOnCharArray[i] = false;
        }

        for (var i = 0; i < count; i++)
        {
            for (var j = 0; j < 3; j++)
            {
                objectIdIntegers[j] = random.Next();
            }

            var bytesOfObjectId = new ReadOnlySpan<byte>(objectIdIntegers, length: 12).ToArray();
            var nString = GetStringN(bytesOfObjectId);
            var spanOfString = MemoryMarshal.CreateSpan(
                ref MemoryMarshal.GetReference(nString.AsSpan()),
                nString.Length
            );

            var brokenCharIndex = i % 24;
            if (brokenCharIndex != 0)
            {
                var shouldBreakUpperByte = breakUpperByteOnCharArray[brokenCharIndex];
                breakUpperByteOnCharArray[brokenCharIndex] = !shouldBreakUpperByte;
                charToBreakPtr[0] = nString[brokenCharIndex];
                if (shouldBreakUpperByte)
                {
                    charBytesPtr[0] = 110;
                }
                else
                {
                    charBytesPtr[1] = 110;
                }

                spanOfString[brokenCharIndex] = charToBreakPtr[0];
            }

            result[i] = nString;
        }

        return result;
    }

    private static string GetStringN(byte[] bytesOfObjectId)
    {
        if (bytesOfObjectId.Length != 12)
        {
            throw new ArgumentException("Array should contain 12 bytes", nameof(bytesOfObjectId));
        }

        return BitConverter
            .ToString(bytesOfObjectId)
            .Replace("-", string.Empty)
            .ToLowerInvariant();
    }
}
