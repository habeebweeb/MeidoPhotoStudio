using System;

namespace MeidoPhotoStudio.Plugin;

public class MenuFilesEventArgs : EventArgs
{
    public MenuFilesEventArgs(EventType type) =>
        Type = type;

    public enum EventType
    {
        HandItems,
        MenuFiles,
        MpnAttach,
    }

    public EventType Type { get; }
}
