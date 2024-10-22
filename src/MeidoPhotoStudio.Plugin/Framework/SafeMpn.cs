namespace MeidoPhotoStudio.Plugin.Framework;

internal static class SafeMpn
{
    private static readonly Dictionary<string, MPN> StringToMpn = new(StringComparer.OrdinalIgnoreCase);

    public static MPN GetValue(string mpnName)
    {
        if (string.IsNullOrEmpty(mpnName))
            throw new ArgumentNullException(nameof(mpnName));

        if (StringToMpn.TryGetValue(mpnName, out var mpn))
            return mpn;

        mpn = StringToMpn[mpnName] = (MPN)Enum.Parse(typeof(MPN), mpnName, true);

        return mpn;
    }

    public static IEnumerable<MPN> GetValues(params string[] mpnNames) =>
        mpnNames.Select(GetValue);
}
