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
public class TryWrite
{
    private const string _hex = "507c7f79bcf86cd7994f6c0e";
    private static readonly ObjectId _mongoId = ObjectId.Parse(_hex);
    private static readonly Sigin.ObjectId.ObjectId _objectId = new(_hex);

    [Benchmark]
    public ObjectId MongoDb_ObjectId_ToByteArray()
    {
        var array = new byte[12];

        _mongoId.ToByteArray(array, offset: 0);

        return new ObjectId(array);
    }

    [Benchmark]
    public Sigin.ObjectId.ObjectId ObjectId_TryWriteBytes()
    {
        Span<byte> array = stackalloc byte[12];

        _objectId.TryWriteBytes(array);

        return new Sigin.ObjectId.ObjectId(array);
    }
}
