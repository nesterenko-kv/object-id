using System.ComponentModel.Design.Serialization;
using System.Diagnostics;

namespace Sigin.ObjectId.Tests;

public class ObjectIdTypeConverterTests
{
    [TestCase(typeof(string))]
    [TestCase(typeof(InstanceDescriptor))]
    public void CanConvertToCorrect(Type type)
    {
        var converter = new ObjectIdTypeConverter();
        Assert.That(converter.CanConvertTo(type), Is.True);
    }

    [Test]
    public void CanConvertFromCorrect()
    {
        var converter = new ObjectIdTypeConverter();
        Assert.That(converter.CanConvertFrom(typeof(string)), Is.True);
    }

    [Test]
    public void CanConvertFromInt32()
    {
        var converter = new ObjectIdTypeConverter();
        Assert.That(converter.CanConvertFrom(typeof(int)), Is.False);
    }

    [Test]
    public void ConvertNotObjectIdToStringWillCallOverrideToString()
    {
        var expectedValue = "133742";
        var notObjectId = new NotObjectId(133742);
        var converter = new ObjectIdTypeConverter();

        var actualValue = converter.ConvertTo(notObjectId, typeof(string));

        Assert.Multiple(
            () =>
            {
                Assert.That(actualValue, Is.Not.Null);
                Assert.That(actualValue, Is.InstanceOf<string>());
                Assert.That((string?) actualValue, Is.EqualTo(expectedValue));
                Assert.That(notObjectId.ToStringCalls, Is.EqualTo(1));
            }
        );
    }

    [Test]
    public void ConvertToString()
    {
        var expectedValue = "28d2b480b9e743f48ee32ecf";
        var objectId = new ObjectId("28d2b480b9e743f48ee32ecf");
        var converter = new ObjectIdTypeConverter();

        var actualValue = converter.ConvertTo(objectId, typeof(string));

        Assert.Multiple(
            () =>
            {
                Assert.That(actualValue, Is.Not.Null);
                Assert.That(actualValue, Is.InstanceOf<string>());
                Assert.That((string?) actualValue, Is.EqualTo(expectedValue));
            }
        );
    }

    [Test]
    public void ConvertToInstanceDescriptor()
    {
        var objectIdCtor = typeof(ObjectId).GetConstructor(new[] {typeof(string)});
        var expectedValue = new InstanceDescriptor(objectIdCtor, new object[] {"ee753afdd98a45678de9740d"});
        var objectId = new ObjectId("ee753afdd98a45678de9740d");
        var converter = new ObjectIdTypeConverter();

        var actualValue = converter.ConvertTo(objectId, typeof(InstanceDescriptor));

        var actualDescriptor = (InstanceDescriptor?) actualValue;
        Assert.Multiple(
            () =>
            {
                Assert.That(actualValue, Is.Not.Null);
                Assert.That(actualValue, Is.InstanceOf<InstanceDescriptor>());
                Assert.That(actualDescriptor?.MemberInfo, Is.EqualTo(expectedValue.MemberInfo));
                Assert.That(actualDescriptor?.IsComplete, Is.EqualTo(expectedValue.IsComplete));
                Assert.That(actualDescriptor?.Arguments, Is.EqualTo(expectedValue.Arguments));
            }
        );
    }

    [Test]
    public void ConvertToInt32()
    {
        var objectId = new ObjectId("28d2b480b9e743f48ee32ecf");
        var converter = new ObjectIdTypeConverter();

        Assert.Throws<NotSupportedException>(
            () =>
            {
                var _ = converter.ConvertTo(objectId, typeof(int));
            }
        );
    }

    [Test]
    public void ConvertFromString()
    {
        var expectedValue = new ObjectId("28d2b480b9e743f48ee32ecf");
        var converter = new ObjectIdTypeConverter();

        var actualValue = converter.ConvertFrom("28d2b480b9e743f48ee32ecf");

        Assert.Multiple(
            () =>
            {
                Assert.That(actualValue, Is.Not.Null);
                Assert.That(actualValue, Is.InstanceOf<ObjectId>());
                Assert.That((ObjectId) actualValue!, Is.EqualTo(expectedValue));
            }
        );
    }

    [Test]
    public void ConvertFromValidInstanceDescriptor()
    {
        var expectedValue = new ObjectId("b28d9df8fd78429f89c7c669");
        var converter = new ObjectIdTypeConverter();
        var objectIdCtor = typeof(ObjectId).GetConstructor(new[] {typeof(string)});
        var descriptor = new InstanceDescriptor(objectIdCtor, new object[] {"b28d9df8fd78429f89c7c669"});

        var actualValue = converter.ConvertFrom(descriptor);

        Assert.Multiple(
            () =>
            {
                Assert.That(actualValue, Is.Not.Null);
                Assert.That(actualValue, Is.InstanceOf<ObjectId>());
                Assert.That((ObjectId) actualValue!, Is.EqualTo(expectedValue));
            }
        );
    }

    [Test]
    public void ConvertFromInvalidInstanceDescriptor()
    {
        var converter = new ObjectIdTypeConverter();
        var guidCtor = typeof(Guid).GetConstructor(new[] {typeof(string)});
        var descriptor = new InstanceDescriptor(guidCtor, new object[] {"b28d9df8fd78429f89c7c669"});

        Assert.Throws<NotSupportedException>(
            () =>
            {
                var _ = converter.ConvertFrom(descriptor);
            }
        );
    }

    [Test]
    public void ConvertFromInt32()
    {
        var converter = new ObjectIdTypeConverter();

        Assert.Throws<NotSupportedException>(
            () =>
            {
                var _ = converter.ConvertFrom(42);
            }
        );
    }

    private class NotObjectId
    {
        public NotObjectId(int id)
        {
            Id = id;
        }

        private int Id { get; }

        public int ToStringCalls { get; private set; }

        [DebuggerStepThrough]
        public override string ToString()
        {
            ToStringCalls++;
            return Id.ToString("D");
        }
    }
}
