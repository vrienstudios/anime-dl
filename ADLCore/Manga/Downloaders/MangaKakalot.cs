using ADLCore.Epub;
using ADLCore.Ext;
using ADLCore.Manga.Models;
using ADLCore.Novels.Models;
using ADLCore.Video.Constructs;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using ADLCore.Ext.ExtendedClasses;

namespace ADLCore.Manga.Downloaders
{
    class MangaKakalot : MangaBase
    {
        public MangaKakalot(argumentList args, int taskIndex, Action<int, string> act) : base(args, taskIndex, act)
        {
        }

        public override Image[] GetImages(ref MangaChapter aski, ref Models.Manga manga, ref ArchiveManager arc,
            Action<int, string> sU, int ti)
        {
            MovePage(aski.linkTo);
            pageEnumerator.Reset();

            Dictionary<string, LinkedList<HtmlNode>> baseInfo =
                pageEnumerator.GetElementsByClassNames(new string[] {"container-chapter-reader"});
            IEnumerator<HtmlNode> a = baseInfo["container-chapter-reader"].GetEnumerator();
            a.MoveNext(); // Set to 1;
            List<HtmlNode> collection = a.Current.ChildNodes.Where(x => x.Name == "img").ToList();
            List<Image> images = new List<Image>();
            for (int idx = 0; idx < collection.Count; idx++)
            {
                a: ;
                try
                {
                    images.Add(Epub.Image.GenerateImageFromByte(
                        webClient.DownloadData(collection[idx].Attributes[0].Value),
                        collection[idx].Attributes[0].Value.RemoveSpecialCharacters()));
                }
                catch
                {
                    Alert.ADLUpdates.CallLogUpdate(
                        $"Timeout on Img. {idx} from {aski.ChapterName}, retrying after 30 seconds.",
                        Alert.ADLUpdates.LogLevel.Middle);
                    sU(ti, $"Timeout on Img. {idx} from {aski.ChapterName}, retrying after 30 seconds.");
                    Thread.Sleep(30000);
                    goto a;
                }

                ADLCore.Alert.ADLUpdates.CallLogUpdate(
                    $"Got Image {idx} out of {collection.Count - 1} for {aski.ChapterName}");
                sU(ti, $"Got Image {idx} out of {collection.Count - 1} for {aski.ChapterName}");
            }

            return images.ToArray();
        }

        protected override MangaChapter[] GetMangaLinks()
        {
            ADLCore.Alert.ADLUpdates.CallLogUpdate($"Getting Chapters for {this.mdata.name}");
            pageEnumerator.Reset();
            Dictionary<string, LinkedList<HtmlNode>> baseInfo =
                pageEnumerator.GetElementsByClassNames(new string[] {"chapter-list", "row-content-chapter"});
            IEnumerator<HtmlNode> a;
            bool orig = false;
            if (baseInfo["chapter-list"].Count != 0)
            {
                a = baseInfo["chapter-list"].GetEnumerator();
                orig = true;
            }
            else
                a = baseInfo["row-content-chapter"].GetEnumerator();

            a.MoveNext();
            //HtmlNodeCollection collection = a.Current.ChildNodes;
            MangaChapter[] chapters;

            chapters = orig ? GetChaptersA(a.Current) : GetChaptersB(a.Current);

            return chapters;
        }

        private MangaChapter[] GetChaptersA(HtmlNode col)
        {
            List<HtmlNode> collection = col.ChildNodes.Where(x => x.Name == "div").ToList();
            List<MangaChapter> chapters = new List<MangaChapter>();

            for (int idx = collection.Count - 1; idx >= 0; idx--)
            {
                MangaChapter mngChp = new MangaChapter();
                HtmlNode chpData = collection[idx].ChildNodes.First(x => x.Name == "span");
                mngChp.ChapterName = chpData.InnerText + ".imc";
                mngChp.linkTo = chpData.ChildNodes[0].Attributes[0].Value;
                chapters.Add(mngChp);
            }

            return chapters.ToArray();
        }

        private MangaChapter[] GetChaptersB(HtmlNode col)
        {
            List<HtmlNode> collection = col.ChildNodes.Where(x => x.Name == "li").ToList();
            List<MangaChapter> chapters = new List<MangaChapter>();

            for (int idx = collection.Count - 1; idx > 0; idx--)
            {
                MangaChapter mngChp = new MangaChapter();
                HtmlNode chpData = collection[idx].ChildNodes.First(x => x.Name == "a");
                mngChp.ChapterName = chpData.InnerText + ".imc";
                mngChp.linkTo = chpData.Attributes[2].Value;
                chapters.Add(mngChp);
            }

            return chapters.ToArray();
        }

        public override MetaData GetMetaData()
        {
            if (mdata != null)
                return mdata;

            ADLCore.Alert.ADLUpdates.CallLogUpdate("Getting MetaData");
            pageEnumerator.Reset();

            Dictionary<string, LinkedList<HtmlNode>> baseInfo = pageEnumerator.GetElementsByClassNames(new string[]
                {"info-image", "story-info-right", "manga-info-pic", "manga-info-text"});

            mdata = new MetaData();
            mdata.url = this.args.term;
            if (baseInfo["manga-info-pic"].Count != 0)
            {
                string x = Regex.Match(baseInfo["manga-info-pic"].First.Value.InnerHtml, @"<img[^>]+src=""([^"">]+)""")
                    .Groups[1].Value;
                mdata.cover = webClient.DownloadData(x);

                string[] generalInfo = baseInfo["manga-info-text"].First.Value.InnerText.Split("\n");
                mdata.name = generalInfo[2].RemoveSpecialCharacters();
                mdata.author = generalInfo[5];
                mdata.type = generalInfo[6];
            }
            else
            {
                string x = Regex.Match(baseInfo["info-image"].First.Value.InnerHtml, @"<img[^>]+src=""([^"">]+)""")
                    .Groups[1].Value;
                AWebClient awc = new AWebClient();
                awc.wCollection.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                mdata.cover = awc.DownloadData(x);
                awc.Dispose();

                string[] generalInfo = baseInfo["story-info-right"].First.Value.InnerText.Split("\n")
                    .Where(x => !string.IsNullOrEmpty(x)).ToArray();
                mdata.name = generalInfo[0].RemoveSpecialCharacters();
                mdata.author = generalInfo[4];
                mdata.type = generalInfo[8];
            }

            ADLCore.Alert.ADLUpdates.CallLogUpdate("Got MetaData for " + mdata.name);
            return mdata;
        }

        public override void GrabHome(int amount)
        {
            throw new NotImplementedException();
        }

        public override void GrabLinks(int[] range)
        {
            throw new NotImplementedException();
        }
    }
}