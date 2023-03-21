using Sigin.ObjectId.Tests.Data;

namespace Sigin.ObjectId.Tests;

public class ObjectIdToByteArrayTests
{
    [TestCaseSource(typeof(ObjectIdTestData), nameof(ObjectIdTestData.CorrectObjectIdBytesArrays))]
    public void ToByteArray(byte[] correctBytes)
    {
        var objectId = new ObjectId(correctBytes);

        var objectIdBytes = objectId.ToByteArray();

        Assert.That(objectIdBytes, Is.EqualTo(correctBytes));
    }
}
