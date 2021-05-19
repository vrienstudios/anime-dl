using ADLCore.Ext;
using ADLCore.Novels.Models;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Text;

namespace ADLCore.Manga.Downloaders
{
    class MangaKakalot : MangaBase
    {
        public MangaKakalot(string url, int taskIndex, Action<int, string> act) : base(url, taskIndex, act)
        {

        }

        public override MetaData GetMetaData()
        {
            if (mdata != null)
                return mdata;

            pageEnumerator.Reset();

            Dictionary<string, LinkedList<HtmlNode>> baseInfo = pageEnumerator.GetElementsByClassNames(new string[] { "manga-info-pic", "manga-info-text" });

            mdata = new MetaData();

            return null;
        }
    }
}
