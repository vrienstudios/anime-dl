﻿using ADLCore.Ext;
using ADLCore.Interfaces;
using ADLCore.Novels.Models;
using ADLCore.Video.Constructs;
using ADLCore.Video.Extractors;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using ADLCore.Constructs;
using ADLCore.Video.Extractors.VidStream;

namespace ADLCore.Video
{
    public class VideoBase : IAppBase
    {
        public ExtractorBase extBase;
        public argumentList ao;
        int taskIndex = 0;
        Action<int, dynamic> updater;
        Thread videoDownloadThread;
        private IAppBase _appBaseImplementation;

        public VideoBase(argumentList args, int ti = -1, Action<int, dynamic> u = null)
        {
            ao = args;
            taskIndex = ti;
            updater = u;

            if (ao.s)
                GlobalAniSearch();
            else if (ao.tS)
                ao.term = SpecifiedSearch(new TwistMoe(ao, taskIndex, updater));
            else if (ao.gS)
                ao.term = SpecifiedSearch(new vidStreamAnimeEN(ao, taskIndex,
                    updater)); //..Anime only searching on streamani for the moment.
            else if (ao.hS)
                ao.term = SpecifiedSearch(new HAnime(ao, taskIndex, updater));
        }

        public void BeginExecution()
        {
            //TODO: Reimplement searching commands.
            if (extBase == null)
            {
                extBase = ao.term.SiteFromString().GenerateExtractor(ao, taskIndex, updater);
                extBase.baseUri = new Uril(ao.term).Host;
            }

            videoDownloadThread = new Thread(() => extBase.Begin());
            videoDownloadThread.Start();
            videoDownloadThread.Join(); // wait;
        }

        void IAppBase.CancelDownload(string mdataLock)
        {
            extBase.allStop = true;
            extBase.Aborted.WaitOne();
            extBase.CancelDownload(mdataLock);
            extBase.Aborted.Reset();
        }

        private ExtractorBase GenerateExtractorFromSite(Site s)
        {
            switch (s)
            {
                case Site.HAnime:
                    return new HAnime(ao, taskIndex, updater);
                case Site.TwistMoe:
                    return new TwistMoe(ao, taskIndex, updater);
                case Site.Vidstreaming:
                    return new vidStreamAnimeEN(ao, taskIndex, updater);
                default:
                    throw new Exception("unexpected site");
            }
        }

        void IAppBase.GenerateHeaders()
        {
            throw new NotImplementedException();
        }
        
        dynamic IAppBase.GetMetaData()
        {
            throw new NotImplementedException();
        }

        private string SpecifiedSearch(ExtractorBase _base)
            => _base.Search(false);


        /// <summary>
        /// Send query to all video extractors to search for the video.
        /// </summary>
        /// <param name="cacheAvailableAnimeList">Flag to cache json anime lists like those from twist.moe</param>
        /// <returns></returns>
        private void GlobalAniSearch(bool cacheAvailableAnimeList = true)
        {
            string search;

            updater?.Invoke(taskIndex, "Searching GoGoStream for Anime " + ao.term);
            ExtractorBase _base = new vidStreamAnimeEN(ao, taskIndex, updater);
            search = _base.Search(false);

            if (search != null)
                goto SetSearch;

            updater?.Invoke(taskIndex, "Searching HAnime.TV for Anime " + ao.term);
            _base = new HAnime(ao, taskIndex, updater);
            search = _base.Search(false);

            if (search != null)
                goto SetSearch;

            updater?.Invoke(taskIndex, "Searching Twist.Moe for Anime " + ao.term);
            _base = new TwistMoe(ao, taskIndex, updater);
            search = _base.Search();

            if (search != null)
                goto SetSearch;

            SetSearch: ;
            ao.term = search;
        }

        public void GrabLinks(int[] range)
        {
            throw new Exception("Video Is Not Supported At The Moment.");
        }

        public dynamic Search(bool puser, bool query)
        {
            throw new NotImplementedException();
        }

        void IAppBase.LoadPage(string html)
        {
            throw new NotImplementedException();
        }

        void IAppBase.MovePage(string uri)
        {
            throw new NotImplementedException();
        }

        void IAppBase.ResumeDownload(string mdataLock)
        {
            extBase.ResumeDownload(mdataLock);
        }

        public void GrabHome(int amount)
        {
            _appBaseImplementation.GrabHome(amount);
        }
    }
}