using ADLCore.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using ADLCore.Novels.Models;
using System.Net;
using HtmlAgilityPack;
using ADLCore.Ext;
using ADLCore.Alert;
using ADLCore.Video.Constructs;
using ADLCore.SiteFolder;
using System.IO;

namespace ADLCore.Novels
{
    //All downloaders inherit from this class so that they can be handled easily.
    public abstract class DownloaderBase : IAppBase
    {
        public WebClient webClient;
        public HtmlDocument page;
        argumentList ao;
        public IEnumerator<HtmlNode> pageEnumerator;

        public MetaData mdata;
        public Uri url;

        public int taskIndex;

        public Action<int, string> updateStatus;

        public Book thisBook;

        public DownloaderBase(argumentList args, int taskIndex, Action<int, string> act)
        {
            ao = args;
            if (taskIndex > -1 && act != null || taskIndex == -1 && act == null)
            {
                this.taskIndex = taskIndex;
                this.updateStatus = act;
            }
            else
                throw new Exception("Invalid statusUpdate args");

            ADLUpdates.CallUpdate("Creating Novel Download Instance", false);
            this.url = new Uri(args.term);
            webClient = new WebClient();
            GenerateHeaders();
            string html = webClient.DownloadString(args.term);
            LoadPage(html);
            html = null;
        }

        public void BeginExecution()
        {
            updateStatus.CommitMessage(taskIndex, "Creating Book Instance.");
            thisBook = new Book() { statusUpdate = updateStatus, dBase = this, ti = taskIndex, root = ao.l ? ao.export : Environment.CurrentDirectory + "\\Epubs" };
            
            thisBook.metaData = GetMetaData();
            thisBook.root += Path.DirectorySeparatorChar + thisBook.metaData.name + ".adl";

            thisBook.ExportToADL(); // Initialize Zipper
            if(thisBook.chapters == null || thisBook.chapters.Length == 0)
                thisBook.chapters = GetChapterLinks();
            thisBook.DownloadChapters(ao.mt);
            
            if(ao.mt) // not unlocked if -mt is not specified, bypass.
                thisBook.awaitThreadUnlock(); // wait until done downloading. (ManualResetEvent not waiting)
            

            if(ao.e)
                thisBook.ExportToEPUB(ao.l ? ao.export + Path.DirectorySeparatorChar + thisBook.metaData.name : Directory.GetCurrentDirectory() + $"{Path.DirectorySeparatorChar}Epubs{Path.DirectorySeparatorChar}" + $"{thisBook.metaData.name}");
        }

        private void sU(int a, string b)
        {
            b = $"{thisBook.metaData.name} {b}";
            updateStatus(a, b);
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
        public abstract dynamic Get(HentaiVideo obj, bool dwnld);

        void IAppBase.CancelDownload(string mdataLock)
        {
            throw new NotImplementedException("Novel Download Control Not Supported");
        }

        void IAppBase.ResumeDownload(string mdataLock)
        {
            throw new NotImplementedException("Novel Download Control Not Supported");
        }
    }
}
