using System;

namespace MeidoPhotoStudio.Plugin.Core.Character;

public class CharacterServiceEventArgs(CharacterController[] loadedCharacters)
    : EventArgs
{
    public CharacterController[] LoadedCharacters { get; } = loadedCharacters
        ?? throw new ArgumentNullException(nameof(loadedCharacters));
}
