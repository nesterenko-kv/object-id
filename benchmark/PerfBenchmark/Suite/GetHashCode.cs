using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Order;
using MongoDB.Bson;

namespace PerfBenchmark.Suite;

[GcServer(true)]
[MemoryDiagnoser]
[MinColumn]
[MaxColumn]
[RPlotExporter]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[DisassemblyDiagnoser(printSource: true, syntax: DisassemblySyntax.Masm)]
public class GetHashCode
{
    private const string _hex = "507c7f79bcf86cd7994f6c0e";
    private static readonly ObjectId _mongoId = ObjectId.Parse(_hex);
    private static readonly Sigin.ObjectId.ObjectId _objectId = new(_hex);

    [Benchmark]
    public int ObjectId_GetHashCode()
    {
        return _objectId.GetHashCode();
    }

    [Benchmark]
    public int MongoDb_ObjectId_GetHashCode()
    {
        return _mongoId.GetHashCode();
    }
}
