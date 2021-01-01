using anime_dl.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using anime_dl.Novels.Models;
using System.Net;
using MSHTML;

namespace anime_dl.Novels
{
    class DownloaderBase : IAppBase
    {
        public WebClient webClient;
        public MSHTML.IHTMLDocument2 page;

        public System.Collections.IEnumerator pageEnumerator;

        public MetaData mdata;
        public Uri url;

        public DownloaderBase(string url)
        {
            this.url = new Uri(url);
            webClient = new WebClient();
            GenerateHeaders();
            string html = webClient.DownloadString(url);
            LoadPage(html);
            html = null;
        }

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
            page = (MSHTML.IHTMLDocument2)new HTMLDocument();
            page.designMode = "On";
            page.write(html);
            page.close();
            pageEnumerator = page.all.GetEnumerator();
            GC.Collect();
        }

        public void MovePage(string uri)
        {
            webClient.Headers.Clear();
            GenerateHeaders();
            LoadPage(webClient.DownloadString(url));
        }
    }
}
