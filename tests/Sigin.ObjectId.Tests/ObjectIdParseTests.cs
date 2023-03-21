using Sigin.ObjectId.Tests.Data;

namespace Sigin.ObjectId.Tests;

public class ObjectIdParseTests
{
    private const string? NullString = null;

    [Test]
    public void ParseNullStringShouldThrows()
    {
        Assert.Throws<ArgumentNullException>(
            () =>
            {
#nullable disable
                var _ = ObjectId.Parse(NullString!);
#nullable restore
            }
        );
    }

    [Test]
    public void ParseEmptyStringShouldThrows()
    {
        Assert.Throws<FormatException>(
            () =>
            {
                var _ = ObjectId.Parse(string.Empty);
            }
        );
    }

    [Test]
    public void ParseEmptySpanShouldThrows()
    {
        Assert.Throws<FormatException>(
            () =>
            {
                var _ = ObjectId.Parse(new ReadOnlySpan<char>(Array.Empty<char>()));
            }
        );
    }

    #region ParseN

    [Test]
    public unsafe void ParseCorrectNString()
    {
        Assert.Multiple(
            () =>
            {
                foreach (var correctNString in ObjectIdTestData.CorrectNStrings)
                {
                    var nString = correctNString.String;
                    var expectedBytes = correctNString.Bytes;
                    var parsedObjectId = ObjectId.Parse(nString);

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
    public unsafe void ParseCorrectNSpan()
    {
        Assert.Multiple(
            () =>
            {
                foreach (var correctNString in ObjectIdTestData.CorrectNStrings)
                {
                    var nSpan = new ReadOnlySpan<char>(correctNString.String.ToCharArray());
                    var expectedBytes = correctNString.Bytes;
                    var parsedObjectId = ObjectId.Parse(nSpan);

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
    public void ParseNIncorrectLargeString()
    {
        Assert.Multiple(
            () =>
            {
                foreach (var largeNString in ObjectIdTestData.LargeNStrings)
                {
                    Assert.Throws<FormatException>(
                        () =>
                        {
                            var _ = ObjectId.Parse(largeNString);
                        }
                    );
                }
            }
        );
    }

    [Test]
    public void ParseNIncorrectLargeSpan()
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
                            var _ = ObjectId.Parse(largeNSpan);
                        }
                    );
                }
            }
        );
    }

    [Test]
    public void ParseNIncorrectSmallString()
    {
        Assert.Multiple(
            () =>
            {
                foreach (var smallNString in ObjectIdTestData.SmallNStrings)
                {
                    Assert.Throws<FormatException>(
                        () =>
                        {
                            var _ = ObjectId.Parse(smallNString);
                        }
                    );
                }
            }
        );
    }

    [Test]
    public void ParseNIncorrectSmallSpan()
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
                            var _ = ObjectId.Parse(smallNSpan);
                        }
                    );
                }
            }
        );
    }

    [Test]
    public void ParseIncorrectNString()
    {
        Assert.Multiple(
            () =>
            {
                foreach (var brokenNString in ObjectIdTestData.BrokenNStrings)
                {
                    Assert.Throws<FormatException>(
                        () =>
                        {
                            var _ = ObjectId.Parse(brokenNString);
                        }
                    );
                }
            }
        );
    }

    [Test]
    public void ParseIncorrectNSpan()
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
                            var _ = ObjectId.Parse(brokenNSpan);
                        }
                    );
                }
            }
        );
    }

    #endregion
}
