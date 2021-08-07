using ADLCore.Alert;
using ADLCore.Ext;
using ADLCore.Interfaces;
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
    public class cNovelFull : DownloaderBase
    {
        public cNovelFull(argumentList args, int taskIndex, Action<int, string> act) : base(args, taskIndex, act)
        {

        }

        public override MetaData GetMetaData()
        {
            if (mdata != null)
                return mdata;
            ADLUpdates.CallLogUpdate("Creating MetaData Object");
            pageEnumerator.Reset();

            Dictionary<string, LinkedList<HtmlNode>> baseInfo = pageEnumerator.GetElementsByClassNames(new string[] { "title", "info", "book"});

            mdata = new MetaData();
            this.mdata.url = this.url.ToString();

            mdata.name = baseInfo["title"].First().InnerText;
            string[] sp = baseInfo["info"].First().InnerText.Split(":");
            mdata.author = sp[1].Replace("Genre", string.Empty);
            mdata.type = "nvl";
            mdata.genre = sp[2];
            mdata.rating = "-1";

            string x = $"http://{url.Host}{Regex.Match(baseInfo["book"].First().OuterHtml, @"<img[^>]+src=""([^"">]+)""").Groups[1].Value}";
            //x = x.Remove(x.IndexOf('?'));
            GenerateHeaders();
            mdata.cover = webClient.DownloadData(x);

            return EndMDataRoutine();
        }


        public override Chapter[] GetChapterLinks(bool sort = false)
        {
            int idx = 0;
            List<Chapter> chaps = new List<Chapter>();
            Regex reg = new Regex("href=\"(.*?)\"");
            ADLUpdates.CallLogUpdate($"Getting Chapter Links for {mdata.name}");
            while (true)
            {
                idx++;
                MovePage($"{mdata.url}?page={idx.ToString()}&per-page=50"); // limited to 50
                Dictionary<string, LinkedList<HtmlNode>> chapterInfo = pageEnumerator.GetElementsByClassNames(new string[] { "list-chapter" });

                if (chapterInfo["list-chapter"].Count <= 0)
                    break;

                IEnumerator<HtmlNode> a = chapterInfo["list-chapter"].GetEnumerator();
                while (a.MoveNext())
                {
                    LoadPage(a.Current.InnerHtml);
                    foreach (HtmlNode ele in page.DocumentNode.SelectNodes("//li"))
                    {
                        Chapter ch = new Chapter(this) { name = ele.InnerText.SkipCharSequence(new char[] { ' ' }), chapterLink = new Uri("https://" + url.Host + reg.Match(ele.InnerHtml).Groups[1].Value) };
                        if (chaps.Where(x => x.chapterLink == ch.chapterLink).Count() == 0)
                            chaps.Add(ch);
                        else
                            goto exit;

                    }
                }
            }
            exit:
            ADLUpdates.CallLogUpdate($"Found {chaps.Count} Chapters for {mdata.name}");
            sU(taskIndex, $"Got MetaData Object for {mdata.name} by {mdata.author}");
            return chaps.ToArray();
        }

        public override TiNodeList GetText(Chapter chp, HtmlDocument use, WebClient wc)
        {
            HtmlNodeCollection b;
            StringBuilder sb = new StringBuilder();

            // Git controls in visual studio are fucking horrible, and I had to rewrite this TWICE. Only if Git Bash wasn't being deprecated...
            wc.Headers = IAppBase.GenerateHeaders(chp.chapterLink.Host);
            string dwnld;
        Retry:;
            try
            {
                dwnld = wc.DownloadString(chp.chapterLink);
            }
            catch
            {
                goto Retry;
            }
            use.LoadHtml(dwnld);
            b = use.DocumentNode.SelectNodes("//div[contains(@class, 'chapter-c')]");

            HtmlNode[] scripts = b[0].DescendantNodes().Where(x => x.XPath.Contains("/script")).ToArray();

            foreach (HtmlNode n in scripts)
                n.RemoveAll();

            use.LoadHtml(b[0].InnerHtml);

            b = use.DocumentNode.SelectNodes("//text()[normalize-space(.) != '']");

            foreach (HtmlNode htmln in b)
                sb.AppendLine(htmln.InnerText + "\n");

            GC.Collect();
            string[] cnt = HttpUtility.HtmlDecode(sb.ToString()).Split("\n");
            TiNodeList tnl = new TiNodeList();
            foreach (string str in cnt)
                tnl.push_back(new Epub.TiNode() { text = str });
            return tnl;
        }

        public override dynamic Get(HentaiVideo obj, bool dwnld)
        {
            throw new NotImplementedException();
        }
    }
}
