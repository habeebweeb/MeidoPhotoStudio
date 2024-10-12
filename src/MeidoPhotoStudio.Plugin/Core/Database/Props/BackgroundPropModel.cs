using MeidoPhotoStudio.Plugin.Core.Database.Background;

namespace MeidoPhotoStudio.Plugin.Core.Database.Props;

public class BackgroundPropModel(BackgroundModel backgroundModel) : IEquatable<BackgroundPropModel>, IPropModel
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

    public bool Equals(IPropModel other) =>
        other is BackgroundPropModel model && Equals(model);

    public bool Equals(BackgroundPropModel other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        return other.backgroundModel.Equals(backgroundModel);
    }

    public override int GetHashCode() =>
        backgroundModel.GetHashCode();
}
