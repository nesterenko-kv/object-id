using Sigin.ObjectId.Tests.Data;
using Sigin.ObjectId.Tests.Data.Models;

namespace Sigin.ObjectId.Tests;

public class ObjectIdTryParseExactTests
{
    private const string? NullString = null;

    [Test]
    public void TryParseExactNullStringCorrectFormatShouldFalse()
    {
        foreach (var format in ObjectIdTestData.Formats.All)
        {
            var parsed = ObjectId.TryParseExact(NullString, format, out var objectId);
            Assert.Multiple(
                () =>
                {
                    Assert.That(parsed, Is.False);
                    Assert.That(objectId, Is.EqualTo(ObjectId.Empty));
                }
            );
        }
    }

    [Test]
    public void TryParseExactCorrectStringNullFormatShouldFalse()
    {
        Assert.Multiple(
            () =>
            {
                foreach (var correctNString in ObjectIdTestData.CorrectNStrings)
                {
                    var parsed = ObjectId.TryParseExact(correctNString.String, NullString, out var objectId);
                    Assert.That(parsed, Is.False);
                    Assert.That(objectId, Is.EqualTo(ObjectId.Empty));
                }
            }
        );
    }

    [Test]
    public void TryParseExactCorrectStringIncorrectFormatShouldFalse()
    {
        Assert.Multiple(
            () =>
            {
                foreach (var correctNString in ObjectIdTestData.CorrectNStrings)
                {
                    var parsed = ObjectId.TryParseExact(correctNString.String, "И", out var objectId);
                    Assert.That(parsed, Is.False);
                    Assert.That(objectId, Is.EqualTo(ObjectId.Empty));
                }
            }
        );
    }

    [Test]
    public void TryParseExactEmptyStringCorrectFormatShouldFalse()
    {
        Assert.Multiple(
            () =>
            {
                foreach (var format in ObjectIdTestData.Formats.All)
                {
                    var parsed = ObjectId.TryParseExact(string.Empty, format, out var objectId);
                    Assert.That(parsed, Is.False);
                    Assert.That(objectId, Is.EqualTo(ObjectId.Empty));
                }
            }
        );
    }

    [Test]
    public void TryParseExactEmptySpanCorrectFormatShouldFalse()
    {
        Assert.Multiple(
            () =>
            {
                foreach (var format in ObjectIdTestData.Formats.All)
                {
                    var stringSpan = new ReadOnlySpan<char>(Array.Empty<char>());
                    var formatSpan = new ReadOnlySpan<char>(format.ToCharArray());
                    var parsed = ObjectId.TryParseExact(stringSpan, formatSpan, out var objectId);
                    Assert.That(parsed, Is.False);
                    Assert.That(objectId, Is.EqualTo(ObjectId.Empty));
                }
            }
        );
    }

    [Test]
    public void TryParseExactCorrectSpanEmptyFormatShouldFalse()
    {
        Assert.Multiple(
            () =>
            {
                foreach (var correctNString in ObjectIdTestData.CorrectNStrings)
                {
                    var stringSpan = new ReadOnlySpan<char>(correctNString.String.ToCharArray());
                    var formatSpan = new ReadOnlySpan<char>(Array.Empty<char>());
                    var parsed = ObjectId.TryParseExact(stringSpan, formatSpan, out var objectId);
                    Assert.That(parsed, Is.False);
                    Assert.That(objectId, Is.EqualTo(ObjectId.Empty));
                }
            }
        );
    }

    [Test]
    public void TryParseExactCorrectSpanIncorrectFormatShouldFalse()
    {
        Assert.Multiple(
            () =>
            {
                foreach (var correctNString in ObjectIdTestData.CorrectNStrings)
                {
                    var stringSpan = new ReadOnlySpan<char>(correctNString.String.ToCharArray());
                    var formatSpan = new ReadOnlySpan<char>(new[] {'И'});
                    var parsed = ObjectId.TryParseExact(stringSpan, formatSpan, out var objectId);
                    Assert.That(parsed, Is.False);
                    Assert.That(objectId, Is.EqualTo(ObjectId.Empty));
                }
            }
        );
    }

    #region TryParseExactN

    [Test]
    public void TryParseExactCorrectNCorrectFormat()
    {
        TryParseExactCorrectStringCorrectFormat(
            ObjectIdTestData.CorrectNStrings,
            ObjectIdTestData.Formats.N
        );
    }

    [Test]
    public void ParseExactCorrectNIncorrectFormat()
    {
        TryParseExactCorrectStringIncorrectFormat(
            ObjectIdTestData.CorrectNStrings,
            ObjectIdTestData.Formats.AllExceptN
        );
    }

    [Test]
    public void ParseExactIncorrectNCorrectFormat()
    {
        TryParseExactIncorrectStringCorrectFormat(
            ObjectIdTestData.BrokenNStrings,
            ObjectIdTestData.Formats.N
        );
    }

    #endregion

    #region Helpers

    private unsafe void TryParseExactCorrectStringCorrectFormat(
        ObjectIdStringWithBytes[] correctStrings,
        string[] correctFormats
        )
    {
        Assert.Multiple(
            () =>
            {
                foreach (var correctString in correctStrings)
                foreach (var format in correctFormats)
                {
                    var isParsedFromString = ObjectId.TryParseExact(
                        correctString.String,
                        format,
                        out var parsedObjectIdFromString
                    );
                    var isParsedBoolFromSpan = ObjectId.TryParseExact(
                        new ReadOnlySpan<char>(correctString.String.ToCharArray()),
                        new ReadOnlySpan<char>(format.ToCharArray()),
                        out var parsedObjectIdFromSpan
                    );

                    var actualBytesString = new byte[12];
                    var actualBytesSpan = new byte[12];
                    fixed (byte* pinnedString = actualBytesString, pinnedSpan = actualBytesSpan)
                    {
                        *(ObjectId*) pinnedString = parsedObjectIdFromString;
                        *(ObjectId*) pinnedSpan = parsedObjectIdFromSpan;
                    }

                    Assert.That(isParsedFromString, Is.True);
                    Assert.That(isParsedBoolFromSpan, Is.True);
                    Assert.That(actualBytesString, Is.EqualTo(correctString.Bytes));
                    Assert.That(actualBytesSpan, Is.EqualTo(correctString.Bytes));
                }
            }
        );
    }

    private static readonly byte[] ExpectedEmptyObjectIdBytes = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};

    private unsafe void TryParseExactCorrectStringIncorrectFormat(
        ObjectIdStringWithBytes[] correctStrings,
        string[] incorrectFormats
        )
    {
        Assert.Multiple(
            () =>
            {
                foreach (var correctString in correctStrings)
                foreach (var incorrectFormat in incorrectFormats)
                {
                    var isParsedFromString = ObjectId.TryParseExact(
                        correctString.String,
                        incorrectFormat,
                        out var parsedObjectIdFromString
                    );
                    var isParsedBoolFromSpan = ObjectId.TryParseExact(
                        new ReadOnlySpan<char>(correctString.String.ToCharArray()),
                        new ReadOnlySpan<char>(incorrectFormat.ToCharArray()),
                        out var parsedObjectIdFromSpan
                    );

                    var actualBytesString = new byte[12];
                    var actualBytesSpan = new byte[12];
                    fixed (byte* pinnedString = actualBytesString, pinnedSpan = actualBytesSpan)
                    {
                        *(ObjectId*) pinnedString = parsedObjectIdFromString;
                        *(ObjectId*) pinnedSpan = parsedObjectIdFromSpan;
                    }

                    Assert.That(isParsedFromString, Is.False);
                    Assert.That(isParsedBoolFromSpan, Is.False);
                    Assert.That(actualBytesString, Is.EqualTo(ExpectedEmptyObjectIdBytes));
                    Assert.That(actualBytesSpan, Is.EqualTo(ExpectedEmptyObjectIdBytes));
                }
            }
        );
    }

    private unsafe void TryParseExactIncorrectStringCorrectFormat(
        string[] brokenStrings,
        string[] correctFormats
        )
    {
        Assert.Multiple(
            () =>
            {
                foreach (var brokenString in brokenStrings)
                foreach (var correctFormat in correctFormats)
                {
                    var isParsedFromString = ObjectId.TryParseExact(
                        brokenString,
                        correctFormat,
                        out var parsedObjectIdFromString
                    );
                    var isParsedBoolFromSpan = ObjectId.TryParseExact(
                        new ReadOnlySpan<char>(brokenString.ToCharArray()),
                        new ReadOnlySpan<char>(correctFormat.ToCharArray()),
                        out var parsedObjectIdFromSpan
                    );

                    var actualBytesString = new byte[12];
                    var actualBytesSpan = new byte[12];
                    fixed (byte* pinnedString = actualBytesString, pinnedSpan = actualBytesSpan)
                    {
                        *(ObjectId*) pinnedString = parsedObjectIdFromString;
                        *(ObjectId*) pinnedSpan = parsedObjectIdFromSpan;
                    }

                    Assert.That(isParsedFromString, Is.False);
                    Assert.That(isParsedBoolFromSpan, Is.False);
                    Assert.That(actualBytesString, Is.EqualTo(ExpectedEmptyObjectIdBytes));
                    Assert.That(actualBytesSpan, Is.EqualTo(ExpectedEmptyObjectIdBytes));
                }
            }
        );
    }

    #endregion
}
