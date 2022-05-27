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
using ADLCore.Constructs;
using ADLCore.Ext.ExtendedClasses;

namespace ADLCore.Novels
{
    //All downloaders inherit from this class so that they can be handled easily.
    public abstract class DownloaderBase : IAppBase
    {
        public AWebClient webClient { get; set; }
        public HtmlDocument page { get; set; }
        public argumentList ao { get; set; }
        public IEnumerator<HtmlNode> pageEnumerator { get; set; }

        public MetaData mdata
        {
            get
            {
                return this.thisBook?.metaData;
                
            }
            set
            {
                if (thisBook == null)
                    thisBook = new Book();
                this.thisBook.metaData = value;
            }
        }

        public Uri url { get; set; }

        public int taskIndex { get; set; }

        public Action<int, dynamic> updateStatus { get; set; }

        public Book thisBook { get; set; }

        public DownloaderBase(argumentList args, int taskIndex, Action<int, dynamic> act)
        {
            ao = args;

            this.taskIndex = taskIndex;
            this.updateStatus = act;


            ADLUpdates.CallLogUpdate("Creating Novel Download Instance");
            webClient = new AWebClient();
            
            if (!ao.s)
            {
                this.url = new Uri(args.term);
                setupWColAndDefPage();
            }
        }

        public virtual void setupWColAndDefPage()
        {
            webClient.wCollection.Add("Referer", ao.term);
            webClient.wCollection.Add("Host", this.url.Host);
            updateStatus?.Invoke(taskIndex, $"SET WCHOST1: {url.Host} | SET WCREF1: {ao.term}");
            string html = webClient.DownloadString(ao.term);
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

            if (ao.imgDefault)
                mdata.cover = Main.imageConverter == null ? mdata.getCover(mdata) : Main.imageConverter(mdata.getCover(mdata));
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
            if (ao.grabHome)
            {
                GrabHome(ao.vRange ? ao.VideoRange[1] : -1);
                return "Not Compatible With Other Options";
            }

            if (ao.linksOnly)
            {
                this.thisBook = new Book() { metaData = new MetaData(){ url = ao.term } };
                GrabLinks(ao.vRange == true ? ao.VideoRange : null);
                return "Not Compatible With Other Options";
            }

            LoadBook(null);

            if (ao.metaO)
            {
                updateStatus?.Invoke(-1, thisBook.metaData);
            }
            
            if (ao.d)
            {
                RegChapterSetup();
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

            return null;
        }

        public byte[] GetCover(MetaData ex)
        {
            using (AWebClient awc = new AWebClient())
            {
                awc.Headers.Add("Accept", "*/*");
                awc.Headers.Add("Host", new Uri(ao.term).Host);
                awc.Headers.Add("Accept-Encoding", "identity");
                awc.Headers.Add("Connection", "keep-alive");
                awc.userAgent = "Mozilla/5.0 (X11; Linux x86_64; rv:91.0) Gecko/20100101 Firefox/91.0";
                var b = awc.DownloadData(ex.coverPath);
                return b;
            }
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

        public abstract dynamic GetMetaData();
        public abstract Chapter[] GetChapterLinks(bool sort = false, int x = 0, int y = 0);
        public abstract TiNodeList GetText(Chapter chp, HtmlDocument use, AWebClient wc);
        public abstract void GrabLinks(int[] range);
        public abstract dynamic Search(bool promptUser = false, bool d = false);

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