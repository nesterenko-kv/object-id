using System.Diagnostics.CodeAnalysis;
using Sigin.ObjectId.Tests.Data;

namespace Sigin.ObjectId.Tests;

public class ObjectIdCtorTests
{
    #region Bytes

    [TestCaseSource(typeof(ObjectIdTestData), nameof(ObjectIdTestData.CorrectObjectIdBytesArrays))]
    public unsafe void CtorFromByteArrayCorrectBytes(byte[] correctBytes)
    {
        var objectId = new ObjectId(correctBytes);

        var objectIdBytes = new byte[12];
        fixed (byte* pinnedObjectIdArray = objectIdBytes)
        {
            *(ObjectId*) pinnedObjectIdArray = objectId;
        }

        Assert.That(objectIdBytes, Is.EqualTo(correctBytes));
    }

    [Test]
    [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
    public void CtorFromByteArrayNullShouldThrows()
    {
#nullable disable
        Assert.Throws<ArgumentNullException>(
            () =>
            {
                var _ = new ObjectId((byte[]) null);
            }
        );
#nullable restore
    }

    [Test]
    public void CtorFromByteArrayNot12BytesShouldThrows()
    {
        Assert.Throws<ArgumentException>(
            () =>
            {
                var _ = new ObjectId(new byte[] {1, 2, 3});
            }
        );
    }

    [TestCaseSource(typeof(ObjectIdTestData), nameof(ObjectIdTestData.CorrectObjectIdBytesArrays))]
    public unsafe void CtorFromPtrCorrectData(byte[] correctBytes)
    {
        var bytePtr = stackalloc byte[correctBytes.Length];
        for (var i = 0; i < correctBytes.Length; i++)
        {
            bytePtr[i] = correctBytes[i];
        }

        var objectId = new ObjectId(bytePtr);

        var objectIdBytes = new byte[12];
        fixed (byte* pinnedObjectIdArray = objectIdBytes)
        {
            *(ObjectId*) pinnedObjectIdArray = objectId;
        }

        Assert.That(objectIdBytes, Is.EqualTo(correctBytes));
    }

    [TestCaseSource(typeof(ObjectIdTestData), nameof(ObjectIdTestData.CorrectObjectIdBytesArrays))]
    public unsafe void CtorFromReadOnlySpanCorrectBytes(byte[] correctBytes)
    {
        var span = new ReadOnlySpan<byte>(correctBytes);
        var objectId = new ObjectId(span);

        var objectIdBytes = new byte[12];
        fixed (byte* pinnedObjectIdArray = objectIdBytes)
        {
            *(ObjectId*) pinnedObjectIdArray = objectId;
        }

        Assert.That(objectIdBytes, Is.EqualTo(correctBytes));
    }

    [Test]
    public void CtorFromReadOnlySpanNot12BytesShouldThrows()
    {
        Assert.Throws<ArgumentException>(
            () =>
            {
                var span = new ReadOnlySpan<byte>(new byte[] {1, 2, 3});
                var _ = new ObjectId(span);
            }
        );
    }

    #endregion

    #region Chars_Strings

    [Test]
    [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
    public void CtorFromStringNullShouldThrows()
    {
        Assert.Throws<ArgumentNullException>(
            () =>
            {
#nullable disable
                var _ = new ObjectId((string) null);
#nullable restore
            }
        );
    }

    [Test]
    public void CtorFromStringEmptyShouldThrows()
    {
        Assert.Throws<FormatException>(
            () =>
            {
                var _ = new ObjectId(string.Empty);
            }
        );
    }

    [Test]
    public void CtorFromCharSpanEmptyShouldThrows()
    {
        Assert.Throws<FormatException>(
            () =>
            {
                var _ = new ObjectId(new ReadOnlySpan<char>(Array.Empty<char>()));
            }
        );
    }

    #region N

    [Test]
    public unsafe void CtorFromStringCorrectNString()
    {
        Assert.Multiple(
            () =>
            {
                foreach (var correctNString in ObjectIdTestData.CorrectNStrings)
                {
                    var nString = correctNString.String;
                    var expectedBytes = correctNString.Bytes;

                    var parsedObjectId = new ObjectId(nString);

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
    public unsafe void CtorFromCharSpanCorrectN()
    {
        Assert.Multiple(
            () =>
            {
                foreach (var correctNString in ObjectIdTestData.CorrectNStrings)
                {
                    var nSpan = new ReadOnlySpan<char>(correctNString.String.ToCharArray());
                    var expectedBytes = correctNString.Bytes;

                    var parsedObjectId = new ObjectId(nSpan);

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

    #endregion

    #endregion

    #region Components

    [Test]
    [TestCaseSource(typeof(ObjectIdTestData), nameof(ObjectIdTestData.CorrectObjectIdAndComponents))]
    public void CtorFromComponentsCorrect(long timestamp, long random, int increment, string result)
    {
        var objectId = ObjectId.NewObjectId(timestamp, random, increment);

        var actualStr = objectId.ToString();

        Assert.That(actualStr, Is.EqualTo(result));
    }

    [Test]
    [TestCase(0xFFFFFFFF + 1L)]
    [TestCase(-0xFFFFFFFF)]
    [TestCase(0 - 1L)]
    [TestCase(long.MaxValue)]
    [TestCase(long.MinValue)]
    [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
    public void CtorFromComponentsInvalidTimestampShouldThrows(long timestamp)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
            {
                var _ = ObjectId.NewObjectId(timestamp, random: 0, increment: 0);
            }
        );
    }

    [Test]
    [TestCase(0xFF_FFFF_FFFF + 1L)]
    [TestCase(-0xFF_FFFF_FFFF)]
    [TestCase(0 - 1L)]
    [TestCase(long.MaxValue)]
    [TestCase(long.MinValue)]
    [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
    public void CtorFromComponentsInvalidRandomShouldThrows(long random)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
            {
                var _ = ObjectId.NewObjectId(timestamp: 0, random, increment: 0);
            }
        );
    }

    [Test]
    [TestCase(0xFFFFFF + 1)]
    [TestCase(-1)]
    [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
    public void CtorFromComponentsInvalidIncrementShouldThrows(int increment)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
            {
                var _ = ObjectId.NewObjectId(timestamp: 0, random: 0, increment);
            }
        );
    }

    #endregion
}
