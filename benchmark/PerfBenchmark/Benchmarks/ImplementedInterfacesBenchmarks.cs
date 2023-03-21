using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Sigin.ObjectId;

namespace PerfBenchmark.Benchmarks;

[GcServer(true)]
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("Performance", "CA1822:Mark members as static")]
public class ImplementedInterfacesBenchmarks
{
    private static readonly ObjectId _objectId;
    private static readonly ObjectId _objectIdSame;
    private static readonly ObjectId _objectIdDifferent;

    static ImplementedInterfacesBenchmarks()
    {
        var objectIdString = "507c7f79bcf86cd7994f6c0e";
        _objectId = new ObjectId(objectIdString);
        _objectIdSame = new ObjectId(objectIdString);

        var differentObjectIdString = "e0c6f4997dc68fcb97f7c705";
        _objectIdDifferent = new ObjectId(differentObjectIdString);
    }

    public IEnumerable<ObjectId> ObjectIdArgs()
    {
        yield return _objectId;
    }

    public IEnumerable<object[]> ObjectIdSameValues()
    {
        yield return new object[] {_objectId, _objectIdSame};
    }

    public IEnumerable<object[]> ObjectIdDifferentValues()
    {
        yield return new object[] {_objectId, _objectIdDifferent};
    }

    // IEquatable<T>.Equals with same value
    [Benchmark]
    [BenchmarkCategory("IEquatable_T_Equals_same")]
    [ArgumentsSource(nameof(ObjectIdSameValues))]
    public bool objectId_EqualsSame(ObjectId objectId, ObjectId sameValue)
    {
        return objectId.Equals(sameValue);
    }

    // IEquatable<T>.Equals with different value
    [Benchmark]
    [BenchmarkCategory("IEquatable_T_Equals_different")]
    [ArgumentsSource(nameof(ObjectIdDifferentValues))]
    public bool objectId_EqualsDifferent(ObjectId objectId, ObjectId differentValue)
    {
        return objectId.Equals(differentValue);
    }

    // IComparable.CompareTo with same value
    [Benchmark]
    [BenchmarkCategory("IComparable_CompareTo_same")]
    [ArgumentsSource(nameof(ObjectIdSameValues))]
    public int objectId_CompareToSameValueObject(ObjectId objectId, object sameValue)
    {
        return objectId.CompareTo(sameValue);
    }

    // IComparable.CompareTo with different value
    [Benchmark]
    [BenchmarkCategory("IComparable_CompareTo_different")]
    [ArgumentsSource(nameof(ObjectIdDifferentValues))]
    public int objectId_CompareToDifferentValueObject(ObjectId objectId, object differentValue)
    {
        return objectId.CompareTo(differentValue);
    }

    // IComparable<T>.CompareTo with same value
    [Benchmark]
    [BenchmarkCategory("IComparable_T_CompareTo_same")]
    [ArgumentsSource(nameof(ObjectIdSameValues))]
    public int objectId_CompareTo_T_SameValue(ObjectId objectId, ObjectId sameValue)
    {
        return objectId.CompareTo(sameValue);
    }

    // IComparable<T>.CompareTo with different value
    [Benchmark]
    [BenchmarkCategory("IComparable _T_CompareTo_different")]
    [ArgumentsSource(nameof(ObjectIdDifferentValues))]
    public int objectId_CompareTo_T_DifferentValue(ObjectId objectId, ObjectId differentValue)
    {
        return objectId.CompareTo(differentValue);
    }

#nullable disable
    // IComparable.CompareTo with null
    [Benchmark]
    [BenchmarkCategory("IComparable_CompareTo_null")]
    [ArgumentsSource(nameof(ObjectIdArgs))]
    [SuppressMessage("ReSharper", "RedundantCast")]
    public int objectId_CompareToNull(ObjectId objectId)
    {
        return objectId.CompareTo((object) null);
    }
#nullable restore
}
