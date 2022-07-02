using System.IO;
using SevenZip.Compression.LZMA;

namespace MeidoPhotoStudio.Converter.Utility
{
    internal static class LZMA
    {
        public static MemoryStream Decompress(Stream inStream)
        {
            var outStream = new MemoryStream();

            var properties = new byte[5];

            if (inStream.Read(properties, 0, 5) != 5)
                throw new("input .lzma is too short");

            var decoder = new Decoder();

            decoder.SetDecoderProperties(properties);

            var outSize = 0L;

            for (var i = 0; i < 8; i++)
            {
                var v = inStream.ReadByte();

                if (v < 0)
                    throw new("Can't Read 1");

                outSize |= ((long)(byte)v) << (8 * i);
            }

            var compressedSize = inStream.Length - inStream.Position;

            decoder.Code(inStream, outStream, compressedSize, outSize, null);

            return outStream;
        }
    }
}
