namespace MeidoPhotoStudio.Plugin.Core.Database.Scenes;

public class CategoryChangeEventArgs(string category) : EventArgs
{
    public string Category { get; } = string.IsNullOrEmpty(category)
        ? throw new ArgumentException($"'{nameof(category)}' cannot be null or empty")
        : category;
}
