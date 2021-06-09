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
        }

        public override dynamic GenerateExtractor(argumentList args, int ti, Action<int, string> act)
            => DownloaderBase.GetDownloader(args, this, ti, act);
    }

    public class WuxiaWorld : SiteBase
    {
        public WuxiaWorld()
        {
            host = "www.wuxiaworld.co";
        }
        public override dynamic GenerateExtractor(argumentList args, int ti, Action<int, string> act)
            => DownloaderBase.GetDownloader(args, this, ti, act);
    }
    public class WuxiaWorldCOM : SiteBase
    {
        public WuxiaWorldCOM()
        {
            host = "www.wuxiaworld.com";
        }

        public override dynamic GenerateExtractor(argumentList args, int ti, Action<int, string> act)
            => DownloaderBase.GetDownloader(args, this, ti, act);
    }
}
