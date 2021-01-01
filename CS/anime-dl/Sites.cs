using System;
using System.Collections.Generic;
using System.Text;

namespace anime_dl
{
    public enum Site
    {
        Error,
        HAnime,
        NovelFull,
        ScribbleHub,
        Vidstreaming,
        wuxiaWorldA,
        wuxiaWorldB,
    }

    public static class Sites
    {
        public static Site SiteFromString(this string str)
        {
            switch (new Uri(str).Host)
            {
                case "www.wuxiaworld.co": return Site.wuxiaWorldA;
                case "www.wuxiaworld.com": return Site.wuxiaWorldB;
                case "www.scribblehub.com": return Site.ScribbleHub;
                case "novelfull.com": return Site.NovelFull;
                case "gogo-stream.com": return Site.Vidstreaming;
                case "vidstreaming.io": return Site.Vidstreaming;
                case "hanime.tv": return Site.HAnime;
                default: return Site.Error;
            }
        }
    }
}
