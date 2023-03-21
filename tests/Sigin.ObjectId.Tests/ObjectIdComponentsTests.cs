using Sigin.ObjectId.Tests.Data;

namespace Sigin.ObjectId.Tests;

public class ObjectIdComponentsTests
{
    [Test]
    [TestCaseSource(typeof(ObjectIdTestData), nameof(ObjectIdTestData.CorrectObjectIdAndComponents))]
    public void ComponentsCorrect(long timestamp, long random, int increment, string objectId)
    {
        var result = ObjectId.ParseExact(objectId, "N");

        Assert.Multiple(
            () =>
            {
                Assert.That(result.Timestamp, Is.EqualTo(timestamp));
                Assert.That(result.Random, Is.EqualTo(random));
                Assert.That(result.Increment, Is.EqualTo(increment));
            }
        );
    }

    [Test]
    [TestCaseSource(typeof(ObjectIdTestData), nameof(ObjectIdTestData.CorrectTimestamps))]
    public void CreationTimeCorrect(long timestamp, DateTimeOffset expected)
    {
        var result = ObjectId.NewObjectId(timestamp, random: 0, increment: 0);

        Assert.Multiple(
            () =>
            {
                Assert.That(result.CreationTime, Is.EqualTo(expected));
            }
        );
    }
}
