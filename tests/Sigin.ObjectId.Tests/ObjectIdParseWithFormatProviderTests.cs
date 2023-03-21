using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Sigin.ObjectId.Tests.Data;

namespace Sigin.ObjectId.Tests;

public class ObjectIdParseWithFormatProviderTests
{
    private const string? NullString = null;

    [SuppressMessage("ReSharper", "RedundantCast")]
    private static IEnumerable GetFormatProviders()
    {
        yield return (IFormatProvider?) CultureInfo.InvariantCulture;
        yield return (IFormatProvider?) new CultureInfo("en-US");
        yield return (IFormatProvider?) null!;
    }

    [Test]
    public void ParseNullStringShouldThrows([ValueSource(nameof(GetFormatProviders))] IFormatProvider formatProvider)
    {
        Assert.Throws<ArgumentNullException>(
            () =>
            {
#nullable disable
                var _ = ObjectId.Parse(NullString!, formatProvider);
#nullable restore
            }
        );
    }

    [Test]
    public void ParseEmptyStringShouldThrows([ValueSource(nameof(GetFormatProviders))] IFormatProvider formatProvider)
    {
        Assert.Throws<FormatException>(
            () =>
            {
                var _ = ObjectId.Parse(string.Empty, formatProvider);
            }
        );
    }

    [Test]
    public void ParseEmptySpanShouldThrows([ValueSource(nameof(GetFormatProviders))] IFormatProvider formatProvider)
    {
        Assert.Throws<FormatException>(
            () =>
            {
                var _ = ObjectId.Parse(new ReadOnlySpan<char>(Array.Empty<char>()), formatProvider);
            }
        );
    }

    #region ParseN

    [Test]
    public unsafe void ParseCorrectNString([ValueSource(nameof(GetFormatProviders))] IFormatProvider formatProvider)
    {
        Assert.Multiple(
            () =>
            {
                foreach (var correctNString in ObjectIdTestData.CorrectNStrings)
                {
                    var nString = correctNString.String;
                    var expectedBytes = correctNString.Bytes;

                    var parsedObjectId = ObjectId.Parse(nString, formatProvider);

                    var actualBytes = new byte[12];
                    fixed (byte* pinnedActualBytes = actualBytes)
                    {
                        *(ObjectId*) pinnedActualBytes = parsedObjectId;
                    }

                    Assert.That(actualBytes, Is.EqualTo(expectedBytes));
                }
            }
        );
    }

    [Test]
    public unsafe void ParseCorrectNSpan([ValueSource(nameof(GetFormatProviders))] IFormatProvider formatProvider)
    {
        Assert.Multiple(
            () =>
            {
                foreach (var correctNString in ObjectIdTestData.CorrectNStrings)
                {
                    var nSpan = new ReadOnlySpan<char>(correctNString.String.ToCharArray());
                    var expectedBytes = correctNString.Bytes;

                    var parsedObjectId = ObjectId.Parse(nSpan, formatProvider);

                    var actualBytes = new byte[12];
                    fixed (byte* pinnedActualBytes = actualBytes)
                    {
                        *(ObjectId*) pinnedActualBytes = parsedObjectId;
                    }

                    Assert.That(actualBytes, Is.EqualTo(expectedBytes));
                }
            }
        );
    }

    [Test]
    public void ParseNIncorrectLargeString([ValueSource(nameof(GetFormatProviders))] IFormatProvider formatProvider)
    {
        Assert.Multiple(
            () =>
            {
                foreach (var largeNString in ObjectIdTestData.LargeNStrings)
                {
                    Assert.Throws<FormatException>(
                        () =>
                        {
                            var _ = ObjectId.Parse(largeNString, formatProvider);
                        }
                    );
                }
            }
        );
    }

    [Test]
    public void ParseNIncorrectLargeSpan([ValueSource(nameof(GetFormatProviders))] IFormatProvider formatProvider)
    {
        Assert.Multiple(
            () =>
            {
                foreach (var largeNString in ObjectIdTestData.LargeNStrings)
                {
                    Assert.Throws<FormatException>(
                        () =>
                        {
                            var largeNSpan = new ReadOnlySpan<char>(largeNString.ToCharArray());
                            var _ = ObjectId.Parse(largeNSpan, formatProvider);
                        }
                    );
                }
            }
        );
    }

    [Test]
    public void ParseNIncorrectSmallString([ValueSource(nameof(GetFormatProviders))] IFormatProvider formatProvider)
    {
        Assert.Multiple(
            () =>
            {
                foreach (var smallNString in ObjectIdTestData.SmallNStrings)
                {
                    Assert.Throws<FormatException>(
                        () =>
                        {
                            var _ = ObjectId.Parse(smallNString, formatProvider);
                        }
                    );
                }
            }
        );
    }

    [Test]
    public void ParseNIncorrectSmallSpan([ValueSource(nameof(GetFormatProviders))] IFormatProvider formatProvider)
    {
        Assert.Multiple(
            () =>
            {
                foreach (var smallNString in ObjectIdTestData.SmallNStrings)
                {
                    Assert.Throws<FormatException>(
                        () =>
                        {
                            var smallNSpan = new ReadOnlySpan<char>(smallNString.ToCharArray());
                            var _ = ObjectId.Parse(smallNSpan, formatProvider);
                        }
                    );
                }
            }
        );
    }

    [Test]
    public void ParseIncorrectNString([ValueSource(nameof(GetFormatProviders))] IFormatProvider formatProvider)
    {
        Assert.Multiple(
            () =>
            {
                foreach (var brokenNString in ObjectIdTestData.BrokenNStrings)
                {
                    Assert.Throws<FormatException>(
                        () =>
                        {
                            var _ = ObjectId.Parse(brokenNString, formatProvider);
                        }
                    );
                }
            }
        );
    }

    [Test]
    public void ParseIncorrectNSpan([ValueSource(nameof(GetFormatProviders))] IFormatProvider formatProvider)
    {
        Assert.Multiple(
            () =>
            {
                foreach (var brokenNString in ObjectIdTestData.BrokenNStrings)
                {
                    Assert.Throws<FormatException>(
                        () =>
                        {
                            var brokenNSpan = new ReadOnlySpan<char>(brokenNString.ToCharArray());
                            var _ = ObjectId.Parse(brokenNSpan, formatProvider);
                        }
                    );
                }
            }
        );
    }

    #endregion
}
