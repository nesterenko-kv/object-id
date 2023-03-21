using Sigin.ObjectId.Tests.Data;

namespace Sigin.ObjectId.Tests;

public class ObjectIdGetHashCodeTests
{
    [TestCaseSource(typeof(ObjectIdTestData), nameof(ObjectIdTestData.CorrectObjectIdBytesArrays))]
    public unsafe void GetHashCode(byte[] correctBytes)
    {
        var objectId = new ObjectId(correctBytes);
        var objectIdPtr = stackalloc ObjectId[1];
        objectIdPtr[0] = objectId;

        var intPtr = (int*) objectIdPtr;
        var int0 = intPtr[0];
        var int1 = intPtr[1];
        var int2 = intPtr[2];
        var int3 = intPtr[3];

        var expectedHashCode = int0 ^ int1 ^ int2 ^ int3;

        var actualHashCode = objectId.GetHashCode();

        Assert.That(actualHashCode, Is.EqualTo(expectedHashCode));
    }
}
