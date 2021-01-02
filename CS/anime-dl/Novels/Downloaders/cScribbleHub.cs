﻿using anime_dl.Ext;
using anime_dl.Novels;
using anime_dl.Novels.Models;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace KobeiD.Downloaders
{
    class cScribbleHub : DownloaderBase
    {
        public cScribbleHub(string uri) : base(uri)
        {

        }

        public MetaData GetMetaData()
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

            string x = Regex.Match(baseInfo["fic_image"].First().OuterHtml, @"<IMG[^>]+src=""([^"">]+)""").Groups[1].Value;
            //x = x.Remove(x.IndexOf('?'));
            GenerateHeaders();
            mdata.cover = webClient.DownloadData(x);

            pageEnumerator.Reset();
            baseInfo.Clear();
            return mdata;
        }

        public Chapter[] GetChapterLinks(bool sort = false)
        {
            int idx = 0;
            List<Chapter> chaps = new List<Chapter>();
            Regex reg = new Regex("href=\"(.*?)\"");
            while (true)
            {
                idx++;
                MovePage($"{mdata.url}?toc={idx.ToString()}#content1");
                Dictionary<string, LinkedList<HtmlNode>> chapterInfo = pageEnumerator.GetElementsByClassNames(new string[] { "toc_a" });

                if (chapterInfo["toc_a"].Count <= 0)
                    break;

                IEnumerator<HtmlNode> a = chapterInfo["toc_a"].GetEnumerator();
                while (a.MoveNext())
                    chaps.Add(new Chapter() { name = a.Current.InnerText, chapterLink = new Uri(reg.Match(a.Current.OuterHtml).Groups[1].Value) });

            }
            chaps.Reverse();
            return chaps.ToArray();
        }
    }
}
