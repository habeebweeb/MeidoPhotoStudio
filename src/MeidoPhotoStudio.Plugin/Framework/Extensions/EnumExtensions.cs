namespace MeidoPhotoStudio.Plugin.Framework.Extensions;

public static class EnumExtensions
{
    public static string ToLower<T>(this T enumValue)
        where T : Enum
    {
        var enumString = enumValue.ToString();

        return char.ToLower(enumString[0]) + enumString.Substring(1);
    }
}
