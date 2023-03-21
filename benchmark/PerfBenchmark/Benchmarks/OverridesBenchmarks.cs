using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Sigin.ObjectId;

namespace PerfBenchmark.Benchmarks;

[GcServer(true)]
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Naming is fine")]
[SuppressMessage("Performance", "CA1822:Mark members as static")]
public class OverridesBenchmarks
{
    private static readonly ObjectId _objectId;
    private static readonly ObjectId _objectIdSame;

    static OverridesBenchmarks()
    {
        _objectId = new ObjectId("507c7f79bcf86cd7994f6c0e");
        _objectIdSame = new ObjectId("507c7f79bcf86cd7994f6c0e");
    }

    public IEnumerable<ObjectId> ObjectIdArgs()
    {
        yield return _objectId;
    }

    public IEnumerable<object[]> ObjectIdSameValues()
    {
        yield return new object[] {_objectId, _objectIdSame};
    }

    public IEnumerable<object[]> ObjectIdDifferentTypesValues()
    {
        yield return new[] {_objectId, new object()};
    }

    // GetHashCode
    [Benchmark]
    [BenchmarkCategory("GetHashCode")]
    [ArgumentsSource(nameof(ObjectIdArgs))]
    public int objectId_GetHashCode(ObjectId objectId)
    {
        return objectId.GetHashCode();
    }

    // Equals with same value object
    [Benchmark]
    [BenchmarkCategory("Equals_same_value_object")]
    [ArgumentsSource(nameof(ObjectIdSameValues))]
    public bool objectId_EqualsWithSameValueObject(ObjectId objectId, object sameValue)
    {
        return objectId.Equals(sameValue);
    }

    // Equals with other type
    [Benchmark]
    [BenchmarkCategory("Equals_different_types_values")]
    [ArgumentsSource(nameof(ObjectIdDifferentTypesValues))]
    public bool objectId_EqualsDifferentTypesValues(ObjectId objectId, object differentTypeValue)
    {
        return objectId.Equals(differentTypeValue);
    }

    // Equals with null
    [Benchmark]
    [BenchmarkCategory("Equals_null")]
    [ArgumentsSource(nameof(ObjectIdArgs))]
    [SuppressMessage("ReSharper", "RedundantCast")]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public bool objectId_EqualsWithNull(ObjectId objectId)
    {
        return objectId.Equals((object?) null);
    }
}
