using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Order;
using Sigin.ObjectId;

namespace PerfBenchmark.Suite;

[GcServer(true)]
[MemoryDiagnoser]
[MinColumn]
[MaxColumn]
[RPlotExporter]
[CsvMeasurementsExporter]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[DisassemblyDiagnoser(printSource: true, syntax: DisassemblySyntax.Masm)]
public class New
{
    [Benchmark]
    public ObjectId ObjectId_New()
    {
        return ObjectId.NewObjectId();
    }

    [Benchmark]
    public MongoDB.Bson.ObjectId MongoDb_ObjectId_New()
    {
        return MongoDB.Bson.ObjectId.GenerateNewId();
    }
}
