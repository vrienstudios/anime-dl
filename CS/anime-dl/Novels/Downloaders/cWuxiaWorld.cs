using anime_dl.Novels;
using anime_dl.Ext;
using anime_dl.Novels.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace KobeiD.Downloaders
{
    /// <summary>
    /// WuxiaWorld.com
    /// </summary>
    class cWuxiaWorld : DownloaderBase
    {
        public cWuxiaWorld(string url) : base(url)
        {

        }

        public MetaData GetMetaData()
        {
            if (mdata != null)
                return mdata;

            pageEnumerator.Reset();

            Dictionary<string, LinkedList<HtmlNode>> baseInfo = pageEnumerator.GetElementsByClassNames(new string[] { "novel-body",  "media-object img-thumbnail"});

            mdata = new MetaData();
            this.mdata.url = this.url.ToString();

            string[] novelInfo = baseInfo["novel-body"].First().InnerText.DeleteFollowingWhiteSpaceA().Split(new string[] { "\n", "\r", "\r\n", "\n\r" }, StringSplitOptions.None);
            mdata.name = novelInfo[0];
            mdata.author = novelInfo[10];
            mdata.type = "unknown";
            mdata.genre = novelInfo[16];
            mdata.rating = "-1";

            novelInfo = baseInfo["media-object img-thumbnail"].First().OuterHtml.Split('\r');
            string x = Regex.Match(novelInfo[0], @"<img[^>]+src=""([^"">]+)""").Groups[1].Value;
            //x = x.Remove(x.IndexOf('?'));
            mdata.cover = webClient.DownloadData($"{x}.jpg");

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
                c[idx] = new Chapter() { name = (a.Current).InnerText.Replace("\r\n", string.Empty), chapterLink = new Uri("https://www.wuxiaworld.com" + reg.Match((a.Current).InnerHtml).Groups[1].Value) };
            }
            reg = null;
            a = null;
            chapterInfo.Clear();

            return c;
        }
    }
}
