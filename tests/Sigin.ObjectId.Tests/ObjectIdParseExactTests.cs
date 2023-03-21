using Sigin.ObjectId.Tests.Data;

namespace Sigin.ObjectId.Tests;

public class ObjectIdParseExactTests
{
    private const string? NullString = null;

    [Test]
    public void ParseExactNullStringCorrectFormatShouldThrows()
    {
        Assert.Multiple(
            () =>
            {
                foreach (var format in ObjectIdTestData.Formats.All)
                {
                    Assert.Throws<ArgumentNullException>(
                        () =>
                        {
#pragma warning disable 8625
                            var _ = ObjectId.ParseExact(NullString, format);
#pragma warning restore 8625
                        }
                    );
                }
            }
        );
    }

    [Test]
    public void ParseExactCorrectStringNullFormatShouldThrows()
    {
        Assert.Multiple(
            () =>
            {
                foreach (var correctNString in ObjectIdTestData.CorrectNStrings)
                {
                    Assert.Throws<ArgumentNullException>(
                        () =>
                        {
#pragma warning disable 8625
                            var _ = ObjectId.ParseExact(correctNString.String, NullString);
#pragma warning restore 8625
                        }
                    );
                }
            }
        );
    }

    [Test]
    public void ParseExactCorrectStringIncorrectFormatShouldThrows()
    {
        Assert.Multiple(
            () =>
            {
                foreach (var correctNString in ObjectIdTestData.CorrectNStrings)
                {
                    Assert.Throws<FormatException>(
                        () =>
                        {
                            // ReSharper disable once RedundantCast
                            var _ = ObjectId.ParseExact(correctNString.String, "И");
                        }
                    );
                }
            }
        );
    }

    [Test]
    public void ParseExactEmptyStringCorrectFormatShouldThrows()
    {
        Assert.Multiple(
            () =>
            {
                foreach (var format in ObjectIdTestData.Formats.All)
                {
                    Assert.Throws<FormatException>(
                        () =>
                        {
                            var _ = ObjectId.ParseExact(string.Empty, format);
                        }
                    );
                }
            }
        );
    }

    [Test]
    public void ParseExactEmptySpanCorrectFormatShouldThrows()
    {
        Assert.Multiple(
            () =>
            {
                foreach (var format in ObjectIdTestData.Formats.All)
                {
                    Assert.Throws<FormatException>(
                        () =>
                        {
                            var formatSpan = new ReadOnlySpan<char>(format.ToCharArray());
                            var _ = ObjectId.ParseExact(new ReadOnlySpan<char>(Array.Empty<char>()), formatSpan);
                        }
                    );
                }
            }
        );
    }

    [Test]
    public void ParseExactCorrectSpanEmptyFormatShouldThrows()
    {
        Assert.Multiple(
            () =>
            {
                foreach (var correctNString in ObjectIdTestData.CorrectNStrings)
                {
                    Assert.Throws<FormatException>(
                        () =>
                        {
                            var nStringSpan = new ReadOnlySpan<char>(correctNString.String.ToCharArray());
                            var formatSpan = new ReadOnlySpan<char>(Array.Empty<char>());
                            // ReSharper disable once RedundantCast
                            var _ = ObjectId.ParseExact(nStringSpan, formatSpan);
                        }
                    );
                }
            }
        );
    }

    [Test]
    public void ParseExactCorrectSpanIncorrectFormatShouldThrows()
    {
        Assert.Multiple(
            () =>
            {
                foreach (var correctNString in ObjectIdTestData.CorrectNStrings)
                {
                    Assert.Throws<FormatException>(
                        () =>
                        {
                            var nStringSpan = new ReadOnlySpan<char>(correctNString.String.ToCharArray());
                            var formatSpan = new ReadOnlySpan<char>(new[] {'И'});
                            // ReSharper disable once RedundantCast
                            var _ = ObjectId.ParseExact(nStringSpan, formatSpan);
                        }
                    );
                }
            }
        );
    }

    #region ParseExactN

    [Test]
    public unsafe void ParseExactCorrectNCorrectFormat()
    {
        Assert.Multiple(
            () =>
            {
                foreach (var correctNString in ObjectIdTestData.CorrectNStrings)
                {
                    var results = new List<byte[]>();
                    foreach (var format in ObjectIdTestData.Formats.N)
                    {
                        var parsedObjectIdString = ObjectId.ParseExact(correctNString.String, format);
                        var parsedObjectIdSpan = ObjectId.ParseExact(
                            new ReadOnlySpan<char>(correctNString.String.ToCharArray()),
                            new ReadOnlySpan<char>(format.ToCharArray())
                        );

                        var actualBytesString = new byte[12];
                        var actualBytesSpan = new byte[12];
                        fixed (byte* pinnedString = actualBytesString, pinnedSpan = actualBytesSpan)
                        {
                            *(ObjectId*) pinnedString = parsedObjectIdString;
                            *(ObjectId*) pinnedSpan = parsedObjectIdSpan;
                        }

                        results.Add(actualBytesString);
                        results.Add(actualBytesSpan);
                    }

                    foreach (var result in results)
                    {
                        Assert.That(result, Is.EqualTo(correctNString.Bytes));
                    }
                }
            }
        );
    }

    [Test]
    public void ParseExactCorrectNIncorrectFormat()
    {
        Assert.Multiple(
            () =>
            {
                foreach (var correctNString in ObjectIdTestData.CorrectNStrings)
                foreach (var format in ObjectIdTestData.Formats.AllExceptN)
                {
                    Assert.Throws<FormatException>(
                        () =>
                        {
                            var _ = ObjectId.ParseExact(correctNString.String, format);
                        }
                    );

                    Assert.Throws<FormatException>(
                        () =>
                        {
                            var _ = ObjectId.ParseExact(
                                new ReadOnlySpan<char>(correctNString.String.ToCharArray()),
                                new ReadOnlySpan<char>(format.ToCharArray())
                            );
                        }
                    );
                }
            }
        );
    }

    [Test]
    public void ParseExactIncorrectNCorrectFormat()
    {
        Assert.Multiple(
            () =>
            {
                foreach (var brokenNString in ObjectIdTestData.BrokenNStrings)
                foreach (var format in ObjectIdTestData.Formats.N)
                {
                    Assert.Throws<FormatException>(
                        () =>
                        {
                            var _ = ObjectId.ParseExact(brokenNString, format);
                        }
                    );

                    Assert.Throws<FormatException>(
                        () =>
                        {
                            var _ = ObjectId.ParseExact(
                                new ReadOnlySpan<char>(brokenNString.ToCharArray()),
                                new ReadOnlySpan<char>(format.ToCharArray())
                            );
                        }
                    );
                }
            }
        );
    }

    #endregion
}
