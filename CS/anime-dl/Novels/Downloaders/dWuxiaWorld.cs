using anime_dl.Ext;
using anime_dl.Novels;
using anime_dl.Novels.Models;
using MSHTML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace KobeiD.Downloaders
{
    /// <summary>
    /// WuxiaWorld.co
    /// </summary>
    class dWuxiaWorld : DownloaderBase
    {
        public dWuxiaWorld(string url) : base(url)
        {

        }

        public MetaData GetMetaData()
        {
            if (mdata != null)
                return mdata;

            pageEnumerator.Reset();

            Dictionary<string, LinkedList<IHTMLElement>> baseInfo = pageEnumerator.GetElementsByClassNames(new string[] { "book-name", "author", "book-state", "book-catalog", "score" });

            mdata = new MetaData();
            this.mdata.url = this.url.ToString();
            mdata.name = baseInfo["book-name"].First().innerText.DeleteFollowingWhiteSpaceA();
            mdata.author = baseInfo["author"].First().innerText.SkipPreceedingAndChar(':');
            mdata.type = baseInfo["book-state"].First().innerText.SkipPreceedingAndChar(' ').DeleteFollowingWhiteSpaceA();;
            mdata.genre = baseInfo["book-catalog"].First().innerText.DeleteFollowingWhiteSpaceA();
            mdata.rating = baseInfo["score"].First().innerText;

            mdata.cover = webClient.DownloadData($"https://img.wuxiaworld.co/BookFiles/BookImages/{mdata.name.Replace(' ', '-').Replace('\'', '-')}.jpg");

            pageEnumerator = page.all.GetEnumerator();
            pageEnumerator.Reset();
            baseInfo.Clear();
            return mdata;
        }

        public Chapter[] GetChapterLinks(bool sort = false)
        {
            Dictionary<string, LinkedList<IHTMLElement>> chapterInfo = pageEnumerator.GetElementsByClassNames(new string[] { "chapter-item" });
            System.Collections.IEnumerator a = chapterInfo["chapter-item"].GetEnumerator();
            Regex reg = new Regex("href=\"(.*?)\"");

            Chapter[] c = new Chapter[chapterInfo["chapter-item"].Count()];

            for (int idx = 0; idx < chapterInfo["chapter-item"].Count(); idx++)
            {
                a.MoveNext();
                c[idx] = new Chapter() { name = ((IHTMLElement)a.Current).innerText.Replace("\r\n", string.Empty), chapterLink = new Uri("https://www.wuxiaworld.co" + reg.Match(((IHTMLElement)a.Current).outerHTML).Groups[1].Value) };
            }
            reg = null;
            a = null;
            chapterInfo.Clear();

            return c;
        }
    }
}
