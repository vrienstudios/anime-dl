﻿using ADLCore.Novels;
using ADLCore.Video.Constructs;
using System;
using System.Collections.Generic;
using System.Text;

namespace ADLCore.SiteFolder
{
    public abstract class SiteBase
    {
        private List<string> hostContainer = new List<string>();
        public string host { get { return hostContainer[0]; } set { hostContainer.Add(value); } }
        public string type;
        public abstract dynamic GenerateExtractor(argumentList args, int ti, Action<int, string> act);

        public bool chkHost(string host)
            => hostContainer.Contains(host);
    }

    #region Novel Sites
    public class AsianHobbyist : SiteBase
    {
        public AsianHobbyist()
        {
            host = "www.asianhobbyist.com";
            type = "nvl";
        }

        public override dynamic GenerateExtractor(argumentList args, int ti, Action<int, string> act)
            => new Novels.Downloaders.AsianHobbyist(args, ti, act);
    }

    public class WuxiaWorld : SiteBase
    {
        public WuxiaWorld()
        {
            host = "www.wuxiaworld.co";
            type = "nvl";
        }
        public override dynamic GenerateExtractor(argumentList args, int ti, Action<int, string> act)
            => new Novels.Downloaders.dWuxiaWorld(args, ti, act);
    }
    public class WuxiaWorldCOM : SiteBase
    {
        public WuxiaWorldCOM()
        {
            host = "www.wuxiaworld.com";
            type = "nvl";
        }

        public override dynamic GenerateExtractor(argumentList args, int ti, Action<int, string> act)
            => new Novels.Downloaders.cWuxiaWorld(args, ti, act);
    }

    public class NovelFull : SiteBase
    {
        public NovelFull()
        {
            host = "novelfull.com";
            type = "nvl";
        }

        public override dynamic GenerateExtractor(argumentList args, int ti, Action<int, string> act)
            => new Novels.Downloaders.cNovelFull(args, ti, act);
    }

    public class ScribbleHub : SiteBase
    {
        public ScribbleHub()
        {
            host = "www.scribblehub.com";
            type = "nvl";
        }

        public override dynamic GenerateExtractor(argumentList args, int ti, Action<int, string> act)
            => new Novels.Downloaders.cScribbleHub(args, ti, act);
    }

    public class NovelHall : SiteBase
    {
        public NovelHall()
        {
            host = "www.novelhall.com";
            host = "novelhall.com";
            type = "nvl";
        }

        public override dynamic GenerateExtractor(argumentList args, int ti, Action<int, string> act)
            => new Novels.Downloaders.NovelHall(args, ti, act);
    }
    #endregion

    #region Video Sites
    public class GoGoStream : SiteBase
    {
        public GoGoStream()
        {
            host = "gogo-stream.com";
            host = "vidstreaming.io";
            host = "streamani.net";
            type = "ani";
        }

        public override dynamic GenerateExtractor(argumentList args, int ti, Action<int, string> act)
            => new Video.Extractors.GoGoStream(args, ti, act);
    }

    public class HAnime : SiteBase
    {
        public HAnime()
        {
            host = "hanime.tv";
            type = "ani";
        }

        public override dynamic GenerateExtractor(argumentList args, int ti, Action<int, string> act)
            => new Video.Extractors.HAnime(args, ti, act);
    }    
    
    public class TwistMoe : SiteBase
    {
        public TwistMoe()
        {
            host = "twist.moe";
            type = "ani";
        }

        public override dynamic GenerateExtractor(argumentList args, int ti, Action<int, string> act)
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
        }

        public override dynamic GenerateExtractor(argumentList args, int ti, Action<int, string> act)
            => new Manga.Downloaders.MangaKakalot(args, ti, act);
    }
    #endregion
}
