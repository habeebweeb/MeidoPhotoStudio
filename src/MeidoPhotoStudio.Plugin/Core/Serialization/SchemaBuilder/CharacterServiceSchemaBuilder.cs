using MeidoPhotoStudio.Plugin.Core.Character;
using MeidoPhotoStudio.Plugin.Core.Schema.Character;

namespace MeidoPhotoStudio.Plugin.Core.Serialization;

public class CharacterServiceSchemaBuilder(
    CharacterService characterService,
    GlobalGravityService globalGravityService,
    ISchemaBuilder<CharacterSchema, CharacterController> characterSchemaBuilder,
    ISchemaBuilder<GlobalGravitySchema, GlobalGravityService> globalGravitySchemaBuilder)
    : ISceneSchemaAspectBuilder<CharactersSchema>
{
    private readonly CharacterService characterService = characterService
        ?? throw new ArgumentNullException(nameof(characterService));

    private readonly GlobalGravityService globalGravityService = globalGravityService
        ?? throw new ArgumentNullException(nameof(globalGravityService));

    private readonly ISchemaBuilder<CharacterSchema, CharacterController> characterSchemaBuilder =
        characterSchemaBuilder ?? throw new ArgumentNullException(nameof(characterSchemaBuilder));

    private readonly ISchemaBuilder<GlobalGravitySchema, GlobalGravityService> globalGravitySchemaBuilder =
        globalGravitySchemaBuilder ?? throw new ArgumentNullException(nameof(globalGravitySchemaBuilder));

    public CharactersSchema Build() =>
        new()
        {
            Characters = characterService.Select(characterSchemaBuilder.Build).ToList(),
            GlobalGravity = globalGravitySchemaBuilder.Build(globalGravityService),
        };
}
