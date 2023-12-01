using System.Collections.Generic;

namespace MeidoPhotoStudio.Database.Props.Menu;

/// <summary>Object representation of a .menu file.</summary>
public partial class MenuFilePropModel : IPropModel
{
    private string name;

    public MenuFilePropModel(string filename, bool gameMenu)
    {
        if (string.IsNullOrEmpty(filename))
            throw new System.ArgumentException($"'{nameof(filename)}' cannot be null or empty.", nameof(filename));

        Filename = filename;
        ID = Filename.ToLower();
        GameMenu = gameMenu;
    }

    public string ID { get; }

    public bool GameMenu { get; }

    public string Filename { get; }

    public string OriginalName { get; private init; }

    public string Name
    {
        get => name;
        set => name = string.IsNullOrEmpty(value) ? OriginalName : value;
    }

    public MPN CategoryMpn { get; init; }

    public string IconFilename { get; init; }

    public float Priority { get; init; }

    public string ModelFilename { get; init; }

    public IEnumerable<MaterialChange> MaterialChanges { get; init; }

    public IEnumerable<ModelAnimation> ModelAnimations { get; init; }

    public IEnumerable<ModelMaterialAnimation> ModelMaterialAnimations { get; init; }
}
