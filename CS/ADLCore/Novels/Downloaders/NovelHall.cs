using ADLCore.Alert;
using ADLCore.Ext;
using ADLCore.Novels.Models;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace ADLCore.Novels.Downloaders
{
    public class NovelHall : DownloaderBase
    {
        public NovelHall(string url, int taskIndex, Action<int, string> act) : base(url, taskIndex, act)
        {

        }

        /// <summary>
        /// Get general information about the novel, cover, title, author, etc
        /// </summary>
        /// <returns></returns>
        public override MetaData GetMetaData()
        {
            if (mdata != null)
                return mdata;

            pageEnumerator.Reset();
            Dictionary<string, LinkedList<HtmlNode>> baseInfo = pageEnumerator.GetElementsByClassNames(new string[] { "book-img", "book-info", "total" });

            HtmlNode[] t = baseInfo["total"].First().SelectNodes("//span[@class=\"blue\"]").ToArray();
            HtmlNode[] to = baseInfo["total"].First().SelectNodes("//a[@class=\"red\"]").ToArray();
            mdata = new MetaData();
            this.mdata.url = this.url.ToString();
            try
            {
                mdata.name = baseInfo["book-info"].First().SelectSingleNode("//h1").InnerText;
                mdata.author = t[0].InnerText;
                mdata.type = t[1].InnerText;
                mdata.genre = to[0].InnerText;
                mdata.rating = " ";
            }
            catch
            {
                updateStatus(taskIndex, "Failed to load some values, failed");
            }

            mdata.cover = webClient.DownloadData(baseInfo["book-img"].First().SelectNodes("//img/@src").ToArray()[1].Attributes.ToArray()[0].Value);

            pageEnumerator.Reset();
            baseInfo.Clear();
            ADLUpdates.CallUpdate($"Got MetaData Object for {mdata.name} by {mdata.author}", false);
            return mdata;
        }

        public override Chapter[] GetChapterLinks(bool sort = false)
        {
            Dictionary<string, LinkedList<HtmlNode>> chapterInfo = pageEnumerator.GetElementsByClassNames(new string[] { "book-catalog" });
            HtmlNode[] n = chapterInfo["book-catalog"].First().SelectNodes("//div[@id=\"morelist\"]//li").ToArray();
            Chapter[] c = new Chapter[n.Length];
            for (int idx = 0; idx < n.Length; idx++)
            {
                c[idx] = new Chapter(this) { name = n[idx].InnerText.Replace("\n", string.Empty).SkipCharSequence(new char[] { ' ' }), chapterLink = new Uri("https://www.novelhall.com" + n[idx].ChildNodes[1].Attributes.First().Value) };
            }
            chapterInfo.Clear();
            return c;
        }

        public override string GetText(Chapter chp, HtmlDocument use, WebClient wc)
        {
            use.LoadHtml(Regex.Replace(wc.DownloadString(chp.chapterLink), "(<br>|<br/>|<br />)", "\n", RegexOptions.None));
            GC.Collect();
            return use.DocumentNode.FindAllNodes().GetFirstElementByClassNameA("entry-content").InnerText;
        }
    }
}
