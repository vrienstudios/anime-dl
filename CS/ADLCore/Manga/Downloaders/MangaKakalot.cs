using ADLCore.Epub;
using ADLCore.Ext;
using ADLCore.Manga.Models;
using ADLCore.Novels.Models;
using ADLCore.Video.Constructs;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ADLCore.Manga.Downloaders
{
    class MangaKakalot : MangaBase
    {
        public MangaKakalot(argumentList args, int taskIndex, Action<int, string> act) : base(args, taskIndex, act)
        {

        }

        public override Image[] GetImages(ref MangaChapter aski, ref Models.Manga manga, ref ArchiveManager arc)
        {
            MovePage(aski.linkTo);
            pageEnumerator.Reset();

            Dictionary<string, LinkedList<HtmlNode>> baseInfo = pageEnumerator.GetElementsByClassNames(new string[] { "container-chapter-reader" });
            IEnumerator<HtmlNode> a = baseInfo["container-chapter-reader"].GetEnumerator();
            a.MoveNext(); // Set to 1;

            HtmlNodeCollection collection = a.Current.ChildNodes;

            List<Image> images = new List<Image>();
            for (int idx = collection.Count - 1; idx > 0; idx--)
            {
                if (collection[idx].Name != "img")
                    continue;

                GenerateHeaders();
                images.Add(Epub.Image.GenerateImageFromByte(webClient.DownloadData(collection[idx].Attributes[0].Value), "empty"));
            }
            return images.ToArray();
        }

        public override MangaChapter[] GetMangaLinks()
        {
            pageEnumerator.Reset();
            Dictionary<string, LinkedList<HtmlNode>> baseInfo = pageEnumerator.GetElementsByClassNames(new string[] { "chapter-list" });
            IEnumerator<HtmlNode> a = baseInfo["chapter-list"].GetEnumerator();
            a.MoveNext();
            HtmlNodeCollection collection = a.Current.ChildNodes;

            List<MangaChapter> chapters = new List<MangaChapter>();

            for(int idx = collection.Count - 1; idx > 0; idx--)
            {
                if (collection[idx].Name != "div")
                    continue;

                MangaChapter mngChp = new MangaChapter();
                mngChp.ChapterName = collection[idx].ChildNodes[1].InnerText.Replace("\n", string.Empty);
                mngChp.linkTo = Regex.Match(collection[idx].InnerHtml, "href=\"(.*?)\"").Groups[1].Value.Replace("\n", string.Empty);
                chapters.Add(mngChp);
            }

            return chapters.ToArray();
        }

        public override MetaData GetMetaData()
        {
            if (mdata != null)
                return mdata;

            pageEnumerator.Reset();

            Dictionary<string, LinkedList<HtmlNode>> baseInfo = pageEnumerator.GetElementsByClassNames(new string[] { "manga-info-pic", "manga-info-text" });

            mdata = new MetaData();
            string x = Regex.Match(baseInfo["manga-info-pic"].First.Value.InnerHtml, @"<img[^>]+src=""([^"">]+)""").Groups[1].Value;
            mdata.cover = webClient.DownloadData(x);

            string[] generalInfo = baseInfo["manga-info-text"].First.Value.InnerText.Split("\n");
            mdata.name = generalInfo[2].RemoveSpecialCharacters();
            mdata.author = generalInfo[5];
            mdata.type = generalInfo[6];

            return mdata;
        }
    }
}
