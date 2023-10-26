using System;

using MeidoPhotoStudio.Database.Background;
using UnityEngine;

namespace MeidoPhotoStudio.Plugin.Core.Background;

public class BackgroundChangeEventArgs : EventArgs
{
    public BackgroundChangeEventArgs(BackgroundModel backgroundInfo, Transform backgroundTransform)
    {
        BackgroundInfo = backgroundInfo;
        BackgroundTransform = backgroundTransform;
    }

    public Transform BackgroundTransform { get; }

    public BackgroundModel BackgroundInfo { get; }
}
