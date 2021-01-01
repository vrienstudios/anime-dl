using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using anime_dl;
using anime_dl.Ext;
using anime_dl.Interfaces;
using anime_dl.Video.Constructs;
using MSHTML;

namespace anime_dl.Video.Extractors
{
    abstract class ExtractorBase : IAppBase
    {
        public string downloadTo;
        public IHTMLDocument2 docu;
        public System.Collections.IEnumerator pageEnumerator;
        public WebClient webClient;
        public Root rootObj;
        public Constructs.Video videoInfo;

        public ExtractorBase()
        {
            webClient = new WebClient();
            docu = docu.GetDefaultDocument();
            pageEnumerator = docu.all.GetEnumerator();

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

        public abstract String Search(string name);
        public abstract String GetDownloadUri(string path);
        public abstract String GetDownloadUri(HentaiVideo path);
        public abstract void GenerateHeaders();

        public void LoadPage(string html)
        {
            docu = (MSHTML.IHTMLDocument2)new HTMLDocument();
            docu.designMode = "On";
            docu.write(html);
            docu.close();
            pageEnumerator = docu.all.GetEnumerator();
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
