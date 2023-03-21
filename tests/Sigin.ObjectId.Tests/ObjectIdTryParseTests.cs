using System.Diagnostics.CodeAnalysis;
using Sigin.ObjectId.Tests.Data;
using Sigin.ObjectId.Tests.Data.Models;

namespace Sigin.ObjectId.Tests;

[SuppressMessage("Performance", "CA1822:Mark members as static")]
public class ObjectIdTryParseTests
{
    [Test]
    public void TryParseNullStringShouldFalse()
    {
        var parsed = ObjectId.TryParse((string?) null, out var objectId);
        Assert.Multiple(
            () =>
            {
                Assert.That(parsed, Is.False);
                Assert.That(objectId, Is.EqualTo(ObjectId.Empty));
            }
        );
    }

    [Test]
    public void TryParseEmptyStringShouldFalse()
    {
        var parsed = ObjectId.TryParse(string.Empty, out var objectId);
        Assert.Multiple(
            () =>
            {
                Assert.That(parsed, Is.False);
                Assert.That(objectId, Is.EqualTo(ObjectId.Empty));
            }
        );
    }

    [Test]
    public void TryParseEmptySpanShouldFalse()
    {
        var parsed = ObjectId.TryParse(new ReadOnlySpan<char>(Array.Empty<char>()), out var objectId);
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
    public void TryParseCorrectNString()
    {
        TryParseCorrectString(ObjectIdTestData.CorrectNStrings);
    }

    [Test]
    public void TryParseCorrectNSpan()
    {
        TryParseCorrectSpan(ObjectIdTestData.CorrectNStrings);
    }

    [Test]
    public void TryParseNIncorrectLargeString()
    {
        TryParseIncorrectString(ObjectIdTestData.LargeNStrings);
    }

    [Test]
    public void ParseNIncorrectLargeSpan()
    {
        TryParseIncorrectSpan(ObjectIdTestData.LargeNStrings);
    }

    [Test]
    public void ParseNIncorrectSmallString()
    {
        TryParseIncorrectString(ObjectIdTestData.SmallNStrings);
    }

    [Test]
    public void ParseNIncorrectSmallSpan()
    {
        TryParseIncorrectSpan(ObjectIdTestData.SmallNStrings);
    }

    [Test]
    public void ParseIncorrectNString()
    {
        TryParseIncorrectString(ObjectIdTestData.BrokenNStrings);
    }

    [Test]
    public void ParseIncorrectNSpan()
    {
        TryParseIncorrectSpan(ObjectIdTestData.BrokenNStrings);
    }

    #endregion

    #region Helpers

    private unsafe void TryParseCorrectString(ObjectIdStringWithBytes[] correctStrings)
    {
        Assert.Multiple(
            () =>
            {
                foreach (var correctString in correctStrings)
                {
                    var stringToParse = correctString.String;
                    var expectedBytes = correctString.Bytes;

                    var parsed = ObjectId.TryParse(stringToParse, out var objectId);

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

    private unsafe void TryParseCorrectSpan(ObjectIdStringWithBytes[] correctStrings)
    {
        Assert.Multiple(
            () =>
            {
                foreach (var correctString in correctStrings)
                {
                    var spanToParse = new ReadOnlySpan<char>(correctString.String.ToCharArray());
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

    private void TryParseIncorrectString(string[] incorrectLargeStrings)
    {
        Assert.Multiple(
            () =>
            {
                foreach (var largeString in incorrectLargeStrings)
                {
                    Assert.That(ObjectId.TryParse(largeString, out _), Is.False);
                }
            }
        );
    }

    private void TryParseIncorrectSpan(string[] incorrectLargeStrings)
    {
        Assert.Multiple(
            () =>
            {
                foreach (var largeString in incorrectLargeStrings)
                {
                    var largeSpan = new ReadOnlySpan<char>(largeString.ToCharArray());
                    Assert.That(ObjectId.TryParse(largeSpan, out _), Is.False);
                }
            }
        );
    }

    #endregion
}
