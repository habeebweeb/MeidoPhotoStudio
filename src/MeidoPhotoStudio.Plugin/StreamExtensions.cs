using System.IO;

using Ionic.Zlib;

namespace MeidoPhotoStudio.Plugin;

public static class StreamExtensions
{
    public static void CopyTo(this Stream stream, Stream outStream)
    {
        var buf = new byte[1024 * 32];

        int length;

        while ((length = stream.Read(buf, 0, buf.Length)) > 0)
            outStream.Write(buf, 0, length);
    }

    public static MemoryStream Decompress(this MemoryStream stream)
    {
        var dataMemoryStream = new MemoryStream();

        using var compressionStream = new DeflateStream(stream, CompressionMode.Decompress, true);

        compressionStream.CopyTo(dataMemoryStream);
        compressionStream.Flush();

        dataMemoryStream.Position = 0L;

        return dataMemoryStream;
    }

    public static DeflateStream GetCompressionStream(this MemoryStream stream) =>
        new(stream, CompressionMode.Compress);
}
