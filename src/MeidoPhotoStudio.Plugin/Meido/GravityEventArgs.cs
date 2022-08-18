using System;

using UnityEngine;

namespace MeidoPhotoStudio.Plugin;

public class GravityEventArgs : EventArgs
{
    public GravityEventArgs(bool isSkirt, Vector3 localPosition)
    {
        LocalPosition = localPosition;
        IsSkirt = isSkirt;
    }

    public Vector3 LocalPosition { get; }

    public bool IsSkirt { get; }
}
