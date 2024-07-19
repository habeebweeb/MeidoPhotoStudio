namespace MeidoPhotoStudio.Plugin.Framework.Extensions;

public static class MaidExtensions
{
    public static void SetMuneYureLWithEnable(this TBody body, bool enabled)
    {
        body.SetMuneYureL(enabled);

        if (body.jbMuneL)
            body.jbMuneL.enabled = enabled;
    }

    public static void SetMuneYureRWithEnable(this TBody body, bool enabled)
    {
        body.SetMuneYureR(enabled);

        if (body.jbMuneR)
            body.jbMuneR.enabled = enabled;
    }

    public static bool GetMuneLEnabled(this TBody body) =>
        body.jbMuneL && body.jbMuneL.enabled;

    public static bool GetMuneREnabled(this TBody body) =>
        body.jbMuneR && body.jbMuneR.enabled;

    public static string ID(this Maid maid) =>
        maid.status?.guid ?? string.Empty;

    public static bool ValueEquals(this Maid maid, Maid other)
    {
        if (other == null)
            return false;

        if (ReferenceEquals(maid, other))
            return true;

        if (maid.GetType() != other.GetType())
            return false;

        return string.Equals(maid.status.guid, other.status.guid, StringComparison.OrdinalIgnoreCase);
    }

    private static void SetMuneYureL(this TBody body, bool enabled) =>
        body.MuneYureL(Convert.ToSingle(enabled));

    private static void SetMuneYureR(this TBody body, bool enabled) =>
        body.MuneYureR(Convert.ToSingle(enabled));
}
