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
public class Parse
{
    private const string _hex = "507c7f79bcf86cd7994f6c0e";
    private static readonly char[] _format = {'N'};

    [Benchmark]
    public ObjectId MongoDb_ObjectId_Parse()
    {
        return ObjectId.Parse(_hex);
    }

    [Benchmark]
    public Sigin.ObjectId.ObjectId ObjectId_Parse_N()
    {
        return Sigin.ObjectId.ObjectId.ParseExact(_hex, new ReadOnlySpan<char>(_format));
    }
}
