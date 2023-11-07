using System.Reflection;

namespace MeidoPhotoStudio.Converter.Utility;

public static class Reflection
{
    private const BindingFlags ReflectionFlags =
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

    public static FieldInfo GetFieldInfo<T>(string field) =>
        typeof(T).GetField(field, ReflectionFlags);

    public static TValue? GetFieldValue<TType, TValue>(TType instance, string field)
    {
        var fieldInfo = GetFieldInfo<TType>(field);

        return fieldInfo is null || !fieldInfo.IsStatic && instance is null
            ? default
            : (TValue)fieldInfo.GetValue(instance);
    }

    public static void SetFieldValue<TType, TValue>(TType instance, string name, TValue value) =>
        GetFieldInfo<TType>(name).SetValue(instance, value);

    public static PropertyInfo GetPropertyInfo<T>(string field) =>
        typeof(T).GetProperty(field, ReflectionFlags);

    public static TValue? GetPropertyValue<TType, TValue>(TType instance, string property)
    {
        var propertyInfo = GetPropertyInfo<TType>(property);

        return propertyInfo is null
            ? default
            : (TValue)propertyInfo.GetValue(instance, null);
    }

    public static void SetPropertyValue<TType, TValue>(TType instance, string name, TValue value) =>
        GetPropertyInfo<TType>(name).SetValue(instance, value, null);
}
