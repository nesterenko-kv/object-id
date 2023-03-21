using Sigin.ObjectId.Tests.Data;

namespace Sigin.ObjectId.Tests;

public unsafe class ObjectIdTryFormatTests
{
    #region TryFormat

    [TestCaseSource(typeof(ObjectIdTestData), nameof(ObjectIdTestData.CorrectObjectIdBytesArrays))]
    public void TryFormatNullFormat(byte[] correctBytes)
    {
        Assert.Multiple(
            () =>
            {
                var objectId = new ObjectId(correctBytes);
                var expectedString = ObjectIdTestsUtils.GetStringN(correctBytes);
                var bufferPtr = stackalloc char[24];
                var spanBuffer = new Span<char>(bufferPtr, length: 24);
                Assert.That(objectId.TryFormat(spanBuffer, out var charsWritten), Is.True);
                Assert.That(charsWritten, Is.EqualTo(24));
                Assert.That(new string(bufferPtr, startIndex: 0, length: 24), Is.EqualTo(expectedString));
            }
        );
    }

    [TestCaseSource(typeof(ObjectIdTestData), nameof(ObjectIdTestData.CorrectObjectIdBytesArrays))]
    public void TryFormatEmptyFormat(byte[] correctBytes)
    {
        Assert.Multiple(
            () =>
            {
                var objectId = new ObjectId(correctBytes);
                var expectedString = ObjectIdTestsUtils.GetStringN(correctBytes);
                var bufferPtr = stackalloc char[24];
                var spanBuffer = new Span<char>(bufferPtr, length: 24);
                Assert.That(objectId.TryFormat(spanBuffer, out var charsWritten, ReadOnlySpan<char>.Empty), Is.True);
                Assert.That(charsWritten, Is.EqualTo(24));
                Assert.That(new string(bufferPtr, startIndex: 0, length: 24), Is.EqualTo(expectedString));
            }
        );
    }

    [TestCaseSource(typeof(ObjectIdTestData), nameof(ObjectIdTestData.CorrectObjectIdBytesArrays))]
    public void TryFormatIncorrectFormat(byte[] correctBytes)
    {
        Assert.Multiple(
            () =>
            {
                var objectId = new ObjectId(correctBytes);
                Span<char> buffer = stackalloc char[68];
                Assert.That(objectId.TryFormat(buffer, out var charsWritten, "И".AsSpan()), Is.False);
                Assert.That(charsWritten, Is.EqualTo(0));
            }
        );
    }

    [TestCaseSource(typeof(ObjectIdTestData), nameof(ObjectIdTestData.CorrectObjectIdBytesArrays))]
    public void TryFormatTooLongFormat(byte[] correctBytes)
    {
        Assert.Multiple(
            () =>
            {
                var objectId = new ObjectId(correctBytes);
                Span<char> buffer = stackalloc char[68];
                Assert.That(objectId.TryFormat(buffer, out var charsWritten, "ИИ".AsSpan()), Is.False);
                Assert.That(charsWritten, Is.EqualTo(0));
            }
        );
    }

    [TestCaseSource(typeof(ObjectIdTestData), nameof(ObjectIdTestData.CorrectObjectIdBytesArrays))]
    public void TryFormatNCorrect(byte[] correctBytes)
    {
        Assert.Multiple(
            () =>
            {
                var objectId = new ObjectId(correctBytes);
                var expectedString = ObjectIdTestsUtils.GetStringN(correctBytes);
                var bufferPtr = stackalloc char[24];
                var spanBuffer = new Span<char>(bufferPtr, length: 24);
                Assert.That(
                    objectId.TryFormat(spanBuffer, out var charsWritten, new ReadOnlySpan<char>(new[] {'N'})),
                    Is.True
                );
                Assert.That(charsWritten, Is.EqualTo(24));
                Assert.That(new string(bufferPtr, startIndex: 0, length: 24), Is.EqualTo(expectedString));
            }
        );
    }

    [TestCaseSource(typeof(ObjectIdTestData), nameof(ObjectIdTestData.CorrectObjectIdBytesArrays))]
    public void TryFormatSmallDestination(byte[] correctBytes)
    {
        Assert.Multiple(
            () =>
            {
                var objectId = new ObjectId(correctBytes);
                Span<char> buffer = stackalloc char[10];
                var formats = new[]
                {
                    'N',
                    'n'
                };

                foreach (var format in formats)
                {
                    Assert.That(
                        objectId.TryFormat(buffer, out var charsWritten, new ReadOnlySpan<char>(new[] {format})),
                        Is.False
                    );
                    Assert.That(charsWritten, Is.EqualTo(0));
                }
            }
        );
    }

    #endregion

#if NET6_0_OR_GREATER
    #region ISpanFormattable.TryFormat

    [TestCaseSource(typeof(ObjectIdTestData), nameof(ObjectIdTestData.CorrectObjectIdBytesArrays))]
    public void SpanFormattableTryFormatEmptyFormat(byte[] correctBytes)
    {
        Assert.Multiple(() =>
        {
            ISpanFormattable objectId = new ObjectId(correctBytes);
            var expectedString = ObjectIdTestsUtils.GetStringN(correctBytes);
            var bufferPtr = stackalloc char[24];
            var spanBuffer = new Span<char>(bufferPtr, 24);
            Assert.That(objectId.TryFormat(spanBuffer, out var charsWritten, ReadOnlySpan<char>.Empty, null), Is.True);
            Assert.That(charsWritten, Is.EqualTo(24));
            Assert.That(new string(bufferPtr, 0, 24), Is.EqualTo(expectedString));
        });
    }

    [TestCaseSource(typeof(ObjectIdTestData), nameof(ObjectIdTestData.CorrectObjectIdBytesArrays))]
    public void SpanFormattableTryFormatIncorrectFormat(byte[] correctBytes)
    {
        Assert.Multiple(() =>
        {
            ISpanFormattable objectId = new ObjectId(correctBytes);
            Span<char> buffer = stackalloc char[68];
            Assert.That(objectId.TryFormat(buffer, out var charsWritten, "И".AsSpan(), null), Is.False);
            Assert.That(charsWritten, Is.EqualTo(0));
        });
    }

    [TestCaseSource(typeof(ObjectIdTestData), nameof(ObjectIdTestData.CorrectObjectIdBytesArrays))]
    public void SpanFormattableTryFormatTooLongFormat(byte[] correctBytes)
    {
        Assert.Multiple(() =>
        {
            ISpanFormattable objectId = new ObjectId(correctBytes);
            Span<char> buffer = stackalloc char[68];
            Assert.That(objectId.TryFormat(buffer, out var charsWritten, "ИИ".AsSpan(), null), Is.False);
            Assert.That(charsWritten, Is.EqualTo(0));
        });
    }

    [TestCaseSource(typeof(ObjectIdTestData), nameof(ObjectIdTestData.CorrectObjectIdBytesArrays))]
    public void SpanFormattableTryFormatNCorrect(byte[] correctBytes)
    {
        Assert.Multiple(() =>
        {
            ISpanFormattable objectId = new ObjectId(correctBytes);
            var expectedString = ObjectIdTestsUtils.GetStringN(correctBytes);
            var bufferPtr = stackalloc char[24];
            var spanBuffer = new Span<char>(bufferPtr, 24);
            Assert.That(objectId.TryFormat(spanBuffer, out var charsWritten, new ReadOnlySpan<char>(new[] { 'N' }), null), Is.True);
            Assert.That(charsWritten, Is.EqualTo(24));
            Assert.That(new string(bufferPtr, 0, 24), Is.EqualTo(expectedString));
        });
    }

    [TestCaseSource(typeof(ObjectIdTestData), nameof(ObjectIdTestData.CorrectObjectIdBytesArrays))]
    public void SpanFormattableTryFormatSmallDestination(byte[] correctBytes)
    {
        Assert.Multiple(() =>
        {
            ISpanFormattable objectId = new ObjectId(correctBytes);
            Span<char> buffer = stackalloc char[10];
            var formats = new[]
            {
                'N',
                'n'
            };
            foreach (var format in formats)
            {
                Assert.That(objectId.TryFormat(buffer, out var charsWritten, new ReadOnlySpan<char>(new[] { format }), null), Is.False);
                Assert.That(charsWritten, Is.EqualTo(0));
            }
        });
    }
    #endregion

#endif
}
