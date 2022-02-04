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
using ADLCore.Epub;
using ADLCore.Ext.ExtendedClasses;

namespace ADLCore.Novels.Downloaders
{
    public class cScribbleHub : DownloaderBase
    {
        //TODO: Use POST to get site to circumvent CloudFlare
        public cScribbleHub(argumentList args, int taskIndex, Action<int, dynamic> act) : base(args, taskIndex, act)
        {
            
        }

        public override MetaData GetMetaData()
        {
            if (mdata != null)
                return mdata;

            pageEnumerator.Reset();

            Dictionary<string, LinkedList<HtmlNode>> baseInfo =
                pageEnumerator.GetElementsByClassNames(new string[]
                    {"fic_title", "auth_name_fic", "fic_image", "fic_genre"});

            mdata = new MetaData();
            this.mdata.url = this.url.ToString();

            mdata.name = baseInfo["fic_title"].First().InnerText;
            mdata.author = baseInfo["auth_name_fic"].First().InnerText;
            mdata.type = "nvl";
            mdata.genre = baseInfo["fic_genre"].First().InnerText;
            mdata.rating = "-1";
            mdata.LangType = MetaData.LangTypes.Original;

            string x = Regex.Match(baseInfo["fic_image"].First().OuterHtml, @"<img[^>]+src=""([^"">]+)""").Groups[1]
                .Value;
            //x = x.Remove(x.IndexOf('?'));
            GenerateHeaders();
            mdata.cover = webClient.DownloadData(x);

            mdata.Downloader = this;
            mdata.Parent = this.thisBook;
            return EndMDataRoutine();
        }
        
        public override void GrabHome(int amount)
        {
            List<MetaData> MData = new List<MetaData>();
            MovePage("https://www.scribblehub.com/");
            Dictionary<string, LinkedList<HtmlNode>> baseInfo =
                pageEnumerator.GetElementsByClassNames(new string[] {"new-novels-carousel"});
            var masterNode = baseInfo["new-novels-carousel"].First().ChildNodes.Where(x => x.Name == "div").ToArray();
            for(int idx = 0; idx < amount; idx++)
            {
                MetaData obj = ParseFlexItem(masterNode[idx]);
                MData.Add(obj);
                updateStatus?.Invoke(taskIndex, obj);
            }

            updateStatus?.Invoke(taskIndex, MData);
        }

        public override void GrabLinks(int[] range)
        {
            Chapter[] cLinks = GetChapterLinks();
            for (; range[0] < range[1]; range[0]++)
                updateStatus?.Invoke(taskIndex, cLinks[range[0]]);
            updateStatus?.Invoke(taskIndex, cLinks);
        }

        MetaData ParseFlexItem(HtmlNode flexNode)
        {
            MetaData mdata = new MetaData();
            var cover = flexNode.ChildNodes[3];
            var info = flexNode.ChildNodes[7];
            mdata.url = cover.GetAttributeValue("href", null);
            mdata.coverPath = cover.FirstChild.GetAttributeValue("src", null);
            mdata.name = info.FirstChild.GetAttributeValue("title", null);
            mdata.getCover = GetCover;
            return mdata;
        }
        
        public override Chapter[] GetChapterLinks(bool sort = false, int x = 0, int y = 0)
        {
            int idx = 0;
            List<Chapter> chaps = new List<Chapter>();
            Regex reg = new Regex("href=\"(.*?)\"");
            //Continuously move table -> and gather links.
            //TODO: Implement start point functionaltiy as well for ScribbleHub

            if (x == y)
                while (GetChapterLink(ref chaps, reg, idx)) //spinwait
                {
                }
            else
                while (chaps.Count <= y && GetChapterLink(ref chaps, reg, idx))
                {
                }

            chaps.Reverse();
            return chaps.ToArray();
        }

        private bool GetChapterLink(ref List<Chapter> chaps, Regex reg, int idx = 0)
        {
            idx++;
            MovePage($"{mdata.url}?toc={idx.ToString()}#content1");
            Dictionary<string, LinkedList<HtmlNode>> chapterInfo =
                pageEnumerator.GetElementsByClassNames(new string[] {"toc_a"});

            if (chapterInfo["toc_a"].Count <= 0)
                return false;

            using IEnumerator<HtmlNode> a = chapterInfo["toc_a"].GetEnumerator();
            while (a.MoveNext())
                chaps.Add(new Chapter(this)
                {
                    name = a.Current.InnerText, chapterLink = new Uri(reg.Match(a.Current.OuterHtml).Groups[1].Value)
                });
            return true;
        }

        public override TiNodeList GetText(Chapter chp, HtmlDocument use, AWebClient wc)
        {
            wc.Headers = IAppBase.GenerateHeaders(chp.chapterLink.Host);
            string dwnld = wc.DownloadString(chp.chapterLink);
            use.LoadHtml(dwnld);
            GC.Collect();
            string[] cnt = use.DocumentNode.FindAllNodes().GetFirstElementByClassNameA("chp_raw").InnerText.Split("\n");
            TiNodeList tnl = new TiNodeList();
            foreach (string str in cnt)
                tnl.push_back(new Epub.TiNode() {text = str});
            return tnl;
        }

        public override dynamic Get(HentaiVideo obj, bool dwnld)
        {
            throw new NotImplementedException();
        }
    }
}