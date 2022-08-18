using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public abstract class MenuItem
{
    public string IconFile { get; set; }

    public Texture2D Icon { get; set; }
}
