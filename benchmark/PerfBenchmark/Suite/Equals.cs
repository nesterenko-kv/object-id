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
public class Equals
{
    private const string _hex = "507c7f79bcf86cd7994f6c0e";
    private static readonly ObjectId _mongoId = ObjectId.Parse(_hex);
    private static readonly ObjectId _mongoId2 = new(_mongoId.ToByteArray());
    private static readonly Sigin.ObjectId.ObjectId _objectId = new(_hex);
    private static readonly Sigin.ObjectId.ObjectId _objectId2 = new(_objectId.ToByteArray());

    [Benchmark]
    public bool MongoDb_Id_Equals_op()
    {
        return _mongoId == _mongoId2;
    }

    [Benchmark]
    public bool MongoDb_Id_Equals_opne()
    {
        return _mongoId != _mongoId2;
    }

    [Benchmark]
    public bool MongoDb_Id_Equals()
    {
        return _mongoId.Equals(_mongoId2);
    }

    [Benchmark]
    public bool ObjectId_Equals_op()
    {
        return _objectId == _objectId2;
    }

    [Benchmark]
    public bool ObjectId_Equals_opne()
    {
        return _objectId != _objectId2;
    }

    [Benchmark]
    public bool ObjectId_Equals()
    {
        return _objectId.Equals(_objectId2);
    }
}
