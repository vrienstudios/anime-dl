using ADLCore;
using ADLCore.Alert;
using ADLCore.Ext;
using ADLCore.Novels;
using ADLCore.Novels.Models;
using ADLCore.Video.Constructs;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace ADLCore.Novels.Downloaders
{
    /// <summary>
    /// WuxiaWorld.co
    /// </summary>
    public class dWuxiaWorld : DownloaderBase
    {
        public dWuxiaWorld(argumentList args, int taskIndex, Action<int, string> act) : base(args, taskIndex, act)
        {

        }

        public override MetaData GetMetaData()
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
                mdata.author = baseInfo["author"].First().InnerText.SkipPreceedingAndChar(':').Sanitize();
                mdata.type = baseInfo["book-state"].First().InnerText.SkipPreceedingAndChar(' ').DeleteFollowingWhiteSpaceA().Sanitize();
                mdata.genre = baseInfo["book-catalog"].First().InnerText.DeleteFollowingWhiteSpaceA().Sanitize();
                mdata.rating = baseInfo["score"].First().InnerText.Sanitize();
            } catch  {
                updateStatus(taskIndex, "Failed to load some values, failed");
            }

            mdata.cover = webClient.DownloadData($"https://img.wuxiaworld.co/BookFiles/BookImages/{mdata.name.Replace(' ', '-').Replace('\'', '-')}.jpg");

            pageEnumerator.Reset();
            baseInfo.Clear();
            ADLUpdates.CallUpdate($"Got MetaData Object for {mdata.name} by {mdata.author}", false);
            return mdata;
        }

        public override Chapter[] GetChapterLinks(bool sort = false)
        {
            Dictionary<string, LinkedList<HtmlNode>> chapterInfo = pageEnumerator.GetElementsByClassNames(new string[] { "chapter-item" });
            IEnumerator<HtmlNode> a = chapterInfo["chapter-item"].GetEnumerator();
            Regex reg = new Regex("href=\"(.*?)\"");

            Chapter[] c = new Chapter[chapterInfo["chapter-item"].Count()];

            for (int idx = 0; idx < chapterInfo["chapter-item"].Count(); idx++)
            {
                a.MoveNext();
                c[idx] = new Chapter(this) { name = (a.Current).InnerText.Replace("\r\n", string.Empty).SkipCharSequence(new char[] { ' ' }), chapterLink = new Uri("https://www.wuxiaworld.co" + reg.Match(a.Current.OuterHtml).Groups[1].Value) };
            }
            reg = null;
            a = null;
            chapterInfo.Clear();

            return c;
        }

        public override string GetText(Chapter chp, HtmlDocument use, WebClient wc)
        {
            use.LoadHtml(Regex.Replace(wc.DownloadString(chp.chapterLink), "(<br>|<br/>)", "\n", RegexOptions.Singleline));
            GC.Collect();
            return HttpUtility.HtmlDecode(Regex.Unescape(use.DocumentNode.FindAllNodes().GetFirstElementByClassNameA("chapter-entity").InnerText));
        }

        public override dynamic Get(HentaiVideo obj, bool dwnld)
        {
            throw new NotImplementedException();
        }
    }
}
