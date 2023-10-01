using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MeidoPhotoStudio.Plugin;

/// <summary>Provides allocation free <see cref="IEqualityComparer{T}"/> for <see langword="enum"/>.</summary>
/// <typeparam name="TEnum">An <see langword="enum"/> type.</typeparam>
internal abstract class EnumEqualityComparer<TEnum> : IEqualityComparer<TEnum>
    where TEnum : Enum
{
    private static EnumEqualityComparer<TEnum> instance;

    private EnumEqualityComparer()
    {
    }

    public static EnumEqualityComparer<TEnum> Instance
    {
        get
        {
            if (instance is not null)
                return instance;

            var underlyingTypeCode = Type.GetTypeCode(Enum.GetUnderlyingType(typeof(TEnum)));

            instance = underlyingTypeCode switch
            {
                TypeCode.SByte => new SbyteEnumEqualityComparer(),
                TypeCode.Int16 => new Int16EnumEqualityComparer(),
                TypeCode.Int32 or TypeCode.UInt32 or TypeCode.Byte or TypeCode.UInt16 =>
                    new Int32EnumEqualityComparer(),
                TypeCode.Int64 or TypeCode.UInt64 => new Int64EnumEqualityComparer(),
                _ => throw new NotSupportedException(underlyingTypeCode.ToString()),
            };

            return instance;
        }
    }

    public abstract bool Equals(TEnum x, TEnum y);

    public abstract int GetHashCode(TEnum obj);

    private class TypeConverter<TType>
        where TType : struct
    {
        private readonly Func<TEnum, TType> converter;

        public TypeConverter()
        {
            var enumParameter = Expression.Parameter(typeof(TEnum), null);
            var convertParameter = Expression.ConvertChecked(enumParameter, typeof(TType));

            converter = Expression.Lambda<Func<TEnum, TType>>(convertParameter, enumParameter).Compile();
        }

        public TType Convert(TEnum value) =>
            converter.Invoke(value);
    }

    private sealed class Int16EnumEqualityComparer : EnumEqualityComparer<TEnum>
    {
        private static readonly TypeConverter<short> Converter = new();

        public override bool Equals(TEnum x, TEnum y) =>
            Converter.Convert(x) == Converter.Convert(y);

        public override int GetHashCode(TEnum obj) =>
            Converter.Convert(obj).GetHashCode();
    }

    private sealed class SbyteEnumEqualityComparer : EnumEqualityComparer<TEnum>
    {
        private static readonly TypeConverter<sbyte> Converter = new();

        public override bool Equals(TEnum x, TEnum y) =>
            Converter.Convert(x) == Converter.Convert(y);

        public override int GetHashCode(TEnum obj) =>
            Converter.Convert(obj).GetHashCode();
    }

    private sealed class Int32EnumEqualityComparer : EnumEqualityComparer<TEnum>
    {
        private static readonly TypeConverter<int> Converter = new();

        public override bool Equals(TEnum x, TEnum y) =>
            Converter.Convert(x) == Converter.Convert(y);

        public override int GetHashCode(TEnum obj) =>
            Converter.Convert(obj).GetHashCode();
    }

    private sealed class Int64EnumEqualityComparer : EnumEqualityComparer<TEnum>
    {
        private static readonly TypeConverter<long> Converter = new();

        public override bool Equals(TEnum x, TEnum y) =>
            Converter.Convert(x) == Converter.Convert(y);

        public override int GetHashCode(TEnum obj) =>
            Converter.Convert(obj).GetHashCode();
    }
}
