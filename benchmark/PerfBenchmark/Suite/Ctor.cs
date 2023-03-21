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
public class Ctor
{
    private const string _hex = "507c7f79bcf86cd7994f6c0e";
    private static readonly ObjectId _mongoId = ObjectId.Parse(_hex);
    private static readonly byte[] _mongoIdArray = _mongoId.ToByteArray();
    private static readonly Sigin.ObjectId.ObjectId _objectId = new(_hex);
    private static readonly byte[] _objectIdArray = _objectId.ToByteArray();

    [Benchmark]
    public ObjectId MongoDb_ObjectId_Ctor()
    {
        return new ObjectId(_mongoIdArray);
    }

    [Benchmark]
    public Sigin.ObjectId.ObjectId ObjectId_Ctor()
    {
        return new Sigin.ObjectId.ObjectId(_objectIdArray);
    }
}
