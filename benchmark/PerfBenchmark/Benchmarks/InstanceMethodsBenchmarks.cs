using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Order;
using Sigin.ObjectId;

namespace PerfBenchmark.Benchmarks;

[InvocationCount(1024 * 1024 * 1024, unrollFactor: 32)]
[GcServer(true)]
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
[MinColumn]
[MaxColumn]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[DisassemblyDiagnoser(printSource: true, syntax: DisassemblySyntax.Masm)]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Naming is fine")]
[SuppressMessage("Performance", "CA1822:Mark members as static")]
public class InstanceMethodsBenchmarks
{
    private static readonly ObjectId _objectId;

    static InstanceMethodsBenchmarks()
    {
        _objectId = new ObjectId("507c7f79bcf86cd7994f6c0e");
    }

    public IEnumerable<ObjectId> ObjectIdArgs()
    {
        yield return _objectId;
    }

    // Increment
    [Benchmark]
    [BenchmarkCategory("Increment")]
    [ArgumentsSource(nameof(ObjectIdArgs))]
    public int objectId_Increment(ObjectId objectId)
    {
        return objectId.Increment;
    }

    //[Benchmark]
    //[BenchmarkCategory("Increment")]
    //[ArgumentsSource(nameof(ObjectIdArgs))]
    //public int objectId_Increment2(ObjectId objectId)
    //{
    //    return objectId.Increment2;
    //}

    // Timestamp
    [Benchmark]
    [BenchmarkCategory("Timestamp")]
    [ArgumentsSource(nameof(ObjectIdArgs))]
    public long objectId_Timestamp(ObjectId objectId)
    {
        return objectId.Timestamp;
    }

    //[Benchmark]
    //[BenchmarkCategory("Timestamp")]
    //[ArgumentsSource(nameof(ObjectIdArgs))]
    //public long objectId_Timestamp2(ObjectId objectId)
    //{
    //    return objectId.Timestamp2;
    //}

    // Random
    [Benchmark]
    [BenchmarkCategory("Random")]
    [ArgumentsSource(nameof(ObjectIdArgs))]
    public long objectId_Random(ObjectId objectId)
    {
        return objectId.Random;
    }

    //[Benchmark]
    //[BenchmarkCategory("Random")]
    //[ArgumentsSource(nameof(ObjectIdArgs))]
    //public long objectId_Random2(ObjectId objectId)
    //{
    //    return objectId.Random2;
    //}

    // ToByteArray
    [Benchmark]
    [BenchmarkCategory("ToByteArray")]
    [ArgumentsSource(nameof(ObjectIdArgs))]
    public byte[] objectId_ToByteArray(ObjectId objectId)
    {
        return objectId.ToByteArray();
    }

    // TryWriteBytes
    [Benchmark(OperationsPerInvoke = 16)]
    [BenchmarkCategory("TryWriteBytes")]
    [ArgumentsSource(nameof(ObjectIdArgs))]
    public void objectId_TryWriteBytes(ObjectId objectId)
    {
        Span<byte> buffer = stackalloc byte[12];
        objectId.TryWriteBytes(buffer);
        objectId.TryWriteBytes(buffer);
        objectId.TryWriteBytes(buffer);
        objectId.TryWriteBytes(buffer);

        objectId.TryWriteBytes(buffer);
        objectId.TryWriteBytes(buffer);
        objectId.TryWriteBytes(buffer);
        objectId.TryWriteBytes(buffer);

        objectId.TryWriteBytes(buffer);
        objectId.TryWriteBytes(buffer);
        objectId.TryWriteBytes(buffer);
        objectId.TryWriteBytes(buffer);

        objectId.TryWriteBytes(buffer);
        objectId.TryWriteBytes(buffer);
        objectId.TryWriteBytes(buffer);
        objectId.TryWriteBytes(buffer);
    }
}
