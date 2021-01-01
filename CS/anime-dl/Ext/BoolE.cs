using System;
using System.Collections.Generic;
using System.Text;

namespace anime_dl.Ext
{
    public static class BoolE
    {
        public static bool IsMp4(this string lnk) => lnk.Contains(".mp4");

        public static bool IsValidUri(this string lnk) => Uri.IsWellFormedUriString(lnk, UriKind.Absolute);
    }
}
