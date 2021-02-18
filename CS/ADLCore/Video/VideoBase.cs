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
        argumentList ao;
        int taskIndex = 0;
        Action<int, string> updater;

        public VideoBase(argumentList args, int ti = -1, Action<int, string> u = null)
        {
            ao = args;
            taskIndex = ti;
            updater = u;

            if (ao.s)
                GlobalAniSearch();

            BeginExecution();
        }

        private void BeginExecution()
        {
            if (extBase != null)
                goto SKIP;
            else
                switch (ao.term.SiteFromString())
                {
                    case Site.HAnime:
                        extBase = GenerateExtractorFromSite(Site.HAnime);
                        if (!ao.d)
                        {
                            updater(taskIndex, $"{ao.term.SkipCharSequence("https://hanime.tv/videos/hentai/".ToCharArray())} {extBase.GetDownloadUri(ao.term)}");
                            return;
                        }
                        break;
                    case Site.TwistMoe:
                        extBase = GenerateExtractorFromSite(Site.TwistMoe);
                        break;
                    case Site.Vidstreaming:
                        extBase = GenerateExtractorFromSite(Site.Vidstreaming);
                        break;
                    default:
                        if (ao.hS)
                            extBase = GenerateExtractorFromSite(Site.HAnime);
                        else if (ao.gS)
                            extBase = GenerateExtractorFromSite(Site.Vidstreaming);
                        else if (ao.tS)
                            extBase = GenerateExtractorFromSite(Site.TwistMoe);
                        break;
                }
            SKIP:;
            extBase.Begin();
        }

        private ExtractorBase GenerateExtractorFromSite(Site s)
        {
            switch(s)
            {
                case Site.HAnime:
                    return new HAnime(ao, taskIndex, updater);
                case Site.TwistMoe:
                    return new TwistMoe(ao, taskIndex, updater);
                case Site.Vidstreaming:
                    return new GoGoStream(ao, taskIndex, updater);
                default:
                    throw new Exception("unexpected site");
            }
        }

        /// <summary>
        /// Send query to all video extractors to search for the video.
        /// </summary>
        /// <param name="cacheAvailableAnimeList">Flag to cache json anime lists like those from twist.moe</param>
        /// <returns></returns>
        private void GlobalAniSearch(bool cacheAvailableAnimeList = true)
        {
            updater?.Invoke(taskIndex, "Searching Twist.Moe");
            TwistMoe tm = new TwistMoe(ao, taskIndex, updater);
            string search = tm.Search();
            
            if (search != null)
                goto SetSearch;
            
            updater?.Invoke(taskIndex, "Searching GoGoStream for Anime " + ao.term);
            GoGoStream ggS = new GoGoStream(ao, taskIndex, updater);
            search = ggS.Search(false);
            
            if (search != null)
                goto SetSearch;

            SetSearch:;
            ao.term = search;
        }
    }
}
