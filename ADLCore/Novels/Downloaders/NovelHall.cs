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
using System.Threading;
using System.Web;
using ADLCore.Ext.ExtendedClasses;
using ADLCore.Interfaces;

namespace ADLCore.Novels.Downloaders
{
    public class NovelHall : DownloaderBase
    {
        public NovelHall(argumentList args, int taskIndex, Action<int, dynamic> act) : base(args, taskIndex, act)
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

            Dictionary<string, LinkedList<HtmlNode>> baseInfo =
                pageEnumerator.GetElementsByClassNames(new string[] {"book-img", "book-info", "total", "intro"});

            HtmlNode[] t = baseInfo["total"].First().SelectNodes("//span[@class=\"blue\"]").ToArray();
            HtmlNode[] to = baseInfo["total"].First().SelectNodes("//a[@class=\"red\"]").ToArray();
            mdata = new MetaData();
            this.mdata.url = this.url.ToString();
            
            string status = getNode(t, "Status", 0).InnerText.Split('：')[1].Skip(1).ToString();
            string description;
            HtmlNode getNode(HtmlNode[] nodes, string substr, int start)
            {
                for (int i = 0; i < nodes.Length; i++)
                {
                    if (nodes[i].InnerText.Length > substr.Length)
                        if (nodes[i].InnerText.Substring(start, substr.Length) == substr)
                            return nodes[i];
                }

                return null;
            }
            try
            {
                mdata.name = baseInfo["book-info"].First().SelectSingleNode("//h1").InnerText;
                mdata.author = t[0].ChildNodes[0].InnerText
                    .SkipCharSequence(new char[] {'A', 'u', 't', 'h', 'o', 'r', '：'});
                mdata.type = "nvl";
                mdata.genre = to[0].InnerText;
                mdata.rating = " ";
                mdata.LangType = MetaData.LangTypes.Mixed;
                mdata.ParseStatus(status);
                
                mdata.description = baseInfo["intro"].First().InnerText;
                
                Dictionary<string, string> dictRepl = new Dictionary<string, string>()
                {
                    {"\t", string.Empty},
                    {"\n", string.Empty},
                    {"\r", string.Empty},
                };
                
                foreach (string str in dictRepl.Keys)
                    mdata.description = mdata.description.Replace(str, dictRepl[str]);
            }
            catch
            {
                updateStatus(taskIndex, "Failed to load some values, failed");
            }

            string uri = baseInfo["book-img"].First().SelectNodes("//img/@src").ToArray()[1].Attributes.ToArray()[0]
                .Value;
            try
            {
                mdata.cover = webClient.DownloadData(uri);
            }
            catch(Exception e)
            {
                mdata.cover =
                    webClient.DownloadData(
                        "https://image.shutterstock.com/image-vector/continuous-one-line-drawing-open-600w-1489544150.jpg");
            }

            mdata.Downloader = this;
            return EndMDataRoutine();
        }

        public override void GrabHome(int amount)
        {
            List<MetaData> MData = new List<MetaData>();
            MovePage("https://www.novelhall.com/");

            Dictionary<string, LinkedList<HtmlNode>> baseInfo =
                pageEnumerator.GetElementsByClassNames(new string[] {"section1"});
            var masterNode = baseInfo["section1"].First().ChildNodes[3].ChildNodes.Where(x => x.Name == "li").ToArray();
            for(int idx = 0; idx < amount; idx++)
            {
                var el = masterNode[idx];
                MetaData obj = ParseFlexItem(el);
                if (ao.imgDefault)
                    obj.cover = Main.imageConverter == null ? GetCover(obj) : Main.imageConverter(GetCover(obj));
                MData.Add(obj);
                updateStatus?.Invoke(taskIndex, obj);
            }

            updateStatus?.Invoke(taskIndex, MData);
        }

        public override void GrabLinks(int[] range)
        {
            Dictionary<string, LinkedList<HtmlNode>> chapterInfo =
                pageEnumerator.GetElementsByClassNames(new string[] {"book-catalog"});
            HtmlNode[] n = chapterInfo["book-catalog"].First().SelectNodes("//div[@id=\"morelist\"]//li").ToArray();
            Chapter[] c = new Chapter[range == null ? n.Length : range[1] - range[0]];
            
            int x = range == null ? 0 : range[0];
            int y = range == null ? n.Length : range[1];
            
            for (int idx = x; idx < y; idx++)
            {
                var b = new Chapter(this)
                {
                    name = n[idx].InnerText.Replace("\n", string.Empty).SkipCharSequence(new char[] {' '}),
                    chapterLink = new Uri("https://www.novelhall.com" + n[idx].ChildNodes[1].Attributes.First().Value)
                };
                updateStatus?.Invoke(taskIndex, b);
                c[idx] = b;
            }

            chapterInfo.Clear();
            updateStatus?.Invoke(taskIndex, c);
        }

        public override dynamic Search(bool promptUser = false, bool grabAll = false)
        {
            //https://www.novelhall.com/index.php?s=so&module=book&keyword= (' ', +)
            MovePage($"https://www.novelhall.com/index.php?s=so&module=book&keyword={this.ao.term.Replace(' ', '+')}");
            
            Dictionary<string, LinkedList<HtmlNode>> dict = mshtml.GetElementsByClassNames(pageEnumerator, new string[] {"container"});
            var vosotrosNode = dict.First().Value.ToArray()[2];
            var tableBody = vosotrosNode.ChildNodes.First(x => x.Name == "div").ChildNodes
                .First(x => x.Name == "table").ChildNodes.First(x => x.Name == "tbody");
            if (!grabAll)
            {
                var main = tableBody.ChildNodes.First(x => x.Name == "tr").ChildNodes.Where(x => x.Name == "td").ToArray()[1];
            
                ao.term = $"https://www.novelhall.com{main.ChildNodes.First(x => x.Name == "a").GetAttributeValue("href", "nll")}";
                this.MovePage(ao.term);
                this.url = new Uri(ao.term);
                return this as DownloaderBase;
            }
            else
            {
                List<DownloaderBase> downloaders = new List<DownloaderBase>();
                var SearchResults = tableBody.ChildNodes.Where(x => x.Name == "tr").ToArray();
                foreach(HtmlNode node in SearchResults)
                {
                    var uri = node.ChildNodes.First(x => x.Name == "tr").ChildNodes.Where(x => x.Name == "td").ToArray()[1].ChildNodes.First(x => x.Name == "a").GetAttributeValue("href", "nll");
                    DownloaderBase _base = new NovelHall(new argumentList(){ term = uri }, 0, null);
                    downloaders.Add(_base);
                }

                return downloaders;
            }
        }

        MetaData ParseFlexItem(HtmlNode nosotrosNode)
        {
            MetaData mdata = new MetaData();
            mdata.coverPath = nosotrosNode.ChildNodes[1].ChildNodes[1].FirstChild.GetAttributeValue("src", null);
            mdata.url = "https://www.novelhall.com" + nosotrosNode.ChildNodes[1].ChildNodes[1].GetAttributeValue("href", null);
            mdata.name = nosotrosNode.ChildNodes[1].ChildNodes[1].FirstChild.GetAttributeValue("alt", null);
            mdata.author = nosotrosNode.ChildNodes[3].ChildNodes[5].ChildNodes[1].ChildNodes[1].InnerText;
            mdata.getCover = GetCover;
            return mdata;
        }
        public override Chapter[] GetChapterLinks(bool sort = false, int x = 0, int y = 0)
        {
            Dictionary<string, LinkedList<HtmlNode>> chapterInfo =
                pageEnumerator.GetElementsByClassNames(new string[] {"book-catalog"});
            HtmlNode[] n = chapterInfo["book-catalog"].First().SelectNodes("//div[@id=\"morelist\"]//li").ToArray();
            Chapter[] c = new Chapter[n.Length];
            for (int idx = 0; idx < n.Length; idx++)
            {
                c[idx] = new Chapter(this)
                {
                    name = n[idx].InnerText.Replace("\n", string.Empty).SkipCharSequence(new char[] {' '}),
                    chapterLink = new Uri("https://www.novelhall.com" + n[idx].ChildNodes[1].Attributes.First().Value)
                };
            }

            chapterInfo.Clear();
            return c;
        }

        public override TiNodeList GetText(Chapter chp, HtmlDocument use, AWebClient wc)
        {
            try
            {
                use.LoadHtml(Regex.Replace(wc.DownloadString(chp.chapterLink), "(<br>|<br/>|<br />)", "\n",
                    RegexOptions.None));
                GC.Collect();
                IEnumerator<HtmlNode> nod = use.DocumentNode.FindAllNodes();
                if (nod == null)
                {
                    TiNodeList ti = new TiNodeList(); //... All I can do.
                    ti.push_back(new Epub.TiNode()
                    {
                        text =
                            "Page was blank, and 0 content could be retrieved from it. Check the url at a later date please... Sorry.\n" +
                            chp.chapterLink
                    });
                    return ti;
                }

                string[] cnt = HttpUtility
                    .HtmlDecode(use.DocumentNode.FindAllNodes().GetFirstElementByClassNameA("entry-content").InnerText)
                    .Split("\n");
                TiNodeList tnl = new TiNodeList();
                foreach (string str in cnt)
                    tnl.push_back(new Epub.TiNode() {text = str});
                return tnl;
            }
            catch
            {
                TiNodeList ti = new TiNodeList(); //... All I can do.
                ti.push_back(new Epub.TiNode()
                    {text = "Failed to get text for this chapter: check here: " + chp.chapterLink});
                return ti;
            }
        }

        public override dynamic Get(HentaiVideo obj, bool dwnld)
        {
            throw new NotImplementedException();
        }
    }
}