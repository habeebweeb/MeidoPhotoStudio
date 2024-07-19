namespace MeidoPhotoStudio.Database.Character;

public class GameBlendSetModel(PhotoFaceData photoFaceData, string name = "") : IBlendSetModel
{
    private readonly PhotoFaceData photoFaceData = photoFaceData
        ?? throw new ArgumentNullException(nameof(photoFaceData));

    private string name = string.IsNullOrEmpty(name) ? photoFaceData.name : name;

    public int ID =>
        photoFaceData.id;

    public string Name
    {
        get => name;
        set => name = string.IsNullOrEmpty(value) ? name : value;
    }

    public string Category =>
        photoFaceData.category;

    public string BlendSetName =>
        photoFaceData.setting_name;

    public bool Custom =>
        false;

    public static bool operator ==(GameBlendSetModel lhs, GameBlendSetModel rhs) =>
        lhs is null ? rhs is null : lhs.Equals(rhs);

    public static bool operator !=(GameBlendSetModel lhs, GameBlendSetModel rhs) =>
        !(lhs == rhs);

    public override bool Equals(object other) =>
        other is GameBlendSetModel model && Equals(model);

    public bool Equals(IBlendSetModel other) =>
        other is GameBlendSetModel model && Equals(model);

    public bool Equals(GameBlendSetModel other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        return ID == other.ID;
    }

    public override int GetHashCode() =>
        (photoFaceData, ID, Name, Custom).GetHashCode();
}
