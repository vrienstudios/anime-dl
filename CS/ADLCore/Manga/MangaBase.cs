using ADLCore.Alert;
using ADLCore.Ext;
using ADLCore.Interfaces;
using ADLCore.Novels.Models;
using ADLCore.Video.Constructs;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace ADLCore.Manga
{
    public abstract class MangaBase : IAppBase
    {
        public WebClient webClient;
        public HtmlDocument page;

        public IEnumerator<HtmlNode> pageEnumerator;

        public MetaData mdata;
        public Uri url;

        public int taskIndex;

        public Action<int, string> updateStatus;

        public MangaBase(string url, int taskIndex, Action<int, string> act)
        {
            if (taskIndex > -1 && act != null || taskIndex == -1 && act == null)
            {
                this.taskIndex = taskIndex;
                this.updateStatus = act;
            }
            else
                throw new Exception("Invalid statusUpdate args");

            ADLUpdates.CallUpdate("Creating Manga Download Instance", false);
            this.url = new Uri(url);
            webClient = new WebClient();
            GenerateHeaders();
            string html = webClient.DownloadString(url);
            LoadPage(html);
            html = null;
        }

        public void BeginExecution()
        {
            throw new NotImplementedException();
        }

        public void CancelDownload(string mdataLock)
        {
            throw new NotImplementedException();
        }

        public void GenerateHeaders()
        {
            throw new NotImplementedException();
        }

        public dynamic Get(HentaiVideo obj, bool dwnld)
        {
            throw new NotImplementedException();
        }

        public abstract MetaData GetMetaData();

        public void LoadPage(string html)
        {
            page = new HtmlDocument();
            page.LoadHtml(html);
            pageEnumerator = page.DocumentNode.FindAllNodes();
            GC.Collect();
        }

        public void MovePage(string uri)
        {
            webClient.Headers.Clear();
            GenerateHeaders();
            LoadPage(webClient.DownloadString(uri));
        }

        public void ResumeDownload(string mdataLock)
        {
            throw new NotImplementedException();
        }
    }
}
