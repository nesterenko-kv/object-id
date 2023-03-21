using System.Text;
using Sigin.ObjectId.Tests.Data;
using Sigin.ObjectId.Tests.Data.Models;

namespace Sigin.ObjectId.Tests;

public class ObjectIdTryParseUtf8Tests
{
    [Test]
    public void TryParseUtf8NullSpanShouldFalse()
    {
        var parsed = ObjectId.TryParse((ReadOnlySpan<byte>) null, out var objectId);
        Assert.Multiple(
            () =>
            {
                Assert.That(parsed, Is.False);
                Assert.That(objectId, Is.EqualTo(ObjectId.Empty));
            }
        );
    }

    [Test]
    public void TryParseUtf8EmptySpanShouldFalse()
    {
        var parsed = ObjectId.TryParse(ReadOnlySpan<byte>.Empty, out var objectId);
        Assert.Multiple(
            () =>
            {
                Assert.That(parsed, Is.False);
                Assert.That(objectId, Is.EqualTo(ObjectId.Empty));
            }
        );
    }

    #region TryParseN

    [Test]
    public void TryParseUtf8CorrectNSpan()
    {
        TryParseUtf8CorrectSpan(ObjectIdTestData.CorrectNStrings);
    }

    [Test]
    public void ParseUtf8NIncorrectLargeSpan()
    {
        TryParseUtf8IncorrectSpan(ObjectIdTestData.LargeNStrings);
    }

    [Test]
    public void ParseNIncorrectSmallSpan()
    {
        TryParseUtf8IncorrectSpan(ObjectIdTestData.SmallNStrings);
    }

    [Test]
    public void ParseIncorrectNSpan()
    {
        TryParseUtf8IncorrectSpan(ObjectIdTestData.BrokenNStrings);
    }

    #endregion

    #region Helpers

    private unsafe void TryParseUtf8CorrectSpan(ObjectIdStringWithBytes[] correctStrings)
    {
        Assert.Multiple(
            () =>
            {
                Span<byte> utf8Buffer = stackalloc byte[8192];
                foreach (var correctString in correctStrings)
                {
                    var utf8Chars = GetUtf8BytesSpanFromString(correctString.String, utf8Buffer);
                    var spanToParse = utf8Buffer[..utf8Chars];
                    var expectedBytes = correctString.Bytes;

                    var parsed = ObjectId.TryParse(spanToParse, out var objectId);

                    var actualBytes = new byte[12];
                    fixed (byte* pinnedActualBytes = actualBytes)
                    {
                        *(ObjectId*) pinnedActualBytes = objectId;
                    }

                    Assert.That(parsed, Is.True);
                    Assert.That(actualBytes, Is.EqualTo(expectedBytes));
                }
            }
        );
    }

    private void TryParseUtf8IncorrectSpan(string[] incorrectLargeStrings)
    {
        Assert.Multiple(
            () =>
            {
                Span<byte> utf8Buffer = stackalloc byte[8192];
                foreach (var largeString in incorrectLargeStrings)
                {
                    var utf8Chars = GetUtf8BytesSpanFromString(largeString, utf8Buffer);
                    var spanToParse = utf8Buffer[..utf8Chars];
                    Assert.That(ObjectId.TryParse(spanToParse, out _), Is.False);
                }
            }
        );
    }

    private static int GetUtf8BytesSpanFromString(string objectIdString, Span<byte> result)
    {
        var resultBytes = Encoding.UTF8.GetBytes(objectIdString);
        if (resultBytes.Length > result.Length)
        {
            throw new Exception("Utf8 bytes larger than provided buffer");
        }

        for (var i = 0; i < resultBytes.Length; i++)
        {
            result[i] = resultBytes[i];
        }

        return resultBytes.Length;
    }

    #endregion
}
