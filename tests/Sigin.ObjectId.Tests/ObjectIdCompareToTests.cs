using Sigin.ObjectId.Tests.Data;

namespace Sigin.ObjectId.Tests;

public class ObjectIdCompareToTests
{
    [TestCaseSource(typeof(ObjectIdTestData), nameof(ObjectIdTestData.CorrectCompareToArraysAndResult))]
    public void CompareToObjectCorrect(
        byte[] correctBytes,
        byte[] correctCompareToBytes,
        int expectedResult
        )
    {
        var objectId = new ObjectId(correctBytes);
        var objectIdToCompareAsObject = (object) new ObjectId(correctCompareToBytes);

        var compareResult = objectId.CompareTo(objectIdToCompareAsObject);

        Assert.That(compareResult, Is.EqualTo(expectedResult));
    }

    [TestCaseSource(typeof(ObjectIdTestData), nameof(ObjectIdTestData.CorrectObjectIdBytesArrays))]
    public void CompareToObjectNullShouldReturn1(byte[] correctBytes)
    {
        var objectId = new ObjectId(correctBytes);

        var compareResult = objectId.CompareTo(null);

        Assert.That(compareResult, Is.EqualTo(1));
    }

    [TestCaseSource(typeof(ObjectIdTestData), nameof(ObjectIdTestData.CorrectObjectIdBytesArrays))]
    public void CompareToObjectOtherTypeShouldThrows(byte[] correctBytes)
    {
        var objectId = new ObjectId(correctBytes);

        Assert.Throws<ArgumentException>(
            () =>
            {
                var _ = objectId.CompareTo(1337);
            }
        );
    }

    [TestCaseSource(typeof(ObjectIdTestData), nameof(ObjectIdTestData.CorrectCompareToArraysAndResult))]
    public void CompareToObjectIdCorrect(
        byte[] correctBytes,
        byte[] correctCompareToBytes,
        int expectedResult
        )
    {
        var objectId = new ObjectId(correctBytes);
        var objectIdToCompareAsObject = new ObjectId(correctCompareToBytes);

        var compareResult = objectId.CompareTo(objectIdToCompareAsObject);

        Assert.That(compareResult, Is.EqualTo(expectedResult));
    }
}
