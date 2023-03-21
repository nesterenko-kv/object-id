using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Sigin.ObjectId.Tests.Data;
using Sigin.ObjectId.Tests.Data.Models;

namespace Sigin.ObjectId.Tests;

[SuppressMessage("Performance", "CA1822:Mark members as static")]
public class ObjectIdTryParseWithFormatProviderTests
{
    private static IEnumerable GetFormatProviders()
    {
        foreach (var nullableFormatProvider in GetNullableFormatProviders())
        {
            yield return nullableFormatProvider;
        }
    }

    [SuppressMessage("ReSharper", "RedundantCast")]
    private static IEnumerable<IFormatProvider?> GetNullableFormatProviders()
    {
        yield return (IFormatProvider?) CultureInfo.InvariantCulture;
        yield return (IFormatProvider?) new CultureInfo("en-US");
        yield return (IFormatProvider?) null!;
    }

    [Test]
    public void TryParseNullStringShouldFalse([ValueSource(nameof(GetFormatProviders))] IFormatProvider formatProvider)
    {
        string? valueToParse = null;
        var parsed = ObjectId.TryParse(valueToParse, formatProvider, out var objectId);
        Assert.Multiple(
            () =>
            {
                Assert.That(parsed, Is.False);
                Assert.That(objectId, Is.EqualTo(ObjectId.Empty));
            }
        );
    }

    [Test]
    public void TryParseEmptyStringShouldFalse([ValueSource(nameof(GetFormatProviders))] IFormatProvider formatProvider)
    {
        var parsed = ObjectId.TryParse(string.Empty, formatProvider, out var objectId);
        Assert.Multiple(
            () =>
            {
                Assert.That(parsed, Is.False);
                Assert.That(objectId, Is.EqualTo(ObjectId.Empty));
            }
        );
    }

    [Test]
    public void TryParseEmptySpanShouldFalse([ValueSource(nameof(GetFormatProviders))] IFormatProvider formatProvider)
    {
        var parsed = ObjectId.TryParse(new ReadOnlySpan<char>(Array.Empty<char>()), formatProvider, out var objectId);
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
                foreach (var formatProvider in GetNullableFormatProviders())
                foreach (var correctString in correctStrings)
                {
                    var stringToParse = correctString.String;
                    var expectedBytes = correctString.Bytes;

                    var parsed = ObjectId.TryParse(stringToParse, formatProvider, out var objectId);

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
                foreach (var formatProvider in GetNullableFormatProviders())
                foreach (var correctString in correctStrings)
                {
                    var spanToParse = new ReadOnlySpan<char>(correctString.String.ToCharArray());
                    var expectedBytes = correctString.Bytes;

                    var parsed = ObjectId.TryParse(spanToParse, formatProvider, out var objectId);

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
                foreach (var formatProvider in GetNullableFormatProviders())
                foreach (var largeString in incorrectLargeStrings)
                {
                    Assert.That(ObjectId.TryParse(largeString, formatProvider, out _), Is.False);
                }
            }
        );
    }

    private void TryParseIncorrectSpan(string[] incorrectLargeStrings)
    {
        Assert.Multiple(
            () =>
            {
                foreach (var formatProvider in GetNullableFormatProviders())
                foreach (var largeString in incorrectLargeStrings)
                {
                    var largeSpan = new ReadOnlySpan<char>(largeString.ToCharArray());
                    Assert.That(ObjectId.TryParse(largeSpan, formatProvider, out _), Is.False);
                }
            }
        );
    }

    #endregion
}
