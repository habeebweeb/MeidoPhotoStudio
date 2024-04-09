namespace MeidoPhotoStudio.Database.Character;

public class HandPresetModel
{
    public HandPresetModel(long id, string category, string filename)
    {
        if (string.IsNullOrEmpty(category))
            throw new System.ArgumentException($"'{nameof(category)}' cannot be null or empty.", nameof(category));

        if (string.IsNullOrEmpty(filename))
            throw new System.ArgumentException($"'{nameof(filename)}' cannot be null or empty.", nameof(filename));

        ID = id;
        Category = category;
        Filename = filename;
    }

    public long ID { get; }

    public string Category { get; }

    public string Filename { get; }

    public string Name =>
        System.IO.Path.GetFileNameWithoutExtension(Filename);

    public override string ToString() =>
        $"{Name} ({ID})";
}
