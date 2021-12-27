using ADLCore.Ext;
using ADLCore.Novels.Models;
using ADLCore.Video.Constructs;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using ADLCore.Ext.ExtendedClasses;

namespace ADLCore.Novels.Downloaders
{
    class VolareNovels : DownloaderBase
    {
        public VolareNovels(argumentList args, int taskIndex, Action<int, dynamic> act) : base(args, taskIndex, act)
        {
        }

        public override dynamic Get(HentaiVideo obj, bool dwnld)
        {
            throw new NotImplementedException();
        }

        public override void GrabHome(int amount)
        {
            List<MetaData> MData = new List<MetaData>();
            MovePage("https://www.volarenovels.com/");

            Dictionary<string, LinkedList<HtmlNode>> baseInfo =
                pageEnumerator.GetElementsByClassNames(new string[] {"col-md-12"});
            var masterNode = baseInfo["col-md-12"].ToArray()[1].ChildNodes[1].ChildNodes.Where(x => x.Name == "div").ToArray()[1].ChildNodes.Where(x => x.Name == "div").ToArray();
            for(int idx = 0; idx < amount; idx++)
            {
                var el = masterNode[idx];
                MetaData obj = ParseFlexItem(el);
                MData.Add(obj);
                updateStatus?.Invoke(taskIndex, obj);
            }

            updateStatus?.Invoke(taskIndex, MData);
        }

        public override void GrabLinks(int[] range)
        {
            throw new NotImplementedException();
        }

        MetaData ParseFlexItem(HtmlNode nosotrosNode)
        {
            MetaData mdata = new MetaData();
            var aTag = nosotrosNode.ChildNodes.First(x => x.Name == "a");
            mdata.coverPath = aTag.ChildNodes.First(x => x.Name == "img").GetAttributeValue("data-src", null);
            mdata.url = "https://volarnovels.com" + aTag.GetAttributeValue("href", null);
            mdata.name = nosotrosNode.ChildNodes.First(x => x.Name == "p").InnerText;
            mdata.getCover = GetCover;
            return mdata;
        }

        public override Chapter[] GetChapterLinks(bool sort = false, int x = 0, int y = 0)
        {
            Dictionary<string, LinkedList<HtmlNode>> kvp =
                pageEnumerator.GetElementsByClassNames(new string[] {"chapter-item"});
            List<Chapter> chapters = new List<Chapter>();

            void GetChaptersFromChapterNode(HtmlNode pane)
                => chapters.Add(new Chapter(this)
                {
                    name = pane.ChildNodes.First(x => x.Name == "a").ChildNodes.First(x => x.Name == "span").InnerText,
                    chapterLink = new Uri("https://www.volarenovels.com" + pane.ChildNodes.First(x => x.Name == "a")
                        .Attributes.First(x => x.Name == "href").Value)
                });

            foreach (HtmlNode nodes in kvp["chapter-item"])
                GetChaptersFromChapterNode(nodes);

            return chapters.ToArray();
        }

        public override MetaData GetMetaData()
        {
            if (mdata != null)
                return mdata;

            pageEnumerator.Reset(); // Unknown state, reset
            Dictionary<string, LinkedList<HtmlNode>> kvp =
                pageEnumerator.GetElementsByClassNames(new string[] {"p-tb-10-rl-30", "m-tb-30", "tab-content"});
            mdata = new MetaData();

            HtmlNode node1 = kvp["p-tb-10-rl-30"].First.Value;
            mdata.name = node1.ChildNodes.First(x => x.Name == "h3").InnerText;
            mdata.author = node1.ChildNodes.First(x => x.Name == "p").ChildNodes.First(x => x.Name == "#text")
                .InnerText;
            List<HtmlNode> nodes = node1.ChildNodes.Where(x => x.Name == "div").ToList();

            for (int idx = 1; idx < nodes[1].ChildNodes.Count; idx++)
                mdata.genre += nodes[1].ChildNodes[idx].InnerText.RemoveSpecialCharacters();

            string JoinP(HtmlNode[] a)
            {
                StringBuilder sb = new StringBuilder();
                foreach (HtmlNode node in a)
                    sb.Append(node.InnerText.NonSafeRemoveSpecialCharacters() + " ");
                return sb.ToString();
            }

            mdata.description = JoinP(kvp["tab-content"].First.Value.ChildNodes.First(x => x.Id == "Details").ChildNodes
                .Where(x => x.Name == "p").ToArray());
            mdata.rating = "-1";
            mdata.type = "nvl";
            string img = kvp["m-tb-30"].First.Value.Attributes.First(x => x.Name == "src").Value;
            mdata.cover = webClient.DownloadData(img);
            return EndMDataRoutine();
        }

        public override TiNodeList GetText(Chapter chp, HtmlDocument use, AWebClient wc)
        {
            MovePage(chp.chapterLink.OriginalString);
            Dictionary<string, LinkedList<HtmlNode>> dict =
                pageEnumerator.GetElementsByClassNames(new string[] {"jfontsize_content"});
            List<HtmlNode> nodes;
            List<string> strings = new List<string>();
            nodes = dict["jfontsize_content"].First.Value.ChildNodes.Where(x => x.Name == "p").ToList();
            foreach (HtmlNode nod in nodes)
                strings.Add(HttpUtility.HtmlDecode(nod.InnerText));
            TiNodeList tnl = new TiNodeList();
            foreach (string str in strings)
                tnl.push_back(new Epub.TiNode() {text = str});
            return tnl;
        }
    }
}