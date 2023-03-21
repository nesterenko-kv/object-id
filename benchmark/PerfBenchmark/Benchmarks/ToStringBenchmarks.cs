using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Order;
using Sigin.ObjectId;

namespace PerfBenchmark.Benchmarks;

[GcServer(true)]
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
[MinColumn]
[MaxColumn]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[DisassemblyDiagnoser(printSource: true, syntax: DisassemblySyntax.Masm)]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Naming is fine")]
[SuppressMessage("Performance", "CA1822:Mark members as static")]
public class ToStringBenchmarks
{
    private static readonly ObjectId _objectId;

    static ToStringBenchmarks()
    {
        _objectId = new ObjectId("507c7f79bcf86cd7994f6c0e");
    }

    public IEnumerable<ObjectId> ObjectIdArgs()
    {
        yield return _objectId;
    }

    // ToString("N");
    [Benchmark]
    [BenchmarkCategory("ToString_N")]
    [ArgumentsSource(nameof(ObjectIdArgs))]
    public string objectId_ToString_N(ObjectId objectId)
    {
        return objectId.ToString("N", formatProvider: null);
    }
}
