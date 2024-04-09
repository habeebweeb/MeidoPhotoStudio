namespace MeidoPhotoStudio.Plugin.Framework.Extensions;

public static class CRC32Extensions
{
    public static uint ComputeChecksum(this wf.CRC32 crc32, Stream inputStream)
    {
        var checksum = crc32.ComputeHash(inputStream);

        if (BitConverter.IsLittleEndian)
            Array.Reverse(checksum);

        return BitConverter.ToUInt32(checksum, 0);
    }
}
