using ADLCore.Alert;
using ADLCore.Ext;
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
    public class AsianHobbyist : DownloaderBase
    {
        public AsianHobbyist(argumentList args, int taskIndex, Action<int, string> act) : base(args, taskIndex, act)
        {

        }

        public override MetaData GetMetaData() // MetaData done
        {
            if (mdata != null)
                return mdata;
            ADLUpdates.CallUpdate("Creating MetaData Object", false);
            pageEnumerator.Reset();

            Dictionary<string, LinkedList<HtmlNode>> baseInfo = pageEnumerator.GetElementsByClassNames(new string[] { "entry-title", "thumb" });

            mdata = new MetaData();
            this.mdata.url = this.url.ToString();

            mdata.name = baseInfo["entry-title"].First().InnerText;
            mdata.author = "www.asianhobbyist.com";
            mdata.type = "uknown";
            mdata.genre = "unknown";
            mdata.rating = "-1";

            string x = baseInfo["thumb"].First().ChildNodes[1].Attributes["data-lazy-src"].Value;
            //x = x.Remove(x.IndexOf('?'));
            GenerateHeaders();
            mdata.cover = webClient.DownloadData(x);

            pageEnumerator.Reset();
            baseInfo.Clear();
            ADLUpdates.CallUpdate($"Got MetaData Object for {mdata.name} by {mdata.author}", false);
            return mdata;
        }


        public override Chapter[] GetChapterLinks(bool sort = false)
        {
            MovePage(mdata.url);
            HtmlNode[] asko = page.DocumentNode.SelectNodes("//div[contains(@class, 'tableBody')]/div[contains(@class, 'row')]/a").ToArray();
            Chapter[] c = new Chapter[asko.Length];

            for(int idx = 0; idx < asko.Length; idx++)
                c[idx] = new Chapter(this) { name = $"Chp. {idx + 1}", chapterLink = new Uri(asko[idx].Attributes[1].Value) };

            return c;
        }

        public override string GetText(Chapter chp, HtmlDocument use, WebClient wc)
        {
            MovePage(chp.chapterLink.ToString());
            HtmlNode[] asko = page.DocumentNode.SelectNodes("//div[contains(@class, 'entry-content')]").ToArray();
            StringBuilder sb = new StringBuilder();
            foreach (HtmlNode n in asko)
                sb.Append(n.InnerText);
            return HttpUtility.HtmlDecode(sb.ToString());
        }

        public override dynamic Get(HentaiVideo obj, bool dwnld)
        {
            throw new NotImplementedException();
        }
    }
}
