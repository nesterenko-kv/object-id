using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Sigin.ObjectId;

namespace PerfBenchmark.Benchmarks;

[GcServer(true)]
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Naming is fine")]
[SuppressMessage("Performance", "CA1822:Mark members as static")]
public class CtorBenchmarks
{
    [Benchmark]
    [BenchmarkCategory("byte[]")]
    [Arguments(new byte[] {170, 238, 47, 83, 214, 78, 140, 107, 139, 94, 5, 145})]
    public ObjectId objectId_CtorByteArray(byte[] objectIdBytes)
    {
        return new ObjectId(objectIdBytes);
    }

    [Benchmark(OperationsPerInvoke = 16)]
    [BenchmarkCategory("ReadOnlySpan<byte>")]
    [Arguments(new byte[] {170, 238, 47, 83, 214, 78, 140, 107, 139, 94, 5, 145})]
    [SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
    [SuppressMessage("Performance", "CA1806:Do not ignore method results", Justification = "<Pending>")]
    public void objectId_CtorReadOnlySpan(byte[] objectIdBytes)
    {
        var span = new ReadOnlySpan<byte>(objectIdBytes);
        new ObjectId(span);
        new ObjectId(span);
        new ObjectId(span);
        new ObjectId(span);

        new ObjectId(span);
        new ObjectId(span);
        new ObjectId(span);
        new ObjectId(span);

        new ObjectId(span);
        new ObjectId(span);
        new ObjectId(span);
        new ObjectId(span);

        new ObjectId(span);
        new ObjectId(span);
        new ObjectId(span);
        new ObjectId(span);
    }
}
