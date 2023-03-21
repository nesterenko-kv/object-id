using Sigin.ObjectId.Tests.Data;

namespace Sigin.ObjectId.Tests;

public class ObjectIdEqualsTests
{
    [TestCaseSource(typeof(ObjectIdTestData), nameof(ObjectIdTestData.CorrectObjectIdBytesArrays))]
    public void EqualsWithObjectNullReturnFalse(byte[] correctBytes)
    {
        var objectId = new ObjectId(correctBytes);

        var isEquals = objectId.Equals(null);

        Assert.That(isEquals, Is.False);
    }

    [TestCaseSource(typeof(ObjectIdTestData), nameof(ObjectIdTestData.CorrectObjectIdBytesArrays))]
    public void EqualsWithObjectOtherTypeReturnFalse(byte[] correctBytes)
    {
        var objectId = new ObjectId(correctBytes);
        var objectWithAnotherType = (object) 42;

        var isEquals = objectId.Equals(objectWithAnotherType);

        Assert.That(isEquals, Is.False);
    }

    [TestCaseSource(typeof(ObjectIdTestData), nameof(ObjectIdTestData.CorrectEqualsToBytesAndResult))]
    public void EqualsWithObjectObjectId(
        byte[] correctBytes,
        byte[] correctEqualsBytes,
        bool expectedResult
        )
    {
        var objectId = new ObjectId(correctBytes);
        var objectObjectId = (object) new ObjectId(correctEqualsBytes);

        var isEquals = objectId.Equals(objectObjectId);

        Assert.That(isEquals, Is.EqualTo(expectedResult));
    }

    [TestCaseSource(typeof(ObjectIdTestData), nameof(ObjectIdTestData.CorrectEqualsToBytesAndResult))]
    public void EqualsWithOtherObjectId(
        byte[] correctBytes,
        byte[] correctEqualsBytes,
        bool expectedResult
        )
    {
        var objectId = new ObjectId(correctBytes);
        var otherObjectId = new ObjectId(correctEqualsBytes);

        var isEquals = objectId.Equals(otherObjectId);

        Assert.That(isEquals, Is.EqualTo(expectedResult));
    }
}
