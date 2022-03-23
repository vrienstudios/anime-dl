using ADLCore.Novels;
using ADLCore.Video.Constructs;
using System;
using System.Collections.Generic;
using System.Text;

namespace ADLCore.SiteFolder
{
    public abstract class SiteBase
    {
        private List<string> hostContainer = new List<string>();
        private List<string> supports = new List<string>();

        public string host
        {
            get { return hostContainer[0]; }
            set { hostContainer.Add(value); }
        }
        
        public virtual string support
        {
            set { supports.Add(value); }
        }
        
        public string type;
        
        public abstract dynamic GenerateExtractor(argumentList args, int ti, Action<int, dynamic> act);

        public bool chkHost(string host)
            => hostContainer.Contains(host);

        public bool this[string x] => supports.Contains(x);
    }

    #region Novel Sites

    //Downloading from cloned NovelHall
    public class localHost : SiteBase
    {
        public localHost()
        {
            host = "10.10.1.12";
            support = "download";
            support = "search";
        }
        public override dynamic GenerateExtractor(argumentList args, int ti, Action<int, dynamic> act)
            => new Novels.Downloaders.NovelHall(args, ti, act);
    }
    
    public class AsianHobbyist : SiteBase
    {
        public AsianHobbyist()
        {
            host = "www.asianhobbyist.com";
            type = "nvl";
            support = "download";
        }

        public override dynamic GenerateExtractor(argumentList args, int ti, Action<int, dynamic> act)
            => new Novels.Downloaders.AsianHobbyist(args, ti, act);
    }

    public class WuxiaWorld : SiteBase
    {
        public WuxiaWorld()
        {
            host = "www.wuxiaworld.co";
            type = "nvl";
            support = "fail";
        }

        public override dynamic GenerateExtractor(argumentList args, int ti, Action<int, dynamic> act)
            => new Novels.Downloaders.dWuxiaWorld(args, ti, act);
    }

    public class WuxiaWorldCOM : SiteBase
    {
        public WuxiaWorldCOM()
        {
            host = "www.wuxiaworld.com";
            host = "wuxiaworld.com";
            type = "nvl";
            //support = "download";
            support = "fail";
        }

        public override dynamic GenerateExtractor(argumentList args, int ti, Action<int, dynamic> act)
            => new Novels.Downloaders.cWuxiaWorld(args, ti, act);
    }

    public class NovelFull : SiteBase
    {
        public NovelFull()
        {
            host = "novelfull.me";
            host = "novelfull.com";
            type = "nvl";
            support = "download";
        }

        public override dynamic GenerateExtractor(argumentList args, int ti, Action<int, dynamic> act)
            => new Novels.Downloaders.cNovelFull(args, ti, act);
    }

    public class ScribbleHub : SiteBase
    {
        public ScribbleHub()
        {
            host = "www.scribblehub.com";
            type = "nvl";
            support = "download";
        }

        public override dynamic GenerateExtractor(argumentList args, int ti, Action<int, dynamic> act)
            => new Novels.Downloaders.cScribbleHub(args, ti, act);
    }

    public class NovelHall : SiteBase
    {
        public NovelHall()
        {
            host = "www.novelhall.com";
            host = "novelhall.com";
            type = "nvl";
            support = "download";
            support = "search";
        }

        public override dynamic GenerateExtractor(argumentList args, int ti, Action<int, dynamic> act)
            => new Novels.Downloaders.NovelHall(args, ti, act);
    }

    public class VolareNovel : SiteBase
    {
        public VolareNovel()
        {
            host = "www.volarenovels.com";
            host = "volarenovels.com";
            type = "nvl";
            support = "download";
            support = "search";
        }

        public override dynamic GenerateExtractor(argumentList args, int ti, Action<int, dynamic> act)
            => new Novels.Downloaders.VolareNovels(args, ti, act);
    }

    #endregion

    #region Video Sites

    public class GoGoStream : SiteBase
    {
        public GoGoStream()
        {
            host = "asianload.cc";
            host = "asianembed.com";
            host = "asianembed.io";
            host = "asianload1.com";
            //ASIAN LOAD LEGACY
            host = "k-vid.co";

            host = "animeid.to";

            host = "gogoplay1.com";
            host = "streamani.net";
            host = "streamani.io";
            //STREAM ANI LEGACY
            host = "gogo-stream.com";
            host = "vidstreaming.io";
            host = "gogo-play.tv";

            host = "vidembed.cc";
            host = "vidembed.io";
            //VIDEMBED LEGACY
            host = "vidcloud9.com";
            host = "vidnode.net";
            host = "vidnext.net";

            type = "ani";
            support = "download";
            support = "search";
        }

        public override dynamic GenerateExtractor(argumentList args, int ti, Action<int, dynamic> act)
            => new Video.Extractors.GoGoStream(args, ti, act) { };
    }

    public class HAnime : SiteBase
    {
        public HAnime()
        {
            host = "hanime.tv";
            type = "ani";
            support = "download";
            support = "search";
        }

        public override dynamic GenerateExtractor(argumentList args, int ti, Action<int, dynamic> act)
            => new Video.Extractors.HAnime(args, ti, act);
    }

    public class TwistMoe : SiteBase
    {
        public TwistMoe()
        {
            host = "twist.moe";
            type = "ani";
            support = "download";
            support = "search";
        }

        public override dynamic GenerateExtractor(argumentList args, int ti, Action<int, dynamic> act)
            => new Video.Extractors.TwistMoe(args, ti, act);
    }

    #endregion

    #region Manga sites

    // Manga Sites will be thrown under the "nvl" categorization.
    public class MangaKakolot : SiteBase
    {
        public MangaKakolot()
        {
            // Jesus christ, pick one domain and stop.
            host = "mangakakalot.com";
            host = "manganato.com";
            host = "readmanganato.com";
            type = "nvl";
            support = "download";
        }

        public override dynamic GenerateExtractor(argumentList args, int ti, Action<int, dynamic> act)
            => new Manga.Downloaders.MangaKakalot(args, ti, act);
    }

    #endregion
}