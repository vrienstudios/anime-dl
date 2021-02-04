using ADLCore.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using ADLCore.Novels.Models;
using System.Net;
using HtmlAgilityPack;
using ADLCore.Ext;
using ADLCore.Alert;

namespace ADLCore.Novels
{
    public abstract class DownloaderBase : IAppBase
    {
        public WebClient webClient;
        public HtmlDocument page;

        public IEnumerator<HtmlNode> pageEnumerator;

        public MetaData mdata;
        public Uri url;

        public int taskIndex;

        public Action<int, string> updateStatus;

        public DownloaderBase(string url, int taskIndex, Action<int, string> act)
        {
            if (taskIndex > -1 && act != null || taskIndex == -1 && act == null)
            {
                this.taskIndex = taskIndex;
                this.updateStatus = act;
            }
            else
                throw new Exception("Invalid statusUpdate args");

            ADLUpdates.CallUpdate("Creating Novel Download Instance", false);
            this.url = new Uri(url);
            webClient = new WebClient();
            GenerateHeaders();
            string html = webClient.DownloadString(url);
            LoadPage(html);
            html = null;
        }
        public abstract MetaData GetMetaData();
        public abstract Chapter[] GetChapterLinks(bool sort = false);
        public abstract string GetText(Chapter chp, HtmlDocument use, WebClient wc);
        public void GenerateHeaders()
        {
            webClient.Headers.Add("accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
            webClient.Headers.Add("User-Agent: Mozilla/5.0 (compatible; MSIE 10.6; Windows NT 6.1; Trident/5.0; InfoPath.2; SLCC1; .NET CLR 3.0.4506.2152; .NET CLR 3.5.30729; .NET CLR 2.0.50727) 3gpp-gba UNTRUSTED/1.0");
            webClient.Headers.Add("referer", url.Host);
            webClient.Headers.Add("DNT", "1");
            webClient.Headers.Add("Upgrade-Insecure-Requests", "1");
        }
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
    }
}
