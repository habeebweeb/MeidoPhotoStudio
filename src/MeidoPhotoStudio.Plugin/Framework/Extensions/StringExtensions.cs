using System.ComponentModel;

namespace MeidoPhotoStudio.Plugin.Framework.Extensions;

public static class StringExtensions
{
    public static bool Contains(this string str, string value, StringComparison comparison)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        if (!Enum.IsDefined(typeof(StringComparison), comparison))
            throw new InvalidEnumArgumentException(nameof(comparison), (int)comparison, typeof(StringComparison));

        return str.IndexOf(value, comparison) >= 0;
    }
}
