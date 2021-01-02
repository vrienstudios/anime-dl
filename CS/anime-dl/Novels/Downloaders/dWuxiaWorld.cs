using anime_dl.Ext;
using anime_dl.Novels;
using anime_dl.Novels.Models;
using HtmlAgilityPack;
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
            Dictionary<string, LinkedList<HtmlNode>> baseInfo = pageEnumerator.GetElementsByClassNames(new string[] { "book-name", "author", "book-state", "book-catalog", "score" });

            mdata = new MetaData();
            this.mdata.url = this.url.ToString();
            try
            {
                mdata.name = baseInfo["book-name"].First().InnerText.DeleteFollowingWhiteSpaceA();
                mdata.author = baseInfo["author"].First().InnerText.SkipPreceedingAndChar(':');
                mdata.type = baseInfo["book-state"].First().InnerText.SkipPreceedingAndChar(' ').DeleteFollowingWhiteSpaceA();
                mdata.genre = baseInfo["book-catalog"].First().InnerText.DeleteFollowingWhiteSpaceA();
                mdata.rating = baseInfo["score"].First().InnerText;
            } catch  {
                Console.WriteLine($"Failed to load some values\n");
                Console.WriteLine(mdata.name);
                Console.WriteLine(mdata.author);
                Console.WriteLine(mdata.type);
                Console.WriteLine(mdata.genre);
                Console.WriteLine(mdata.rating);
            }

            mdata.cover = webClient.DownloadData($"https://img.wuxiaworld.co/BookFiles/BookImages/{mdata.name.Replace(' ', '-').Replace('\'', '-')}.jpg");

            pageEnumerator.Reset();
            baseInfo.Clear();
            return mdata;
        }

        public Chapter[] GetChapterLinks(bool sort = false)
        {
            Dictionary<string, LinkedList<HtmlNode>> chapterInfo = pageEnumerator.GetElementsByClassNames(new string[] { "chapter-item" });
            IEnumerator<HtmlNode> a = chapterInfo["chapter-item"].GetEnumerator();
            Regex reg = new Regex("href=\"(.*?)\"");

            Chapter[] c = new Chapter[chapterInfo["chapter-item"].Count()];

            for (int idx = 0; idx < chapterInfo["chapter-item"].Count(); idx++)
            {
                a.MoveNext();
                c[idx] = new Chapter() { name = ((HtmlNode)a.Current).InnerText.Replace("\r\n", string.Empty), chapterLink = new Uri("https://www.wuxiaworld.co" + reg.Match(a.Current.OuterHtml).Groups[1].Value) };
            }
            reg = null;
            a = null;
            chapterInfo.Clear();

            return c;
        }
    }
}
