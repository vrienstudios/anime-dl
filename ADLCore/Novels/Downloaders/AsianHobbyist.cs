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
using ADLCore.Constructs;
using ADLCore.Ext.ExtendedClasses;

namespace ADLCore.Novels.Downloaders
{
    public class AsianHobbyist : DownloaderBase
    {
        public AsianHobbyist(argumentList args, int taskIndex, Action<int, dynamic> act) : base(args, taskIndex, act)
        {
        }

        public override dynamic GetMetaData() // MetaData done
        {
            if (mdata != null)
                return mdata;
            ADLUpdates.CallLogUpdate("Creating MetaData Object");
            pageEnumerator.Reset();

            Dictionary<string, LinkedList<HtmlNode>> baseInfo =
                pageEnumerator.GetElementsByClassNames(new string[] {"entry-title", "thumb"});

            mdata = new MetaData();
            this.mdata.url = this.url.ToString();

            mdata.name = baseInfo["entry-title"].First().InnerText;
            mdata.author = "www.asianhobbyist.com";
            mdata.type = "nvl";
            mdata.genre = "unknown";
            mdata.rating = "-1";
            mdata.LangType = MetaData.LangTypes.Translated;

            string x = baseInfo["thumb"].First().ChildNodes[1].Attributes["data-lazy-src"].Value;
            //x = x.Remove(x.IndexOf('?'));
            GenerateHeaders();
            mdata.cover = webClient.DownloadData(x);

            mdata.Downloader = this;
            mdata.Parent = this.thisBook;
            return EndMDataRoutine();
        }

        public static void grabHomeTest()
        {
            ADLCore.Novels.Downloaders.AsianHobbyist asian = new AsianHobbyist(
                new argumentList()
                {
                    term="https://www.asianhobbyist.com/",
                    mn="nvl",
                    d=true,
                }, 0, null
            );
            asian.GrabHome(1);
            Console.ReadLine();
        }
        public override void GrabHome(int amount)
        {
            List<MetaData> MData = new List<MetaData>();
            MovePage("https://www.asianhobbyist.com/");
            Dictionary<string, LinkedList<HtmlNode>> baseInfo =
                pageEnumerator.GetElementsByClassNames(new string[] {"latest-wrap"});
            var masterNode = baseInfo["latest-wrap"].First().FirstChild;
            for (int idx = 0; idx < amount; idx++)
            {
                MData.Add(ParseFlexItem(masterNode.ChildNodes[idx]));
                updateStatus?.Invoke(taskIndex, MData[idx]);
            }

            updateStatus?.Invoke(taskIndex, MData);
        }

        public override void GrabLinks(int[] range)
        {
            MovePage(mdata.url);
            HtmlNode[] asko = page.DocumentNode
                .SelectNodes("//div[contains(@class, 'tableBody')]/div[contains(@class, 'row')]/a").ToArray();
            Chapter[] c = new Chapter[range == null ? asko.Length : range[1] - range[0]];

            for (int idx = 0; idx < asko.Length; idx++)
            {
                var chp = new Chapter(this)
                    {name = $"Chp. {idx + 1}", chapterLink = new Uri(asko[idx].Attributes[1].Value)};
                c[idx] = chp;
                updateStatus?.Invoke(taskIndex, chp);
            }

            updateStatus?.Invoke(taskIndex, c);
        }

        public override dynamic Search(bool promptUser = false, bool grabAll = false)
        {
            throw new NotImplementedException();
        }

        MetaData ParseFlexItem(HtmlNode flexNode)
        {
            MetaData mdata = new MetaData();
            var details = flexNode.ChildNodes[1];
            mdata.name = details.ChildNodes[1].GetAttributeValue("alt", null);
            mdata.author = "AsianHobbyist";
            mdata.url = details.GetAttributeValue("href", null);
            mdata.coverPath = details.ChildNodes[1].GetAttributeValue("data-lazy-src", null);
            mdata.getCover = GetCover;
            return mdata;
        }

        public override Chapter[] GetChapterLinks(bool sort = false, int x = 0, int y = 0)
        {
            MovePage(mdata.url);
            HtmlNode[] asko = page.DocumentNode
                .SelectNodes("//div[contains(@class, 'tableBody')]/div[contains(@class, 'row')]/a").ToArray();
            Chapter[] c = new Chapter[asko.Length];

            for (int idx = 0; idx < asko.Length; idx++)
                c[idx] = new Chapter(this)
                    {name = $"Chp. {idx + 1}", chapterLink = new Uri(asko[idx].Attributes[1].Value)};

            return c;
        }

        public override TiNodeList GetText(Chapter chp, HtmlDocument use, AWebClient wc)
        {
            //TODO: Needs special formatting due to @NBSP
            MovePage(chp.chapterLink.ToString());
            HtmlNode[] asko = page.DocumentNode.SelectNodes("//div[contains(@class, 'entry-content')]").ToArray();
            StringBuilder sb = new StringBuilder();
            foreach (HtmlNode n in asko)
                sb.Append(n.InnerText);
            string[] cnt = HttpUtility.HtmlDecode(sb.ToString()).Split("\n");
            TiNodeList tnl = new TiNodeList();
            foreach (string str in cnt)
                tnl.push_back(new Epub.TiNode() {text = str});
            return tnl;
        }
    }
}