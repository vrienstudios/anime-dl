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
using ADLCore.Ext.ExtendedClasses;

namespace ADLCore.Novels
{
    //All downloaders inherit from this class so that they can be handled easily.
    public abstract class DownloaderBase : IAppBase
    {
        public AWebClient webClient { get; set; }
        public HtmlDocument page { get; set; }
        argumentList ao { get; set; }
        public IEnumerator<HtmlNode> pageEnumerator { get; set; }

        public MetaData mdata
        {
            get { return this.thisBook.metaData; }
            set { this.thisBook.metaData = value; }
        }

        public Uri url { get; set; }

        public int taskIndex { get; set; }

        public Action<int, string> updateStatus { get; set; }

        public Book thisBook { get; set; }

        public DownloaderBase(argumentList args, int taskIndex, Action<int, string> act)
        {
            ao = args;

            this.taskIndex = taskIndex;
            this.updateStatus = act;


            ADLUpdates.CallLogUpdate("Creating Novel Download Instance");
            this.url = new Uri(args.term);
            webClient = new AWebClient();
            webClient.wCollection.Add("Referer", args.term);
            webClient.wCollection.Add("Host", this.url.Host);
            act.Invoke(taskIndex, $"SET WCHOST1: {url.Host} | SET WCREF1: {args.term}");
            string html = webClient.DownloadString(args.term);
            LoadPage(html);
            html = null;
        }

        public MetaData EndMDataRoutine()
        {
            pageEnumerator.Reset();
            ADLUpdates.CallLogUpdate($"Got MetaData Object for {mdata.name} by {mdata.author}");
            sU(taskIndex, $"Got MetaData Object for {mdata.name} by {mdata.author}");
            return mdata;
        }

        public void LoadBook(Action<int, string> u)
        {
            if (thisBook == null)
                thisBook = new Book(updateStatus, this, taskIndex,
                    ao.l ? ao.export : Environment.CurrentDirectory + Path.DirectorySeparatorChar + "Epubs");
            if (!ao.term.IsValidUri())
                thisBook.LoadFromADL(ao.term);
            else
            {
                mdata = GetMetaData();
                mdata.givenCommand = ao.ToString();
            }

            thisBook.root += Path.DirectorySeparatorChar + thisBook.metaData.name + ".adl";
        }

        public void InitialsChapterSetup()
        {
            Chapter[] chapters = ao.vRange
                ? GetChapterLinks(false, ao.VideoRange[0], ao.VideoRange[1])
                : GetChapterLinks();

            if (thisBook.chapters.Length != chapters.Length)
            {
                Chapter[] buffer = new Chapter[chapters.Length];
                Array.Copy(chapters, thisBook.chapters.Length, buffer, thisBook.chapters.Length,
                    chapters.Length - thisBook.chapters.Length);
                Array.Copy(thisBook.chapters, 0, buffer, 0, thisBook.chapters.Length);
                thisBook.chapters = buffer;
            }
        }

        public void RegChapterSetup()
        {
            thisBook.chapters = ao.vRange
                ? GetChapterLinks(false, ao.VideoRange[0], ao.VideoRange[1])
                : GetChapterLinks();
        }

        public dynamic StartQuery()
        {
            LoadBook(null);
            RegChapterSetup();

            if (!ao.d)
                return thisBook.chapters;
            else
            {
                thisBook.DownloadChapters(true);
                if (ao.mt)
                    thisBook.awaitThreadUnlock();
            }

            if (ao.e)
            {
                thisBook.ExportToEPUB(ao.l
                    ? ao.export + Path.DirectorySeparatorChar + thisBook.metaData.name
                    : Directory.GetCurrentDirectory() +
                      $"{Path.DirectorySeparatorChar}Epubs{Path.DirectorySeparatorChar}" + $"{thisBook.metaData.name}");
                return 0;
            }
            else
                return thisBook.chapters;
        }

        public void BeginExecution()
        {
            updateStatus?.Invoke(taskIndex, "Creating Book Instance.");

            LoadBook(updateStatus);

            thisBook.ExportToADL(); // Initialize Zipper
            if (ao.d)
            {
                if (thisBook.chapters != null && thisBook.chapters.Length > 0)
                    InitialsChapterSetup();
                else
                    RegChapterSetup();

                if (ao.vRange == true)
                {
                    Chapter[] chapters = new Chapter[ao.VideoRange[1] - ao.VideoRange[0]];
                    Array.Copy(thisBook.chapters, ao.VideoRange[0], chapters, 0, ao.VideoRange[1]);
                    thisBook.chapters = chapters;
                }

                thisBook.DownloadChapters(ao.mt);
            }

            if (ao.mt) // not unlocked if -mt is not specified, bypass.
                thisBook.awaitThreadUnlock(); // wait until done downloading. (ManualResetEvent not waiting)


            if (ao.e)
                thisBook.ExportToEPUB(ao.l
                    ? ao.export + Path.DirectorySeparatorChar + thisBook.metaData.name
                    : Directory.GetCurrentDirectory() +
                      $"{Path.DirectorySeparatorChar}Epubs{Path.DirectorySeparatorChar}" + $"{thisBook.metaData.name}");
        }

        public void sU(int a, string b)
        {
            b = $"{thisBook.metaData?.name} {b}";
            updateStatus?.Invoke(a, b);
        }

        public abstract MetaData GetMetaData();
        public abstract Chapter[] GetChapterLinks(bool sort = false, int x = 0, int y = 0);
        public abstract TiNodeList GetText(Chapter chp, HtmlDocument use, AWebClient wc);

        public void GenerateHeaders()
        {
            webClient.Headers.Add(
                "accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
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

        public abstract void GrabHome(int amount);
    }
}