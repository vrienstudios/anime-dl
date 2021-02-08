using ADLCore.Ext;
using ADLCore.Video.Constructs;
using ADLCore.Video.Extractors;
using KobeiD.Downloaders;
using System;
using System.Collections.Generic;
using System.Text;

namespace ADLCore.Video
{
    public class VideoBase
    {
        ExtractorBase extBase;
        public VideoBase(ArgumentObject args, int ti = -1, Action<int, string> u = null)
        {
            if (args.s)
                if (args.h)
                    extBase = new HAnime(args, ti, u);
                else
                    extBase = new GoGoStream(args, ti, u);
            else
                switch (args.term.SiteFromString())
                {
                    case Site.HAnime:
                        extBase = new HAnime(args, ti, u);
                        if (!args.d)
                        {
                            u(ti, $"{args.term.SkipCharSequence("https://hanime.tv/videos/hentai/".ToCharArray())} {extBase.GetDownloadUri(args.term)}");
                            return;
                        }
                        break;
                    case Site.TwistMoe:
                        extBase = new TwistMoe(args, ti, u);
                        break;
                    case Site.Vidstreaming:
                        extBase = new GoGoStream(args, ti, u);
                        break;
                }
            extBase.Begin();
        }
    }
}
