using System;

namespace MeidoPhotoStudio.Database.Character;

public class AddedBlendSetEventArgs(IBlendSetModel blendSet) : EventArgs
{
    public IBlendSetModel BlendSet { get; } = blendSet ?? throw new ArgumentNullException(nameof(blendSet));
}
