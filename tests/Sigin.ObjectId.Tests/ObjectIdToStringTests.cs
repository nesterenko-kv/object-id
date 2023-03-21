using Sigin.ObjectId.Tests.Data;

namespace Sigin.ObjectId.Tests;

public class ObjectIdToStringTests
{
    [TestCaseSource(typeof(ObjectIdTestData), nameof(ObjectIdTestData.CorrectObjectIdBytesArrays))]
    public void ToString(byte[] correctBytes)
    {
        var objectId = new ObjectId(correctBytes);
        var expectedString = ObjectIdTestsUtils.GetStringN(correctBytes);

        var actualString = objectId.ToString();

        Assert.That(actualString, Is.EqualTo(expectedString));
    }

    [TestCaseSource(typeof(ObjectIdTestData), nameof(ObjectIdTestData.CorrectObjectIdBytesArrays))]
    public void ToStringNullFormat(byte[] correctBytes)
    {
        var objectId = new ObjectId(correctBytes);
        var expectedString = ObjectIdTestsUtils.GetStringN(correctBytes);

        var actualString = objectId.ToString(null);

        Assert.That(actualString, Is.EqualTo(expectedString));
    }

    [TestCaseSource(typeof(ObjectIdTestData), nameof(ObjectIdTestData.CorrectObjectIdBytesArrays))]
    public void ToStringEmptyFormat(byte[] correctBytes)
    {
        var objectId = new ObjectId(correctBytes);
        var expectedString = ObjectIdTestsUtils.GetStringN(correctBytes);

        var actualString = objectId.ToString(string.Empty);

        Assert.That(actualString, Is.EqualTo(expectedString));
    }

    [TestCaseSource(typeof(ObjectIdTestData), nameof(ObjectIdTestData.CorrectObjectIdBytesArrays))]
    public void ToStringIncorrectFormat(byte[] correctBytes)
    {
        var objectId = new ObjectId(correctBytes);

        Assert.Throws<FormatException>(
            () =>
            {
                var _ = objectId.ToString("È");
            }
        );
    }

    [TestCaseSource(typeof(ObjectIdTestData), nameof(ObjectIdTestData.CorrectObjectIdBytesArrays))]
    public void ToStringTooLongFormat(byte[] correctBytes)
    {
        var objectId = new ObjectId(correctBytes);

        Assert.Throws<FormatException>(
            () =>
            {
                var _ = objectId.ToString("NN");
            }
        );
    }

    [TestCaseSource(typeof(ObjectIdTestData), nameof(ObjectIdTestData.CorrectObjectIdBytesArrays))]
    public void ToStringN(byte[] correctBytes)
    {
        var objectId = new ObjectId(correctBytes);
        var expectedString = ObjectIdTestsUtils.GetStringN(correctBytes);

        var actualString = objectId.ToString("N");

        Assert.That(actualString, Is.EqualTo(expectedString));
    }
}
