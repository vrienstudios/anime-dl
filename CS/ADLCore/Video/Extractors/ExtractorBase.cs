using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using ADLCore;
using ADLCore.Ext;
using ADLCore.Interfaces;
using ADLCore.Novels.Models;
using ADLCore.Video.Constructs;
using HtmlAgilityPack;

namespace ADLCore.Video.Extractors
{
    public abstract class ExtractorBase : IAppBase
    {
        public string downloadTo;
        public HtmlDocument docu;
        public IEnumerator<HtmlNode> pageEnumerator;
        public WebClient webClient;
        public Root rootObj;
        public Constructs.Video videoInfo;
        public int taskIndex;
        public Action<int, string> updateStatus;
        public Site quester;
        public argumentList ao;
        protected int m3uLocation;

        public ManualResetEvent Aborted;
        public bool allStop = false;

        public ExtractorBase(argumentList a, int ti, Action<int, string> u, Site host)
        {
            Aborted = new ManualResetEvent(false);
            ao = a;
            webClient = new WebClient();
            if (ti > -1 && u != null)
            {
                taskIndex = ti;
                updateStatus = u;
            }
            else
                throw new Exception("Action not provided when setting taskIndex");

            quester = host;
        }

        protected void invoker()
        {
            Aborted.Set();
        }

        public abstract void Begin();
        public abstract bool Download(string path, bool mt, bool continuos);
        
        protected bool mergeToMain(string path, byte[] data)
        {
            if (data.Length <= 0)
                return false;

            if (!File.Exists(path))
                File.Create(path).Close();
            using (FileStream fs = new FileStream(path, FileMode.Append))
                fs.Write(data, 0, data.Length);
            return true;
        }

        public abstract String Search(bool d = false);
        public abstract String GetDownloadUri(string path);
        public abstract String GetDownloadUri(HentaiVideo path);
        public abstract void GenerateHeaders();
        public abstract dynamic Get(HentaiVideo obj, bool dwnld);
        public abstract MetaData GetMetaData();

        public void LoadPage(string html)
        {
            docu = new HtmlDocument();
            docu.LoadHtml(html);
            pageEnumerator = docu.DocumentNode.FindAllNodes();
            GC.Collect();
        }

        public void MovePage(string uri)
        {
            webClient.Headers.Clear();
            GenerateHeaders();
            LoadPage(webClient.DownloadString(uri));
        }

        public void CancelDownload(string mdataLock)
        {
            string _base = downloadTo.TrimToSlash();
            string exp = downloadTo += "_tmp";

            using (FileStream fs = new FileStream(exp, FileMode.Create))
            using (ZipArchive zarch = new ZipArchive(fs, ZipArchiveMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(zarch.CreateEntry("part").Open()))
                {
                    sw.WriteLine($"{m3uLocation}");
                }
                using (StreamWriter sw = new StreamWriter(zarch.CreateEntry("mDat").Open()))
                {
                    sw.WriteLine($"{ao.ToString()}");
                }
            }
        }

        public void ResumeDownload(string mdataLock)
        {
            string _base = downloadTo.TrimToSlash();
            string exp = downloadTo += "_tmp";
            FileStream fs = new FileStream(exp, FileMode.Open);
            ZipArchive zarch = new ZipArchive(fs, ZipArchiveMode.Read);
            zarch.CreateEntry("part");
        }

        protected bool CompatibilityCheck()
        {
            switch(quester)
            {
                case Site.HAnime: return true;
                default:
                    throw new Exception("This site does not support Cancellation or Resumation");
            }
        }
    }
}
