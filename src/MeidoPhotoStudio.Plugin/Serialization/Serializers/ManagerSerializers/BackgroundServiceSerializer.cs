using System.IO;
using System.Text.RegularExpressions;

using MeidoPhotoStudio.Database.Background;
using MeidoPhotoStudio.Plugin.Core.Background;

namespace MeidoPhotoStudio.Plugin;

public class BackgroundServiceSerializer : Serializer<BackgroundService>
{
    public const string Header = "ENVIRONMENT";

    private const short Version = 2;

    private static readonly Regex GuidRegEx =
        new(@"^[a-f0-9]{8}(\-[a-f0-9]{4}){3}\-[a-f0-9]{12}$", RegexOptions.IgnoreCase);

    public override void Serialize(BackgroundService backgroundService, BinaryWriter writer)
    {
        writer.Write(Header);
        writer.WriteVersion(Version);

        Serialization.GetSimple<BackgroundModel>().Serialize(backgroundService.CurrentBackground, writer);

        var backgroundTransform = backgroundService.BackgroundTransform;
        var transformDto = backgroundTransform ? new TransformDTO(backgroundTransform) : new TransformDTO();

        Serialization.GetSimple<TransformDTO>().Serialize(transformDto, writer);

        writer.Write(backgroundService.BackgroundColour);
    }

    public override void Deserialize(BackgroundService backgroundService, BinaryReader reader, SceneMetadata metadata)
    {
        var version = reader.ReadVersion();

        BackgroundModel backgroundModel;

        if (version is 1)
        {
            var assetName = reader.ReadString();
            var isCreativeBg = IsGuidString(assetName);

            // TODO: This does not account for CM3D2 backgrounds. MPS still functions but the data would be inaccurate
            // if for example the UI needed to be updated.
            backgroundModel = new(isCreativeBg ? BackgroundCategory.MyRoomCustom : BackgroundCategory.COM3D2, assetName);
        }
        else
        {
            backgroundModel = Serialization.GetSimple<BackgroundModel>().Deserialize(reader, metadata);
        }

        var backgroundTransformDto = Serialization.GetSimple<TransformDTO>().Deserialize(reader, metadata);

        backgroundService.ChangeBackground(backgroundModel);

        var backgroundTransform = backgroundService.BackgroundTransform;

        if (backgroundTransform)
        {
            backgroundTransform.SetPositionAndRotation(
                backgroundTransformDto.Position, backgroundTransformDto.Rotation);
            backgroundTransform.localScale = backgroundTransformDto.LocalScale;
        }

        if (version >= 2)
            backgroundService.BackgroundColour = reader.ReadColour();
    }

    private static bool IsGuidString(string guid) =>
        !string.IsNullOrEmpty(guid) && guid.Length is 36 && GuidRegEx.IsMatch(guid);
}
