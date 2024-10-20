namespace MeidoPhotoStudio.Plugin.Core.Database.Props;

public class FavouritePropRepository(IFavouritePropListSerializer favouritePropListSerializer)
    : IEnumerable<FavouritePropModel>
{
    private readonly IFavouritePropListSerializer favouritePropListSerializer = favouritePropListSerializer
        ?? throw new ArgumentNullException(nameof(favouritePropListSerializer));

    private Dictionary<IPropModel, FavouritePropModel> props;
    private List<FavouritePropModel> favouriteProps;

    public event EventHandler<FavouritePropRepositoryEventArgs> AddedFavouriteProp;

    public event EventHandler<FavouritePropRepositoryEventArgs> RemovedFavouriteProp;

    public event EventHandler Refreshing;

    public event EventHandler Refreshed;

    public int Count =>
        FavouriteProps.Count;

    private List<FavouritePropModel> FavouriteProps
    {
        get
        {
            if (favouriteProps is not null)
                return favouriteProps;

            Initialize();

            return favouriteProps;
        }
    }

    private Dictionary<IPropModel, FavouritePropModel> Props
    {
        get
        {
            if (props is not null)
                return props;

            Initialize();

            return props;
        }
    }

    public FavouritePropModel this[int index] =>
        (uint)index >= FavouriteProps.Count
            ? throw new ArgumentOutOfRangeException(nameof(index))
            : FavouriteProps[index];

    public IEnumerator<FavouritePropModel> GetEnumerator() =>
        FavouriteProps.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    public bool ContainsProp(IPropModel prop) =>
        prop is null
            ? throw new ArgumentNullException(nameof(prop))
            : Props.ContainsKey(prop);

    public void Add(IPropModel prop)
    {
        _ = prop ?? throw new ArgumentNullException(nameof(prop));

        if (Props.ContainsKey(prop))
            return;

        var favouriteProp = new FavouritePropModel(prop, string.Empty, DateTime.UtcNow);

        FavouriteProps.Add(favouriteProp);
        Props.Add(prop, favouriteProp);

        Utility.LogDebug($"Added new prop: {prop.Name}");

        AddedFavouriteProp?.Invoke(this, new(favouriteProp));
    }

    public void Remove(IPropModel favouriteProp)
    {
        _ = favouriteProp ?? throw new ArgumentNullException(nameof(favouriteProp));

        if (!Props.TryGetValue(favouriteProp, out var favouritePropModel))
            return;

        Props.Remove(favouriteProp);
        FavouriteProps.Remove(favouritePropModel);

        Utility.LogDebug($"Removed prop: {favouriteProp.Name}");

        RemovedFavouriteProp?.Invoke(this, new(favouritePropModel));
    }

    public void Refresh()
    {
        Refreshing?.Invoke(this, EventArgs.Empty);

        Initialize();

        Refreshed?.Invoke(this, EventArgs.Empty);
    }

    public void Save() =>
        favouritePropListSerializer.Serialize(this);

    private void Initialize()
    {
        favouriteProps = [.. favouritePropListSerializer.Deserialize()];

        props = favouriteProps.ToDictionary(favouriteProp => favouriteProp.PropModel, favouriteProp => favouriteProp);
    }
}
