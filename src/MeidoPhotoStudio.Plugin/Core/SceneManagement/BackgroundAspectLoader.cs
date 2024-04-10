using System.Text.RegularExpressions;

using MeidoPhotoStudio.Plugin.Core.Background;
using MeidoPhotoStudio.Plugin.Core.Schema.Background;

namespace MeidoPhotoStudio.Plugin.Core.SceneManagement;

public class BackgroundAspectLoader(BackgroundService backgroundService) : ISceneAspectLoader<BackgroundSchema>
{
    private readonly BackgroundService backgroundService = backgroundService
        ?? throw new ArgumentNullException(nameof(backgroundService));

    public void Load(BackgroundSchema backgroundSchema, LoadOptions loadOptions)
    {
        if (!loadOptions.Background)
            return;

        if (backgroundSchema.Version is 1)
        {
            if (IsGuidString(backgroundSchema.BackgroundName))
                GameMain.Instance.BgMgr.ChangeBgMyRoom(backgroundSchema.BackgroundName);
            else
                GameMain.Instance.BgMgr.ChangeBg(backgroundSchema.BackgroundName);
        }
        else if (backgroundSchema.Version >= 2)
        {
            var modelSchema = backgroundSchema.Background;

            backgroundService.ChangeBackground(new(modelSchema.Category, modelSchema.AssetName, modelSchema.Name));
        }

        if (backgroundService.BackgroundTransform)
        {
            backgroundService.BackgroundTransform.SetPositionAndRotation(
                backgroundSchema.Transform.Position, backgroundSchema.Transform.Rotation);
            backgroundService.BackgroundTransform.localScale = backgroundSchema.Transform.LocalScale;
        }

        if (backgroundSchema.Version >= 2)
            backgroundService.BackgroundColour = backgroundSchema.Colour;
    }

    private static bool IsGuidString(string guid)
    {
        var guidRegEx =
            new Regex(@"^[a-f0-9]{8}(\-[a-f0-9]{4}){3}\-[a-f0-9]{12}$", RegexOptions.IgnoreCase);

        return !string.IsNullOrEmpty(guid) && guid.Length is 36 && guidRegEx.IsMatch(guid);
    }
}
