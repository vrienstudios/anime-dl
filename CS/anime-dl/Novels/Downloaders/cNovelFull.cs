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
    class cNovelFull : DownloaderBase
    {
        public cNovelFull(string url) : base(url)
        {

        }

        public MetaData GetMetaData()
        {
            if (mdata != null)
                return mdata;

            pageEnumerator.Reset();

            Dictionary<string, LinkedList<IHTMLElement>> baseInfo = pageEnumerator.GetElementsByClassNames(new string[] { "title", "info", "book"});

            mdata = new MetaData();
            this.mdata.url = this.url.ToString();

            mdata.name = baseInfo["title"].First().innerText;
            string[] sp = baseInfo["info"].First().innerText.Replace("\r", string.Empty).Split("\n");
            mdata.author = sp[1];
            mdata.type = sp.Last();
            mdata.genre = sp[3];
            mdata.rating = "-1";

            string x = $"http://{url.Host}{Regex.Match(baseInfo["book"].First().outerHTML, @"<IMG[^>]+src=""([^"">]+)""").Groups[1].Value}";
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
                MovePage($"{mdata.url}?page={idx.ToString()}&per-page=50"); // limited to 50
                Dictionary<string, LinkedList<IHTMLElement>> chapterInfo = pageEnumerator.GetElementsByClassNames(new string[] { "list-chapter" });

                if (chapterInfo["list-chapter"].Count <= 0)
                    break;

                System.Collections.IEnumerator a = chapterInfo["list-chapter"].GetEnumerator();
                while(a.MoveNext())
                    foreach (MSHTML.IHTMLElement ele in ((IHTMLElement)a.Current).getLiElements())
                    {
                        Chapter ch = new Chapter() { name = ele.innerText, chapterLink = new Uri("https://" + url.Host + reg.Match(ele.innerHTML).Groups[1].Value) };
                        if (chaps.Where(x => x.chapterLink == ch.chapterLink).Count() == 0)
                            chaps.Add(ch);
                        else
                            goto exit;

                    }
            }
            exit:
            return chaps.ToArray();
        }
    }
}
