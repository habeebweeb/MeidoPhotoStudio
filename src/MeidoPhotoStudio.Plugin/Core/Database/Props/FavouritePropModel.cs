namespace MeidoPhotoStudio.Plugin.Core.Database.Props;

public class FavouritePropModel
{
    private string name;

    public FavouritePropModel(IPropModel prop, string name, DateTime dateAdded)
    {
        PropModel = prop ?? throw new ArgumentNullException(nameof(prop));
        Name = name;
        DateAdded = dateAdded;
    }

    public IPropModel PropModel { get; }

    public string Name
    {
        get => name;
        set
        {
            var newName = value;

            if (string.IsNullOrEmpty(newName))
                newName = PropModel.Name;

            name = newName;
        }
    }

    public DateTime DateAdded { get; }
}
