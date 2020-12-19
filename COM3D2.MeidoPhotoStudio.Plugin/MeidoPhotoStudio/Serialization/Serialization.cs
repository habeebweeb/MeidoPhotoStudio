using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace COM3D2.MeidoPhotoStudio.Plugin
{
    public static class Serialization
    {
        private static readonly Dictionary<Type, ISerializer> serializers = new();

        private static readonly Dictionary<Type, ISimpleSerializer> simpleSerializers = new();

        static Serialization()
        {
            var types = (from t in typeof(MeidoPhotoStudio).Assembly.GetTypes()
                let baseType = t.BaseType
                where !t.IsAbstract && !t.IsInterface && baseType != null && baseType.IsGenericType
                select new { type = t, baseType }).ToArray();

            var concreteSerializers = from t in types
                where t.baseType.GetGenericTypeDefinition() == typeof(Serializer<>)
                select new { t.type, arg = t.baseType.GetGenericArguments().First() };

            foreach (var serializer in concreteSerializers)
                serializers[serializer.arg] = (ISerializer) Activator.CreateInstance(serializer.type);

            var concreteSimpleSerializers = from t in types
                where t.baseType.GetGenericTypeDefinition() == typeof(SimpleSerializer<>)
                select new { t.type, arg = t.baseType.GetGenericArguments().First() };

            foreach (var serializer in concreteSimpleSerializers)
                simpleSerializers[serializer.arg] = (ISimpleSerializer) Activator.CreateInstance(serializer.type);
        }

        public static Serializer<T> Get<T>() => serializers[typeof(T)] as Serializer<T>;

        public static ISerializer Get(Type type) => serializers[type];

        public static SimpleSerializer<T> GetSimple<T>() => simpleSerializers[typeof(T)] as SimpleSerializer<T>;

        public static ISimpleSerializer GetSimple(Type type) => simpleSerializers[type];

        public static short ReadVersion(this BinaryReader reader) => reader.ReadInt16();

        public static void WriteVersion(this BinaryWriter writer, short version) => writer.Write(version);
    }
}
