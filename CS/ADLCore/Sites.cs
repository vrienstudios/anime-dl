using ADLCore.Ext;
using System;
using System.Linq;
using System.Text;

namespace ADLCore
{
    public enum Site
    {
        Error,
        AsianHobbyist,
        HAnime,
        NovelFull,
        ScribbleHub,
        Vidstreaming,
        wuxiaWorldA,
        wuxiaWorldB,
        NovelHall,
        // Video servers below this link
        www03Cloud9xx,
    }

    public enum ImageExtensions
    {
        JPG,
        PNG,
        GIF,
        Error
    }

    public class Uril
    {
        public string lnk;
        public string Host;
        public Uril(string url)
        {
            lnk = url;
            //if(!url.Contains("//"))
            //Host = getHost(url, true);
            //else
            //Host = getHost(url);
            Host = gHost(url);
        }

        private string gHost(string uri)
        {
            for (int idx = 0; idx < uri.Length; idx++)
                if (char.Equals(uri[idx], '/') && char.Equals(uri[idx + 1], '/'))
                    return LoopUntilSlash(uri, idx + 2);
            return LoopUntilSlash(uri, 0);
        }

        private string LoopUntilSlash(string l, int idx)
        {
            StringBuilder builder = new StringBuilder();
            builder.Capacity = l.Length;
            for (int i = idx; i < l.Length; i++)
            {
                if (!char.Equals(l[i], '/'))
                    builder.Append(l[i]);
                else
                    return builder.ToString();
            }
            return builder.ToString();
        }

        [Obsolete] // Made for fun
        private string getHost(string url, bool d = false, int idx = 0, int k = 0, bool ht = false, int start = 0, int end = 0) 
            => idx < url.Length
                ? d == false 
                    ? char.Equals(url[idx], '/') && char.Equals(url[idx + 1], '/') 
                        ? getHost(url, true, idx + 2, k, ht, idx + 2, idx + 2) : char.Equals(url[idx], '/') 
                            ? getHost(url, true, k) : getHost(url, d, idx + 1, k + 1, false) : !(char.Equals(url[idx], '/')) && (idx < url.Length) ? getHost(url, true, idx + 1, k, ht, start, end + 1) : buildString(url, start, end) : buildString(url, start, end);
       
        private bool contains(string url, char d, int idx = 0) => url.Length < idx ? char.Equals(url[idx], d) ? true : contains(url, d, idx + 1) : false;
        private string buildString(string x, int d, int i)
        {
            StringBuilder sb = new StringBuilder();
            sb.Capacity = i - d;
            for (int idx = d; idx < i; idx++)
                sb.Append(x[idx]);
            return sb.ToString();
        }
        public override string ToString()
        {
            return base.ToString();
        }
    }

    public static class Sites
    {
        public static Site SiteFromString(this string str)
        {
            if (str.IsValidUri())
                switch (new Uril(str).Host)
                {
                    case "www.asianhobbyist.com": return Site.AsianHobbyist;
                    case "www.wuxiaworld.co": return Site.wuxiaWorldA;
                    case "www.wuxiaworld.com": return Site.wuxiaWorldB;
                    case "www.scribblehub.com": return Site.ScribbleHub;
                    case "novelfull.com": return Site.NovelFull;
                    case "novelhall.com": return Site.NovelHall;
                    case "gogo-stream.com": return Site.Vidstreaming;
                    case "vidstreaming.io": return Site.Vidstreaming;
                    case "hanime.tv": return Site.HAnime;
                    default: return Site.Error;
                }
            else
                return Site.Error;
        }

        public static Site m3u8ServerFromString(this string str)
        {
            switch (new Uri(str).Host)
            {
                case "www03.cloud9xx.com": return Site.www03Cloud9xx;
                default: return Site.Error;
            }
        }

        public static ImageExtensions GetImageExtension(this string str)
        {
            switch (new string(str.Skip(str.Length - 3).ToArray()))
            {
                case "jpg": return ImageExtensions.JPG;
                case "png": return ImageExtensions.PNG;
                case "gif": return ImageExtensions.GIF;
                default:
                    return ImageExtensions.Error;
            }
        }
    }
}
