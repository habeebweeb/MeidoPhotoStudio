namespace MeidoPhotoStudio.Database.Character;

public class CustomBlendSetModel : IBlendSetModel
{
    public CustomBlendSetModel(long id, string category, string filename)
    {
        if (string.IsNullOrEmpty(category))
            throw new ArgumentException($"'{nameof(category)}' cannot be null or empty.", nameof(category));

        if (string.IsNullOrEmpty(filename))
            throw new ArgumentException($"'{nameof(filename)}' cannot be null or empty.", nameof(filename));

        ID = id;
        Category = category;
        BlendSetName = filename;
    }

    public long ID { get; }

    public string Category { get; }

    public string Name =>
        Path.GetFileNameWithoutExtension(BlendSetName);

    public string BlendSetName { get; }

    public bool Custom =>
        true;
}