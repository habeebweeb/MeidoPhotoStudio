namespace MeidoPhotoStudio.Plugin.Core.Schema.Character;

public class CharactersSchema(short version = CharactersSchema.SchemaVersion)
{
    public const short SchemaVersion = 2;

    public short Version { get; } = version;

    public List<CharacterSchema> Characters { get; init; }

    public GlobalGravitySchema GlobalGravity { get; init; }
}
