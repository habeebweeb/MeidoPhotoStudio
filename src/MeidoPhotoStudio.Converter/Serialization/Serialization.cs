using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEngine;

namespace MeidoPhotoStudio.Converter.Serialization;

public static class Serialization
{
    private static readonly Dictionary<Type, ISerializer> Serializers;

    private static readonly Dictionary<Type, ISimpleSerializer> SimpleSerializers;

    static Serialization()
    {
        var types =
            (from t in typeof(Plugin).Assembly.GetTypes()
             let baseType = t.BaseType
             where !t.IsAbstract && !t.IsInterface && baseType?.IsGenericType == true
             select new { type = t, baseType }).ToArray();

        Serializers = types.Where(t => t.baseType.GetGenericTypeDefinition() == typeof(Serializer<>))
            .Select(t => new { t.type, arg = t.baseType.GetGenericArguments()[0] })
            .ToDictionary(x => x.arg, x => (ISerializer)Activator.CreateInstance(x.type));

        SimpleSerializers = types.Where(t => t.baseType.GetGenericTypeDefinition() == typeof(SimpleSerializer<>))
            .Select(t => new { t.type, arg = t.baseType.GetGenericArguments()[0] })
            .ToDictionary(x => x.arg, x => (ISimpleSerializer)Activator.CreateInstance(x.type));
    }

    public static Serializer<T> Get<T>() =>
        (Serializer<T>)Serializers[typeof(T)];

    public static ISerializer Get(Type type) =>
        Serializers[type];

    public static SimpleSerializer<T> GetSimple<T>()
        where T : notnull =>
        (SimpleSerializer<T>)SimpleSerializers[typeof(T)];

    public static ISimpleSerializer GetSimple(Type type) =>
        SimpleSerializers[type];

    public static short ReadVersion(this BinaryReader reader) =>
        reader.ReadInt16();

    public static void WriteVersion(this BinaryWriter writer, short version) =>
        writer.Write(version);

    public static Vector3 ReadVector3(this BinaryReader reader) =>
        new(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

    public static Quaternion ReadQuaternion(this BinaryReader reader) =>
        new(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

    public static Color ReadColour(this BinaryReader reader) =>
        new(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
}
