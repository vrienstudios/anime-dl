using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using ADLCore;
using ADLCore.Ext;
using ADLCore.Interfaces;
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

        public ArgumentObject ao;

        public ExtractorBase(int ti, Action<int, string> u)
        {
            webClient = new WebClient();
            if (ti > -1 && u != null)
            {
                taskIndex = ti;
                updateStatus = u;
            }
            else
                throw new Exception("Action not provided when setting taskIndex");
        }

        public abstract bool Download(string path, bool mt, bool continuos);
        
        public bool mergeToMain(string path, byte[] data)
        {
            if (data.Length <= 0)
                return false;

            if (!File.Exists(path))
                File.Create(path).Close();
            using (FileStream fs = new FileStream(path, FileMode.Append))
                fs.Write(data, 0, data.Length);
            return true;
        }

        public abstract String Search(string name, bool d = false);
        public abstract String GetDownloadUri(string path);
        public abstract String GetDownloadUri(HentaiVideo path);
        public abstract void GenerateHeaders();
        public abstract dynamic Get(HentaiVideo obj, bool dwnld);

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
    }
}
