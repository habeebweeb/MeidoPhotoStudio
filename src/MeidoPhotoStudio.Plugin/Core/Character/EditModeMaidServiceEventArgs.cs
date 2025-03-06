using MeidoPhotoStudio.Plugin.Core.Database.Character;

namespace MeidoPhotoStudio.Plugin.Core.Character;

public class EditModeMaidServiceEventArgs(Maid maid, CharacterModel characterModel) : EventArgs
{
    public Maid Maid { get; } = maid ? maid : throw new ArgumentNullException(nameof(maid));

    public CharacterModel Character { get; } = characterModel ?? throw new ArgumentNullException(nameof(characterModel));
}
