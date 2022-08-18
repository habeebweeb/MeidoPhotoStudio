using System;
using System.IO;
using System.Text;

using Ionic.Zlib;
using MeidoPhotoStudio.Plugin;

namespace MeidoPhotoStudio.Converter;

public static class MPSSceneSerializer
{
    private const string NoThumbBase64 =
        "iVBORw0KGgoAAAANSUhEUgAAADIAAAAyCAIAAACRXR/mAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7D"
        + "AcdvqGQAAAFOSURBVFhH3dJbjoMwEETRLIRP9r+zrCGpqJABY+x+2Ua5ys9EcteJNK/3sj7ws7E+j2ln8Q9+O7eE2Vjpq4kdJTsLTZRl"
        + "jBMLTZFdDTkLDZYVAQUWGia7Wy+z0ABZZfqWhbrK6rs1Fuoka442WChcJllss1CgTDgnYqEQmXxLykJOmWpIwUJmmXZFx0IGmWFCzUKq"
        + "J7b7FhYSvjIfN7JQ86Hnsp2FKm+dZ10sVHzuv+lloexCyMEAFkpHoq7FsBDuBJ76a1Y6EnXtT//li8/9N12sylvnWTur+dBz2cgSvjIf"
        + "t7BUT2z31azePwOpWQYT064oWGYTUw1JWU4Tk2+JWCEmJpxrswJNTLLYYIWbWHO0xupkYvXdW1ZXE6tMl1kDTOxuvcAaZmJFQM4abGJX"
        + "w4k1xcQyxs6aaGJHycaabmIJ82M9xMTo2VjP+izrF8NPHwq3SYqeAAAAAElFTkSuQmCC";

    private static byte[]? noThumb;

    public static byte[] NoThumb =>
        noThumb ??= Convert.FromBase64String(NoThumbBase64);

    public static void SaveToFile(string filename, SceneMetadata metadata, byte[] rawSceneData, string? thumbnail)
    {
        var rawThumbnail = string.IsNullOrEmpty(thumbnail) ? NoThumb : Convert.FromBase64String(thumbnail);

        SaveToFile(filename, metadata, rawSceneData, rawThumbnail);
    }

    public static void SaveToFile(string filename, SceneMetadata metadata, byte[] rawSceneData, byte[] thumbnail)
    {
        if (!string.Equals(Path.GetExtension(filename), ".png", StringComparison.OrdinalIgnoreCase))
            filename += ".png";

        using var fileStream = File.Create(filename);

        fileStream.Write(thumbnail, 0, thumbnail.Length);

        using var headerWriter = new BinaryWriter(fileStream, Encoding.UTF8);

        headerWriter.Write(MeidoPhotoStudio.Plugin.MeidoPhotoStudio.SceneHeader);

        metadata.WriteMetadata(headerWriter);

        using var compressionStream = new DeflateStream(fileStream, CompressionMode.Compress);

        compressionStream.Write(rawSceneData, 0, rawSceneData.Length);

        compressionStream.Close();
    }

    public static string FormatDate(DateTime date) =>
        date.ToString("yyyyMMddHHmmss");
}
