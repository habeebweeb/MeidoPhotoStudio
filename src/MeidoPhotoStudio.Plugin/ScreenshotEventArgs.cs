using System;

using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class ScreenshotEventArgs : EventArgs
{
    public string Path { get; set; } = string.Empty;

    public int SuperSize { get; set; } = -1;

    public bool HideMaids { get; set; }

    public bool InMemory { get; set; } = false;

    public Texture2D Screenshot { get; set; }
}
