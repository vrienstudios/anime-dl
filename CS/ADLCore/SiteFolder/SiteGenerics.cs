using ADLCore.Novels;
using ADLCore.Video.Constructs;
using System;
using System.Collections.Generic;
using System.Text;

namespace ADLCore.SiteFolder
{
    public abstract class SiteBase
    {
        public string host;
        public string type;
        public abstract dynamic GenerateExtractor(argumentList args, int ti, Action<int, string> act);

    }

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
            host = "novelhall.com";
            type = "nvl";
        }

        public override dynamic GenerateExtractor(argumentList args, int ti, Action<int, string> act)
            => new Novels.Downloaders.NovelHall(args, ti, act);
    }
}
