using ADLCore.Ext;
using ADLCore.Interfaces;
using ADLCore.Novels;
using ADLCore.Video.Constructs;
using System;
using System.Linq;
using System.Text;
using ADLCore.SiteFolder;

namespace ADLCore
{
    public enum Site
    {
        Error,
        //Novel Sites below this
        AsianHobbyist,
        NovelFull,
        ScribbleHub,
        wuxiaWorldA,
        wuxiaWorldB,
        NovelHall,
        // Video sites below this
        TwistMoe,
        Vidstreaming,
        HAnime,
        // Manga Sites
        readKingdom,
        MangaKakalot,
        // Specific Servers below this
        www03Cloud9xx,
        // Integrated Sites
        MyAnimeList,
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
        public string Domain;

        public Uril(string url)
        {
            lnk = url;
            Host = gHost(url);
            string[] a = Host.Split('.');

            if (a.Length == 3)
                Domain = a[1];
            else
                Domain = a[0];
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

        private bool contains(string url, char d, int idx = 0) => url.Length < idx ? char.Equals(url[idx], d) ? true : contains(url, d, idx + 1) : false;
    }

    public static class Sites
    {
        //Acts as a dict for easy searching.
        public static readonly SiteBase[] continuity = new SiteBase[] {
            //Novels
            new AsianHobbyist(), new ScribbleHub(), new WuxiaWorld(), new WuxiaWorldCOM(), new NovelHall(), new NovelFull(), new VolareNovel(),
            //Anime
            new HAnime(), new GoGoStream(), new TwistMoe(), 
            //Manga
            new MangaKakolot(),
        };

        /// <summary>
        /// Gets the site from the urls through one-one matching.
        /// </summary>
        /// <param name="str"></param>
        /// <returns>Returns Site.{SiteObj} for easy handling.</returns>
        public static SiteBase SiteFromString(this string str)
        {
            Uril main = new Uril(str);
            if (str.IsValidUri())
            {
                SiteBase c = continuity.Where(x => x.chkHost(main.Host)).First();
                if (c == null)
                    throw new NotImplementedException("Based on Domain not implemented TODO");
                return c;
            }
            return null;
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
