using ADLCore.Novels;
using ADLCore.Ext;
using ADLCore.Novels.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks.Sources;
using HtmlAgilityPack;
using ADLCore;
using ADLCore.Alert;
using System.Web;
using ADLCore.Constructs;
using ADLCore.Ext.ExtendedClasses;
using ADLCore.Video.Constructs;

namespace ADLCore.Novels.Downloaders
{
    /// <summary>
    /// WuxiaWorld.com
    /// </summary>
    public class cWuxiaWorld : DownloaderBase
    {
        public cWuxiaWorld(argumentList args, int taskIndex, Action<int, dynamic> act) : base(args, taskIndex, act)
        {
        }

        public override MetaData GetMetaData()
        {
            if (mdata != null)
                return mdata;

            pageEnumerator.Reset();

            Dictionary<string, LinkedList<HtmlNode>> baseInfo =
                pageEnumerator.GetElementsByClassNames(new string[] {"novel-body", "media-object"});

            mdata = new MetaData();
            this.mdata.url = this.url.ToString();

            string[] novelInfo = baseInfo["novel-body"].First().InnerText.DeleteFollowingWhiteSpaceA()
                .DeleteConDuplicate('\n').Split("\n");
            mdata.name = novelInfo[1];
            mdata.author = novelInfo[7];
            mdata.type = "nvl";
            mdata.genre = novelInfo[10];
            mdata.rating = "-1";
            mdata.LangType = MetaData.LangTypes.Translated;

            novelInfo = baseInfo["media-object"].First().OuterHtml.Split('\r');
            string x = Regex.Match(novelInfo[0], @"<img[^>]+src=""([^"">]+)""").Groups[1].Value;
            //x = x.Remove(x.IndexOf('?'));
            mdata.cover = webClient.DownloadData($"{x}.jpg");

            mdata.Downloader = this;
            mdata.Parent = this.thisBook;
            return EndMDataRoutine();
        }

        public override void GrabHome(int amount)
        {
            List<MetaData> MData = new List<MetaData>();
            MovePage("https://www.wuxiaworld.com/");


            var node = page.DocumentNode.SelectSingleNode("/html[1]/head[1]/script[14]");
            var b = new string(node.InnerText.Split("HOME = ")[1].Split("};")[0].ToArray()) + "}";
            JsonDocument jDoc = JsonDocument.Parse(b);
            for(int idx = 0; idx < amount; idx++)
            {
                var JsonElement = jDoc.RootElement.GetProperty("tags")[2].GetProperty("novels")[idx];
                MetaData obj = ParseFlexItem(JsonElement);
                MData.Add(obj);
                updateStatus?.Invoke(taskIndex, obj);
            }

            updateStatus?.Invoke(taskIndex, MData);
        }

        public override void GrabLinks(int[] range)
        {
            Dictionary<string, LinkedList<HtmlNode>> chapterInfo =
                pageEnumerator.GetElementsByClassNames(new string[] {"chapter-item"});
            IEnumerator<HtmlNode> a = chapterInfo["chapter-item"].GetEnumerator();
            Regex reg = new Regex("href=\"(.*?)\"");

            int x = range == null ? 0 : range[0];
            int y = range == null ? chapterInfo["chapter-item"].Count : range[1];
            Chapter[] c = new Chapter[y];

            for (int idx = x; idx < y; idx++)
            {
                a.MoveNext();
                var b = new Chapter(this)
                {
                    name = (a.Current).InnerText.Replace("\r\n", string.Empty).SkipCharSequence(new char[] {' '}),
                    chapterLink = new Uri("https://www.wuxiaworld.com" +
                                          reg.Match((a.Current).InnerHtml).Groups[1].Value)
                };
                updateStatus?.Invoke(taskIndex, b);
                c[idx] = b;
            }

            reg = null;
            a = null;
            chapterInfo.Clear();
            updateStatus?.Invoke(taskIndex, c);
        }

        public override dynamic Search(bool promptUser = false, bool grabAll = false)
        {
            throw new NotImplementedException();
        }

        MetaData ParseFlexItem(JsonElement flexNode)
        {
            MetaData mdata = new MetaData();
            mdata.url = $"https://www.wuxiaworld.com/novel/{flexNode.GetProperty("slug").GetString()}";
            mdata.coverPath = flexNode.GetProperty("coverUrl").GetString();
            mdata.name = flexNode.GetProperty("name").GetString();
            mdata.getCover = GetCover;
            return mdata;
        }

        public override Chapter[] GetChapterLinks(bool sort = false, int x = 0, int y = 0)
        {
            Dictionary<string, LinkedList<HtmlNode>> chapterInfo =
                pageEnumerator.GetElementsByClassNames(new string[] {"chapter-item"});
            IEnumerator<HtmlNode> a = chapterInfo["chapter-item"].GetEnumerator();
            Regex reg = new Regex("href=\"(.*?)\"");

            Chapter[] c = new Chapter[chapterInfo["chapter-item"].Count()];

            for (int idx = 0; idx < chapterInfo["chapter-item"].Count(); idx++)
            {
                a.MoveNext();
                c[idx] = new Chapter(this)
                {
                    name = (a.Current).InnerText.Replace("\r\n", string.Empty).SkipCharSequence(new char[] {' '}),
                    chapterLink = new Uri("https://www.wuxiaworld.com" +
                                          reg.Match((a.Current).InnerHtml).Groups[1].Value)
                };
            }

            reg = null;
            a = null;
            chapterInfo.Clear();

            return c;
        }

        public override TiNodeList GetText(Chapter chp, HtmlDocument use, AWebClient wc)
        {
            use.LoadHtml(Regex.Replace(wc.DownloadString(chp.chapterLink), "(<br>|<br/>)", "\n",
                RegexOptions.Singleline));
            GC.Collect();
            HtmlNode a = use.DocumentNode.SelectSingleNode("//*[@id=\"chapter-content\"]");
            HtmlNodeCollection aaab = use.DocumentNode.SelectNodes("//*[@dir=\"ltr\"]");
            List<HtmlNode> aa = new List<HtmlNode>();

            if (aaab != null)
                aa = aaab.ToList();
            else
            {
                use.LoadHtml(a.OuterHtml);
                aa = use.DocumentNode.SelectNodes("//p").ToList();
            }

            StringBuilder b = new StringBuilder();
            foreach (HtmlNode n in aa)
                b.Append(HttpUtility.HtmlDecode(Regex.Unescape(n.InnerText) +
                                                "\n\n")); //Decode items such as & and remove excessive new lines.
            string[] cnt = b.ToString().Split("\n");
            TiNodeList tnl = new TiNodeList();
            foreach (string str in cnt)
                tnl.push_back(new Epub.TiNode() {text = str});
            return tnl;
        }
    }
}