namespace Sigin.ObjectId.Tests;

public class ObjectIdGeneratorTests
{
    [Test]
    public unsafe void NewObjectId()
    {
        var startDate = DateTimeOffset.UtcNow;
        var objectId = ObjectId.NewObjectId();
        var endDate = DateTimeOffset.UtcNow;
        var objectIdPtr = (byte*) &objectId;
        var seconds = (endDate - startDate).Ticks / TimeSpan.TicksPerSecond + 1;

        for (var i = 0; i < seconds; i++)
        {
            var attemptSeconds = startDate.Ticks / TimeSpan.TicksPerSecond + i -
                                 DateTimeOffset.UnixEpoch.Ticks / TimeSpan.TicksPerSecond;
            var secondsPtr = (byte*) &attemptSeconds;
            if (IsObjectIdForSpecifiedTime(secondsPtr, objectIdPtr))
            {
                Assert.Pass();
            }
        }

        Assert.Fail("Could not find time when ObjectID was generated, or generation was broken");
    }

    private unsafe bool IsObjectIdForSpecifiedTime(byte* secondsPtr, byte* objectIdBytes)
    {
        return objectIdBytes[0] == secondsPtr[3]
               && objectIdBytes[1] == secondsPtr[2]
               && objectIdBytes[2] == secondsPtr[1]
               && objectIdBytes[3] == secondsPtr[0];
    }
}
