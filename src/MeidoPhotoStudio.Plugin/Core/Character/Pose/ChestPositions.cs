namespace MeidoPhotoStudio.Plugin.Core.Character.Pose;

public readonly record struct ChestPositions(Vector3 Mune, Vector3 SubMune)
{
    public static ChestPositions operator +(ChestPositions a, ChestPositions b) =>
        new(a.Mune + b.Mune, a.SubMune + b.SubMune);

    public static ChestPositions operator -(ChestPositions a, ChestPositions b) =>
        new(a.Mune - b.Mune, a.SubMune - b.SubMune);
}
