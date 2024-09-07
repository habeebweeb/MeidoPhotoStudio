using MeidoPhotoStudio.Plugin.Core.Database.Background;

namespace MeidoPhotoStudio.Plugin.Core.Database.Props;

public class BackgroundPropModel(BackgroundModel backgroundModel) : IPropModel
{
    private readonly BackgroundModel backgroundModel = backgroundModel;

    public string Name =>
        backgroundModel.Name;

    public string IconFilename =>
        string.Empty;

    public string ID =>
        backgroundModel.ID;

    public BackgroundCategory Category =>
        backgroundModel.Category;

    public string AssetName =>
        backgroundModel.AssetName;
}
