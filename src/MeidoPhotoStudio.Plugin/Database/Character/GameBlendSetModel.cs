using System;

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
}
