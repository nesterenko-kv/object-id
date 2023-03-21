using Sigin.ObjectId.Tests.Data;

namespace Sigin.ObjectId.Tests;

public class ObjectIdOperatorsTests
{
    #region ==

    [Test]
    [TestCaseSource(typeof(ObjectIdTestData), nameof(ObjectIdTestData.CorrectEqualsToBytesAndResult))]
    public void EqualsOperator(
        byte[] correctBytes,
        byte[] correctEqualsBytes,
        bool expectedResult
        )
    {
        var objectId = new ObjectId(correctBytes);
        var otherObjectId = new ObjectId(correctEqualsBytes);

        var isEquals = objectId == otherObjectId;

        Assert.That(isEquals, Is.EqualTo(expectedResult));
    }

    #endregion

    #region !=

    [TestCaseSource(typeof(ObjectIdTestData), nameof(ObjectIdTestData.CorrectEqualsToBytesAndResult))]
    public void NotEqualsOperator(
        byte[] correctBytes,
        byte[] correctEqualsBytes,
        bool notExpectedResult
        )
    {
        var expectedResult = !notExpectedResult;
        var objectId = new ObjectId(correctBytes);
        var otherObjectId = new ObjectId(correctEqualsBytes);

        var isEquals = objectId != otherObjectId;

        Assert.That(isEquals, Is.EqualTo(expectedResult));
    }

    #endregion

    #region <

    [TestCaseSource(typeof(ObjectIdTestData), nameof(ObjectIdTestData.LeftLessThanRight))]
    public void LessThan_ReturnsTrue_WhenLeftLessThanRight(ObjectId left, ObjectId right)
    {
        var isLeftLessThatRight = left < right;
        Assert.That(isLeftLessThatRight, Is.True);
    }

    [Test]
    public void LessThan_ReturnsFalse_WhenLeftEqualsToRight()
    {
        var objectIdString = "a0b8e3b45fab11eda0f8378e";
        var left = new ObjectId(objectIdString);
        var right = new ObjectId(objectIdString);

        var isLeftLessThatRight = left < right;

        Assert.That(isLeftLessThatRight, Is.False);
    }

    [TestCaseSource(typeof(ObjectIdTestData), nameof(ObjectIdTestData.RightLessThanLeft))]
    public void LessThan_ReturnsFalse_WhenRightLessThanLeft(ObjectId left, ObjectId right)
    {
        var isLeftLessThatRight = left < right;
        Assert.That(isLeftLessThatRight, Is.False);
    }

    #endregion

    #region <=

    [TestCaseSource(typeof(ObjectIdTestData), nameof(ObjectIdTestData.LeftLessThanRight))]
    public void LessThanOrEqual_ReturnsTrue_WhenLeftLessThanRight(ObjectId left, ObjectId right)
    {
        var isLeftLessThatRight = left <= right;
        Assert.That(isLeftLessThatRight, Is.True);
    }

    [Test]
    public void LessThanOrEqual_ReturnsTrue_WhenLeftEqualsToRight()
    {
        var objectIdString = "a0b8e3b45fab11eda0f8378e";
        var left = new ObjectId(objectIdString);
        var right = new ObjectId(objectIdString);

        var isLeftLessThatRight = left <= right;

        Assert.That(isLeftLessThatRight, Is.True);
    }

    [TestCaseSource(typeof(ObjectIdTestData), nameof(ObjectIdTestData.RightLessThanLeft))]
    public void LessThanOrEqual_ReturnsFalse_WhenRightLessThanLeft(ObjectId left, ObjectId right)
    {
        var isLeftLessThatRight = left <= right;
        Assert.That(isLeftLessThatRight, Is.False);
    }

    #endregion

    #region >

    [TestCaseSource(typeof(ObjectIdTestData), nameof(ObjectIdTestData.RightLessThanLeft))]
    public void GreaterThan_ReturnsTrue_WhenLeftGreaterThanRight(ObjectId left, ObjectId right)
    {
        var isLeftLessThatRight = left > right;
        Assert.That(isLeftLessThatRight, Is.True);
    }

    [Test]
    public void GreaterThan_ReturnsFalse_WhenLeftEqualsToRight()
    {
        var objectIdString = "a0b8e3b45fab11eda0f8378e";
        var left = new ObjectId(objectIdString);
        var right = new ObjectId(objectIdString);

        var isLeftLessThatRight = left > right;

        Assert.That(isLeftLessThatRight, Is.False);
    }

    [TestCaseSource(typeof(ObjectIdTestData), nameof(ObjectIdTestData.LeftLessThanRight))]
    public void GreaterThan_ReturnsFalse_WhenRightGreaterThanLeft(ObjectId left, ObjectId right)
    {
        var isLeftLessThatRight = left > right;
        Assert.That(isLeftLessThatRight, Is.False);
    }

    #endregion

    #region >=

    [TestCaseSource(typeof(ObjectIdTestData), nameof(ObjectIdTestData.RightLessThanLeft))]
    public void GreaterThanOrEqual_ReturnsTrue_WhenLeftGreaterThanRight(ObjectId left, ObjectId right)
    {
        var isLeftLessThatRight = left >= right;
        Assert.That(isLeftLessThatRight, Is.True);
    }

    [Test]
    public void GreaterThanOrEqual_ReturnsFalse_WhenLeftEqualsToRight()
    {
        var objectIdString = "a0b8e3b45fab11eda0f8378e";
        var left = new ObjectId(objectIdString);
        var right = new ObjectId(objectIdString);

        var isLeftLessThatRight = left >= right;

        Assert.That(isLeftLessThatRight, Is.True);
    }

    [TestCaseSource(typeof(ObjectIdTestData), nameof(ObjectIdTestData.LeftLessThanRight))]
    public void GreaterThanOrEqual_ReturnsFalse_WhenRightGreaterThanLeft(ObjectId left, ObjectId right)
    {
        var isLeftLessThatRight = left >= right;
        Assert.That(isLeftLessThatRight, Is.False);
    }

    #endregion
}
