using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Sigin.ObjectId;

/// <summary>
///     Converter that used to convert between <see cref="ObjectId" /> structure and another data types.
/// </summary>
public class ObjectIdTypeConverter : TypeConverter
{
    private static readonly ConstructorInfo ObjectIdStringCtor = typeof(ObjectId)
        .GetTypeInfo()
        .DeclaredConstructors
        .Single(
            static x =>
            {
                var parameters = x.GetParameters();
                return parameters.Length == 1 && parameters[0].ParameterType == typeof(string);
            }
        );

    /// <inheritdoc />
    public override bool CanConvertFrom(
        ITypeDescriptorContext? context,
        Type sourceType
        )
    {
        return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
    }

    /// <inheritdoc />
    public override bool CanConvertTo(
        ITypeDescriptorContext? context,
        Type? destinationType
        )
    {
        return destinationType == typeof(InstanceDescriptor) || base.CanConvertTo(
            context,
            destinationType
#if NETSTANDARD2_0 || NETSTANDARD2_1 || NETCOREAPP3_1 || NET5_0
            !
#endif
        );
    }

    /// <inheritdoc />
    public override object? ConvertFrom(
        ITypeDescriptorContext? context,
        CultureInfo? culture,
        object value
        )
    {
        return value switch
        {
            string text => new ObjectId(text),
            InstanceDescriptor descriptor when descriptor.MemberInfo == ObjectIdStringCtor => descriptor.Invoke(),
            InstanceDescriptor => throw GetConvertFromException(value),
#if NETSTANDARD2_0 || NETSTANDARD2_1 || NETCOREAPP3_1 || NET5_0
            _ => base.ConvertFrom(context!, culture!, value)
#else
            _ => base.ConvertFrom(context, culture, value)
#endif
        };
    }

    /// <inheritdoc />
    public override object? ConvertTo(
        ITypeDescriptorContext? context,
        CultureInfo? culture,
        object? value,
        Type destinationType
        )
    {
        return value switch
        {
            ObjectId objectIdValue when destinationType == typeof(string) => objectIdValue.ToString("N", formatProvider: null),
            ObjectId objectIdValue when destinationType == typeof(InstanceDescriptor) => new InstanceDescriptor(
                ObjectIdStringCtor,
                new object[] {objectIdValue.ToString("N", formatProvider: null)}
            ),
            _ => base.ConvertTo(context, culture, value, destinationType)
#if NETSTANDARD2_0 || NETSTANDARD2_1 || NETCOREAPP3_1 || NET5_0
                !
#endif
        };
    }
}
