namespace MeidoPhotoStudio.Converter.Utility;

internal static class PngUtility
{
    private static readonly byte[] PngHeader = { 137, 80, 78, 71, 13, 10, 26, 10 };
    private static readonly byte[] PngEnd = System.Text.Encoding.ASCII.GetBytes("IEND");

    public static byte[]? ExtractPng(Stream stream)
    {
        var memoryStream = new MemoryStream();
        var headerBuffer = new byte[PngHeader.Length];

        stream.Read(headerBuffer, 0, headerBuffer.Length);

        if (!headerBuffer.SequenceEqual(PngHeader))
            return null;

        memoryStream.Write(headerBuffer, 0, headerBuffer.Length);

        var fourByteBuffer = new byte[4];
        var chunkBuffer = new byte[1024];

        try
        {
            do
            {
                // chunk length
                var read = stream.Read(fourByteBuffer, 0, 4);

                memoryStream.Write(fourByteBuffer, 0, read);

                if (BitConverter.IsLittleEndian)
                    Array.Reverse(fourByteBuffer);

                var length = BitConverter.ToUInt32(fourByteBuffer, 0);

                // chunk type
                read = stream.Read(fourByteBuffer, 0, 4);
                memoryStream.Write(fourByteBuffer, 0, read);

                if (chunkBuffer.Length < length + 4L)
                    chunkBuffer = new byte[length + 4L];

                // chunk data + CRC
                read = stream.Read(chunkBuffer, 0, (int)(length + 4L));
                memoryStream.Write(chunkBuffer, 0, read);
            }
            while (!fourByteBuffer.SequenceEqual(PngEnd));
        }
        catch
        {
            return null;
        }

        return memoryStream.ToArray();
    }
}
