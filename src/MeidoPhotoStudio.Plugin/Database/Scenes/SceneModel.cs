namespace MeidoPhotoStudio.Database.Scenes;

public class SceneModel
{
    private Texture2D thumbnail;

    public SceneModel(string category, string filename)
    {
        if (string.IsNullOrEmpty(category))
            throw new ArgumentException($"'{nameof(category)}' cannot be null or empty.", nameof(category));

        if (string.IsNullOrEmpty(filename))
            throw new ArgumentException($"'{nameof(filename)}' cannot be null or empty.", nameof(filename));

        Category = category;
        Filename = filename;
        Name = Path.GetFileName(Filename);
    }

    public Texture2D Thumbnail
    {
        get
        {
            if (thumbnail)
                return thumbnail;

            thumbnail = new(1, 1, TextureFormat.ARGB32, false);
            thumbnail.LoadImage(File.ReadAllBytes(Filename));

            return thumbnail;
        }
    }

    public string Name { get; }

    public string Category { get; }

    public string Filename { get; }

    public void DestroyThumnail()
    {
        if (!thumbnail)
            return;

        Object.Destroy(thumbnail);
    }
}
