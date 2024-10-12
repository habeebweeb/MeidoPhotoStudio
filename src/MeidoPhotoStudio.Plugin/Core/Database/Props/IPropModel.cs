namespace MeidoPhotoStudio.Plugin.Core.Database.Props;

public interface IPropModel : IEquatable<IPropModel>
{
    string Name { get; }

    string IconFilename { get; }
}
