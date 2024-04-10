namespace MeidoPhotoStudio.Plugin.Framework.Menu;

public class AFileBaseStream(AFileBase aFileBase) : Stream
{
    private AFileBase aFileBase = aFileBase ?? throw new ArgumentNullException(nameof(aFileBase));
    private bool disposed;

    public override bool CanRead =>
        true;

    public override bool CanSeek =>
        true;

    public override bool CanWrite =>
        false;

    public override long Length =>
        aFileBase.GetSize();

    public override long Position
    {
        get => aFileBase.Tell();
        set => aFileBase.Seek((int)value, true);
    }

    public override void Flush()
    {
    }

    public override int Read(byte[] buffer, int offset, int count) =>
        aFileBase.Read(ref buffer, count);

    public override long Seek(long offset, SeekOrigin origin)
    {
        var position = origin switch
        {
            SeekOrigin.Begin or SeekOrigin.Current => (int)offset,
            SeekOrigin.End => (int)offset + aFileBase.GetSize(),
            _ => throw new ArgumentException($"'{nameof(origin)}' is not a valid {nameof(SeekOrigin)}"),
        };

        return aFileBase.Seek(position, origin is SeekOrigin.Begin or SeekOrigin.End);
    }

    public override void SetLength(long value) =>
        throw new NotSupportedException($"{nameof(AFileBase)} does not support writing");

    public override void Write(byte[] buffer, int offset, int count) =>
        throw new NotSupportedException($"{nameof(AFileBase)} does not support writing");

    protected override void Dispose(bool disposing)
    {
        if (disposed)
            return;

        if (disposing)
        {
            aFileBase.Dispose();
            aFileBase = null;
        }

        disposed = true;

        base.Dispose(disposing);
    }
}
