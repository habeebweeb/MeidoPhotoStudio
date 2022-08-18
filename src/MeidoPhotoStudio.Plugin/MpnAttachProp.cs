namespace MeidoPhotoStudio.Plugin;

public readonly struct MpnAttachProp
{
    public MpnAttachProp(MPN tag, string menuFile)
    {
        Tag = tag;
        MenuFile = menuFile;
    }

    public MPN Tag { get; }

    public string MenuFile { get; }
}
