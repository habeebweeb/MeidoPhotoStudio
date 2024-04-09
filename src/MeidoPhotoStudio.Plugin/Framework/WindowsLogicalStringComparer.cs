using System.Runtime.InteropServices;

namespace MeidoPhotoStudio.Plugin.Framework;

public class WindowsLogicalStringComparer : Comparer<string>
{
    public override int Compare(string x, string y) =>
        StrCmpLogicalW(x, y);

    [DllImport("shlwapi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern int StrCmpLogicalW(string x, string y);
}
