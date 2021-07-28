using ADLCore;
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

namespace ADLCore.Novels.Downloaders
{
    public class cScribbleHub : DownloaderBase
    {
        public cScribbleHub(argumentList args, int taskIndex, Action<int, string> act) : base(args, taskIndex, act)
        {

        }

        public override MetaData GetMetaData()
        {
            if (mdata != null)
                return mdata;

            pageEnumerator.Reset();

            Dictionary<string, LinkedList<HtmlNode>> baseInfo = pageEnumerator.GetElementsByClassNames(new string[] { "fic_title", "auth_name_fic", "fic_image", "fic_genre" });

            mdata = new MetaData();
            this.mdata.url = this.url.ToString();

            mdata.name = baseInfo["fic_title"].First().InnerText;
            mdata.author = baseInfo["auth_name_fic"].First().InnerText;
            mdata.type = "unknown";
            mdata.genre = baseInfo["fic_genre"].First().InnerText;
            mdata.rating = "-1";

            string x = Regex.Match(baseInfo["fic_image"].First().OuterHtml, @"<img[^>]+src=""([^"">]+)""").Groups[1].Value;
            //x = x.Remove(x.IndexOf('?'));
            GenerateHeaders();
            mdata.cover = webClient.DownloadData(x);

            pageEnumerator.Reset();
            baseInfo.Clear();
            ADLUpdates.CallLogUpdate($"Got MetaData Object for {mdata.name} by {mdata.author}");
            sU(taskIndex, $"Got MetaData Object for {mdata.name} by {mdata.author}");
            return mdata;
        }

        public override Chapter[] GetChapterLinks(bool sort = false)
        {
            int idx = 0;
            List<Chapter> chaps = new List<Chapter>();
            Regex reg = new Regex("href=\"(.*?)\"");
            //Continuously move table -> and gather links.
            while (true)
            {
                idx++;
                MovePage($"{mdata.url}?toc={idx.ToString()}#content1");
                Dictionary<string, LinkedList<HtmlNode>> chapterInfo = pageEnumerator.GetElementsByClassNames(new string[] { "toc_a" });

                if (chapterInfo["toc_a"].Count <= 0)
                    break;

                IEnumerator<HtmlNode> a = chapterInfo["toc_a"].GetEnumerator();
                while (a.MoveNext())
                    chaps.Add(new Chapter(this) { name = a.Current.InnerText, chapterLink = new Uri(reg.Match(a.Current.OuterHtml).Groups[1].Value) });

            }
            chaps.Reverse();
            return chaps.ToArray();
        }

        public override string GetText(Chapter chp, HtmlDocument use, WebClient wc)
        {
            wc.Headers = IAppBase.GenerateHeaders(chp.chapterLink.Host);
            string dwnld = wc.DownloadString(chp.chapterLink);
            use.LoadHtml(dwnld);
            GC.Collect();
            return use.DocumentNode.FindAllNodes().GetFirstElementByClassNameA("chp_raw").InnerText;
        }

        public override dynamic Get(HentaiVideo obj, bool dwnld)
        {
            throw new NotImplementedException();
        }
    }
}
