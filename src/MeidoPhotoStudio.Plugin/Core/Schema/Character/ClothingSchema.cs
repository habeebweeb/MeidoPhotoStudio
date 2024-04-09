using MeidoPhotoStudio.Plugin.Core.Schema.Props;

namespace MeidoPhotoStudio.Plugin.Core.Schema.Character;

public class ClothingSchema(short version = ClothingSchema.SchemaVersion)
{
    public const short SchemaVersion = 2;

    public short Version { get; } = version;

    public bool MMConverted { get; init; }

    public bool BodyVisible { get; init; }

    public Dictionary<SlotID, bool> ClothingSet { get; init; }

    public bool CurlingFront { get; init; }

    public bool CurlingBack { get; init; }

    public bool PantsuShift { get; init; }

    public MenuFilePropModelSchema AttachedLowerAccessory { get; init; }

    public MenuFilePropModelSchema AttachedUpperAccessory { get; init; }

    public bool HairGravityEnabled { get; init; }

    public Vector3 HairGravityPosition { get; init; }

    public bool ClothingGravityEnabled { get; init; }

    public Vector3 ClothingGravityPosition { get; init; }

    public bool CustomFloorHeight { get; init; }

    public float FloorHeight { get; init; }
}
