using anime_dl.Ext;
using anime_dl.Novels;
using anime_dl.Novels.Models;
using MSHTML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace KobeiD.Downloaders
{
    class cScribbleHub : DownloaderBase
    {
        public cScribbleHub(string uri) : base(uri)
        {

        }

        public MetaData GetMetaData()
        {
            if (mdata != null)
                return mdata;

            pageEnumerator.Reset();

            Dictionary<string, LinkedList<IHTMLElement>> baseInfo = pageEnumerator.GetElementsByClassNames(new string[] { "fic_title", "auth_name_fic", "fic_image", "fic_genre" });

            mdata = new MetaData();
            this.mdata.url = this.url.ToString();

            mdata.name = baseInfo["fic_title"].First().innerText;
            mdata.author = baseInfo["auth_name_fic"].First().innerText;
            mdata.type = "unknown";
            mdata.genre = baseInfo["fic_genre"].First().innerText;
            mdata.rating = "-1";

            string x = Regex.Match(baseInfo["fic_image"].First().outerHTML, @"<IMG[^>]+src=""([^"">]+)""").Groups[1].Value;
            //x = x.Remove(x.IndexOf('?'));
            GenerateHeaders();
            mdata.cover = webClient.DownloadData(x);

            pageEnumerator = page.all.GetEnumerator();
            pageEnumerator.Reset();
            baseInfo.Clear();
            return mdata;
        }

        public Chapter[] GetChapterLinks(bool sort = false)
        {
            int idx = 0;
            List<Chapter> chaps = new List<Chapter>();
            Regex reg = new Regex("href=\"(.*?)\"");
            while (true)
            {
                idx++;
                MovePage($"{mdata.url}?toc={idx.ToString()}#content1");
                Dictionary<string, LinkedList<IHTMLElement>> chapterInfo = pageEnumerator.GetElementsByClassNames(new string[] { "toc_a" });

                if (chapterInfo["toc_a"].Count <= 0)
                    break;

                System.Collections.IEnumerator a = chapterInfo["toc_a"].GetEnumerator();
                while (a.MoveNext())
                    chaps.Add(new Chapter() { name = ((IHTMLElement)a.Current).innerText, chapterLink = new Uri(reg.Match(((IHTMLElement)a.Current).outerHTML).Groups[1].Value) });

            }
            chaps.Reverse();
            return chaps.ToArray();
        }
    }
}
