using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MeidoPhotoStudio.Plugin
{
    public class WindowsLogicalComparer : IComparer<string>
    {
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int StrCmpLogicalW(string x, string y);

        public int Compare(string x, string y) =>
            StrCmpLogicalW(x, y);
    }
}
