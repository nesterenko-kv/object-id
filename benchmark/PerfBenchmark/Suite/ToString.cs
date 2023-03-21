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
public class ToString
{
    private const string _hex = "507c7f79bcf86cd7994f6c0e";
    private static readonly ObjectId _mongoId = ObjectId.Parse(_hex);
    private static readonly Sigin.ObjectId.ObjectId _objectId = new(_hex);

    [Benchmark]
    public string MongoDb_ObjectId_ToString()
    {
        return _mongoId.ToString();
    }

    [Benchmark]
    public string ObjectId_ToStringN()
    {
        return _objectId.ToString("N");
    }
}
