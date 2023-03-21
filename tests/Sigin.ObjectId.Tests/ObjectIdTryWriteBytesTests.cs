using Sigin.ObjectId.Tests.Data;

namespace Sigin.ObjectId.Tests;

public class ObjectIdTryWriteBytesTests
{
    [TestCaseSource(typeof(ObjectIdTestData), nameof(ObjectIdTestData.CorrectObjectIdBytesArrays))]
    public unsafe void ToByteArrayExactOutputSize(byte[] correctBytes)
    {
        var objectId = new ObjectId(correctBytes);
        var buffer = stackalloc byte[12];
        var output = new Span<byte>(buffer, length: 12);

        var wasWritten = objectId.TryWriteBytes(output);

        var outputBytes = output.ToArray();
        Assert.Multiple(
            () =>
            {
                Assert.That(wasWritten, Is.True);
                Assert.That(outputBytes, Is.EqualTo(correctBytes));
            }
        );
    }

    [TestCaseSource(typeof(ObjectIdTestData), nameof(ObjectIdTestData.CorrectObjectIdBytesArrays))]
    public unsafe void ToByteArrayLargeOutputSize(byte[] correctBytes)
    {
        var objectId = new ObjectId(correctBytes);
        var buffer = stackalloc byte[512];
        var output = new Span<byte>(buffer, length: 512);

        var wasWritten = objectId.TryWriteBytes(output);

        var outputBytes = output[..12].ToArray();
        Assert.Multiple(
            () =>
            {
                Assert.That(wasWritten, Is.True);
                Assert.That(outputBytes, Is.EqualTo(correctBytes));
            }
        );
    }

    [TestCaseSource(typeof(ObjectIdTestData), nameof(ObjectIdTestData.CorrectObjectIdBytesArrays))]
    public unsafe void ToByteArraySmallOutputSize(byte[] correctBytes)
    {
        var objectId = new ObjectId(correctBytes);
        var buffer = stackalloc byte[4];
        var output = new Span<byte>(buffer, length: 4);

        var wasWritten = objectId.TryWriteBytes(output);

        var outputBytes = output.ToArray();
        Assert.Multiple(
            () =>
            {
                Assert.That(wasWritten, Is.False);
                Assert.That(outputBytes, Is.Not.EqualTo(correctBytes));
            }
        );
    }
}
