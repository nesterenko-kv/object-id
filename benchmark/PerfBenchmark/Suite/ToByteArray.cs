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
public class ToByteArray
{
    private const string _hex = "507c7f79bcf86cd7994f6c0e";
    private static readonly ObjectId _mongoId = ObjectId.Parse(_hex);
    private static readonly Sigin.ObjectId.ObjectId _objectId = new(_hex);

    [Benchmark]
    public byte[] MongoDb_ObjectId_ToByteArray()
    {
        return _mongoId.ToByteArray();
    }

    [Benchmark]
    public byte[] ObjectId_ToByteArray()
    {
        return _objectId.ToByteArray();
    }
}
