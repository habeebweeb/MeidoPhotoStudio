using MeidoPhotoStudio.Plugin.Core.Database.Props.Menu;
using MeidoPhotoStudio.Plugin.Framework.Extensions;

namespace MeidoPhotoStudio.Plugin.Framework.Menu;

public class MenuFileCacheSerializer : IMenuFileCacheSerializer
{
    private const int CacheVersion = 3;

    private readonly string cacheDirectory;

    public MenuFileCacheSerializer(string cacheDirectory)
    {
        if (string.IsNullOrEmpty(cacheDirectory))
            throw new ArgumentException($"'{nameof(cacheDirectory)}' cannot be null or empty.", nameof(cacheDirectory));

        this.cacheDirectory = cacheDirectory;
    }

    private string CacheFilePath =>
        Path.Combine(cacheDirectory, "cache.dat");

    public Dictionary<string, MenuFilePropModel> Deserialize()
    {
        try
        {
            using var fileStream = new FileStream(CacheFilePath, FileMode.Open, FileAccess.Read);
            using var binaryReader = new BinaryReader(fileStream, Encoding.UTF8);

            return DeserializeCache(binaryReader);
        }
        catch
        {
            return new(StringComparer.OrdinalIgnoreCase);
        }
    }

    public void Serialize(Dictionary<string, MenuFilePropModel> menuFileCache)
    {
        using var fileStream = new FileStream(CacheFilePath, FileMode.OpenOrCreate, FileAccess.Write);
        using var binaryWriter = new BinaryWriter(fileStream, Encoding.UTF8);

        SerializeCache(binaryWriter, menuFileCache);
    }

    private static Dictionary<string, MenuFilePropModel> DeserializeCache(BinaryReader reader)
    {
        var menuFiles = new Dictionary<string, MenuFilePropModel>(StringComparer.OrdinalIgnoreCase);

        if (reader.ReadInt32() != CacheVersion)
            return menuFiles;

        var menuFileCount = reader.ReadInt32();

        for (var i = 0; i < menuFileCount; i++)
        {
            var menuFile = DeserializeMenuFile(reader);

            menuFiles[menuFile.Filename] = menuFile;
        }

        return menuFiles;

        static MenuFilePropModel DeserializeMenuFile(BinaryReader reader)
        {
            var menuFilename = reader.ReadString();
            var gameMenu = reader.ReadBoolean();

            var builder = new MenuFilePropModel.Builder(menuFilename, gameMenu)
                .WithName(reader.ReadNullableString())
                .WithMpn(SafeMpn.GetValue(reader.ReadString()))
                .WithIconFilename(reader.ReadNullableString())
                .WithPriority(reader.ReadSingle())
                .WithModelFilename(reader.ReadNullableString());

            var materialChangeCount = reader.ReadInt32();

            for (var i = 0; i < materialChangeCount; i++)
                builder.AddMaterialChange(new()
                {
                    MaterialIndex = reader.ReadInt32(),
                    MaterialFilename = reader.ReadNullableString(),
                });

            var modelAnimationCount = reader.ReadInt32();

            for (var i = 0; i < modelAnimationCount; i++)
                builder.AddModelAnime(new()
                {
                    Slot = (SlotID)reader.ReadInt32(),
                    AnimationName = reader.ReadNullableString(),
                    Loop = reader.ReadBoolean(),
                });

            var modelMaterialAnimationCount = reader.ReadInt32();

            for (var i = 0; i < modelMaterialAnimationCount; i++)
                builder.AddModelMaterialAnimation(new()
                {
                    Slot = (SlotID)reader.ReadInt32(),
                    MaterialIndex = reader.ReadInt32(),
                });

            var materialTextureChangeCount = reader.ReadInt32();

            for (var i = 0; i < materialTextureChangeCount; i++)
                builder.AddMaterialTextureChange(new()
                {
                    MaterialIndex = reader.ReadInt32(),
                    MaterialPropertyName = reader.ReadNullableString(),
                    TextureFilename = reader.ReadNullableString(),
                });

            return builder.Build();
        }
    }

    private static void SerializeCache(BinaryWriter writer, Dictionary<string, MenuFilePropModel> menuFileCache)
    {
        writer.Write(CacheVersion);
        writer.Write(menuFileCache.Count);

        foreach (var (_, menuFile) in menuFileCache)
            SerializeMenuFile(writer, menuFile);

        static void SerializeMenuFile(BinaryWriter writer, MenuFilePropModel menuFile)
        {
            writer.Write(menuFile.Filename);
            writer.Write(menuFile.GameMenu);
            writer.WriteNullableString(menuFile.Name);
            writer.Write(menuFile.CategoryMpn.ToString());
            writer.WriteNullableString(menuFile.IconFilename);
            writer.Write(menuFile.Priority);
            writer.WriteNullableString(menuFile.ModelFilename);

            writer.Write(menuFile.MaterialChanges.Count());

            foreach (var material in menuFile.MaterialChanges)
            {
                writer.Write(material.MaterialIndex);
                writer.WriteNullableString(material.MaterialFilename);
            }

            writer.Write(menuFile.ModelAnimations.Count());

            foreach (var modelAnimation in menuFile.ModelAnimations)
            {
                writer.Write((int)modelAnimation.Slot);
                writer.WriteNullableString(modelAnimation.AnimationName);
                writer.Write(modelAnimation.Loop);
            }

            writer.Write(menuFile.ModelMaterialAnimations.Count());

            foreach (var modelMaterialAnimation in menuFile.ModelMaterialAnimations)
            {
                writer.Write((int)modelMaterialAnimation.Slot);
                writer.Write(modelMaterialAnimation.MaterialIndex);
            }

            writer.Write(menuFile.MaterialTextureChanges.Count());

            foreach (var materialTextureChange in menuFile.MaterialTextureChanges)
            {
                writer.Write(materialTextureChange.MaterialIndex);
                writer.WriteNullableString(materialTextureChange.MaterialPropertyName);
                writer.WriteNullableString(materialTextureChange.TextureFilename);
            }
        }
    }
}
