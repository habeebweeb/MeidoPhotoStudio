using System.Text.RegularExpressions;

using MeidoPhotoStudio.Database.Background;
using MeidoPhotoStudio.Plugin.Core.Background;
using MeidoPhotoStudio.Plugin.Core.Schema.Background;

namespace MeidoPhotoStudio.Plugin.Core.SceneManagement;

public class BackgroundAspectLoader(BackgroundService backgroundService, BackgroundRepository backgroundRepository)
    : ISceneAspectLoader<BackgroundSchema>
{
    private readonly BackgroundService backgroundService = backgroundService
        ?? throw new ArgumentNullException(nameof(backgroundService));

    public void Load(BackgroundSchema backgroundSchema, LoadOptions loadOptions)
    {
        if (!loadOptions.Background)
            return;

        BackgroundModel backgroundModel = null;

        if (backgroundSchema.Version is 1)
            backgroundModel = GetModelFromID(backgroundSchema.BackgroundName);
        else if (backgroundSchema.Version >= 2)
            backgroundModel = GetModelFromSchema(backgroundSchema.Background);

        if (backgroundModel is not null)
            backgroundService.ChangeBackground(backgroundModel);

        if (backgroundService.BackgroundTransform)
        {
            backgroundService.BackgroundTransform.SetPositionAndRotation(
                backgroundSchema.Transform.Position, backgroundSchema.Transform.Rotation);
            backgroundService.BackgroundTransform.localScale = backgroundSchema.Transform.LocalScale;
        }

        if (backgroundSchema.Version >= 2)
        {
            backgroundService.BackgroundColour = backgroundSchema.Colour;
            backgroundService.BackgroundVisible = backgroundSchema.Visible;
        }

        BackgroundModel GetModelFromID(string backgroundName)
        {
            if (IsGuidString(backgroundName))
            {
                return backgroundRepository[BackgroundCategory.MyRoomCustom].FirstOrDefault(model =>
                    string.Equals(backgroundName, model.ID, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                IEnumerable<BackgroundModel> models = backgroundRepository[BackgroundCategory.COM3D2];

                if (backgroundRepository.ContainsCategory(BackgroundCategory.CM3D2))
                    models = models.Concat(backgroundRepository[BackgroundCategory.CM3D2]);

                return models.FirstOrDefault(model => string.Equals(backgroundName, model.ID, StringComparison.OrdinalIgnoreCase));
            }

            static bool IsGuidString(string guid)
            {
                var guidRegEx =
                    new Regex(@"^[a-f0-9]{8}(\-[a-f0-9]{4}){3}\-[a-f0-9]{12}$", RegexOptions.IgnoreCase);

                return !string.IsNullOrEmpty(guid) && guid.Length is 36 && guidRegEx.IsMatch(guid);
            }
        }

        BackgroundModel GetModelFromSchema(BackgroundModelSchema schema) =>
            backgroundRepository.ContainsCategory(schema.Category)
                ? backgroundRepository[schema.Category]
                    .FirstOrDefault(model => string.Equals(model.ID, schema.ID, StringComparison.OrdinalIgnoreCase))
                : null;
    }
}
