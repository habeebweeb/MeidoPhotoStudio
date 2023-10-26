using System.IO;
using System.Text.RegularExpressions;

using MeidoPhotoStudio.Database.Background;
using MeidoPhotoStudio.Plugin.Core.Background;

namespace MeidoPhotoStudio.Plugin;

public class BackgroundServiceSerializer : Serializer<BackgroundService>
{
    public const string Header = "ENVIRONMENT";

    private const short Version = 1;

    private static readonly Regex GuidRegEx =
        new(@"^[a-f0-9]{8}(\-[a-f0-9]{4}){3}\-[a-f0-9]{12}$", RegexOptions.IgnoreCase);

    public override void Serialize(BackgroundService backgroundService, BinaryWriter writer)
    {
        writer.Write(Header);
        writer.WriteVersion(Version);

        writer.Write(backgroundService.CurrentBackground.AssetName);

        var backgroundTransform = backgroundService.BackgroundTransform;
        var transformDto = backgroundTransform ? new TransformDTO(backgroundTransform) : new TransformDTO();

        Serialization.GetSimple<TransformDTO>().Serialize(transformDto, writer);
    }

    public override void Deserialize(BackgroundService backgroundService, BinaryReader reader, SceneMetadata metadata)
    {
        var version = reader.ReadVersion();

        var assetName = reader.ReadString();
        var isCreativeBg = IsGuidString(assetName);

        var backgroundModel = new BackgroundModel(
            isCreativeBg
                ? BackgroundCategory.MyRoomCustom
                : BackgroundCategory.COM3D2, assetName);

        var backgroundTransformDto = Serialization.GetSimple<TransformDTO>().Deserialize(reader, metadata);

        backgroundService.ChangeBackground(backgroundModel);

        var backgroundTransform = backgroundService.BackgroundTransform;

        if (backgroundTransform)
        {
            backgroundTransform.SetPositionAndRotation(
                backgroundTransformDto.Position, backgroundTransformDto.Rotation);
            backgroundTransform.localScale = backgroundTransformDto.LocalScale;
        }
    }

    private static bool IsGuidString(string guid) =>
        !string.IsNullOrEmpty(guid) && guid.Length is 36 && GuidRegEx.IsMatch(guid);
}
