namespace MeidoPhotoStudio.Plugin.Framework.Extensions;

public static class BinaryReaderWriterExtensions
{
    public static string ReadNullableString(this BinaryReader binaryReader) =>
        binaryReader.ReadBoolean()
            ? binaryReader.ReadString()
            : null;

    public static void WriteNullableString(this BinaryWriter binaryWriter, string str)
    {
        binaryWriter.Write(str is not null);

        if (str is not null)
            binaryWriter.Write(str);
    }

    public static void Write(this BinaryWriter binaryWriter, Vector3 vector3)
    {
        binaryWriter.Write(vector3.x);
        binaryWriter.Write(vector3.y);
        binaryWriter.Write(vector3.z);
    }

    public static void WriteVector3(this BinaryWriter binaryWriter, Vector3 vector3)
    {
        binaryWriter.Write(vector3.x);
        binaryWriter.Write(vector3.y);
        binaryWriter.Write(vector3.z);
    }

    public static Vector2 ReadVector2(this BinaryReader binaryReader) =>
        new(binaryReader.ReadSingle(), binaryReader.ReadSingle());

    public static Vector3 ReadVector3(this BinaryReader binaryReader) =>
        new(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());

    public static Vector4 ReadVector4(this BinaryReader binaryReader) =>
        new(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());

    public static void Write(this BinaryWriter binaryWriter, Quaternion quaternion)
    {
        binaryWriter.Write(quaternion.x);
        binaryWriter.Write(quaternion.y);
        binaryWriter.Write(quaternion.z);
        binaryWriter.Write(quaternion.w);
    }

    public static void WriteQuaternion(this BinaryWriter binaryWriter, Quaternion quaternion)
    {
        binaryWriter.Write(quaternion.x);
        binaryWriter.Write(quaternion.y);
        binaryWriter.Write(quaternion.z);
        binaryWriter.Write(quaternion.w);
    }

    public static Quaternion ReadQuaternion(this BinaryReader binaryReader) =>
        new(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());

    public static void Write(this BinaryWriter binaryWriter, Color colour)
    {
        binaryWriter.Write(colour.r);
        binaryWriter.Write(colour.g);
        binaryWriter.Write(colour.b);
        binaryWriter.Write(colour.a);
    }

    public static void WriteColour(this BinaryWriter binaryWriter, Color colour)
    {
        binaryWriter.Write(colour.r);
        binaryWriter.Write(colour.g);
        binaryWriter.Write(colour.b);
        binaryWriter.Write(colour.a);
    }

    public static Color ReadColour(this BinaryReader binaryReader) =>
        new(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());

    public static Matrix4x4 ReadMatrix4x4(this BinaryReader binaryReader)
    {
        Matrix4x4 matrix = default;

        for (var i = 0; i < 16; i++)
            matrix[i] = binaryReader.ReadSingle();

        return matrix;
    }
}
