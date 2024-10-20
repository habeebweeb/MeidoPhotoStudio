using MeidoPhotoStudio.Plugin.Core.Database.Props;
using MeidoPhotoStudio.Plugin.Core.Props;
using MeidoPhotoStudio.Plugin.Core.Schema.Props;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MeidoPhotoStudio.Plugin.Core.Serialization;

public class FavouritePropListSerializer : IFavouritePropListSerializer
{
    private readonly string favouritePropListFilePath;
    private readonly ISchemaBuilder<IPropModelSchema, IPropModel> propSchemaBuilder;
    private readonly PropSchemaToPropModelMapper propSchemaMapper;

    public FavouritePropListSerializer(
        string propListDirectory,
        ISchemaBuilder<IPropModelSchema, IPropModel> propSchemaBuilder,
        PropSchemaToPropModelMapper propSchemaMapper)
    {
        if (string.IsNullOrEmpty(propListDirectory))
            throw new ArgumentException($"'{nameof(propListDirectory)}' cannot be null or empty.", nameof(propListDirectory));

        favouritePropListFilePath = Path.Combine(propListDirectory, "favourite_props.json");

        this.propSchemaBuilder = propSchemaBuilder ?? throw new ArgumentNullException(nameof(propSchemaBuilder));
        this.propSchemaMapper = propSchemaMapper ?? throw new ArgumentNullException(nameof(propSchemaMapper));
    }

    private static JsonSerializer Serializer =>
        JsonSerializer.Create(new()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters = [new PropModelSchemaConverter()],
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented,
        });

    public void Serialize(IEnumerable<FavouritePropModel> favouriteProps)
    {
        using var fileStream = File.Create(favouritePropListFilePath);
        using var streamWriter = new StreamWriter(fileStream, Encoding.UTF8);
        using var jsonWriter = new JsonTextWriter(streamWriter);

        Serializer.Serialize(
            jsonWriter,
            favouriteProps.Select(favouriteProp => new FavouritePropModelSchema()
            {
                PropModel = propSchemaBuilder.Build(favouriteProp.PropModel),
                Name = favouriteProp.Name,
                DateAdded = favouriteProp.DateAdded,
            }));
    }

    public IEnumerable<FavouritePropModel> Deserialize()
    {
        try
        {
            using var fileStream = File.OpenRead(favouritePropListFilePath);
            using var streamReader = new StreamReader(fileStream, Encoding.UTF8);
            using var jsonReader = new JsonTextReader(streamReader);

            var favouriteProps = Serializer.Deserialize<IEnumerable<FavouritePropModelSchema>>(jsonReader);

            return favouriteProps
                .Select(ConvertSchema)
                .Where(favouriteProp => favouriteProp is not null);
        }
        catch
        {
            return [];
        }

        FavouritePropModel ConvertSchema(FavouritePropModelSchema schema)
        {
            var propModel = propSchemaMapper.Resolve(schema.PropModel);

            return propModel is null
                ? null
                : new(propModel, schema.Name, schema.DateAdded);
        }
    }
}
