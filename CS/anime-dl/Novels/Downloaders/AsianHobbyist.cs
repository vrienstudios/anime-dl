using anime_dl.Ext;
using anime_dl.Novels.Models;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace anime_dl.Novels.Downloaders
{
    class AsianHobbyist : DownloaderBase
    {
        public AsianHobbyist(string url, int taskIndex, Action<int, string> act) : base(url, taskIndex, act)
        {

        }

        public override MetaData GetMetaData() // MetaData done
        {
            if (mdata != null)
                return mdata;
            Program.WriteToConsole("Creating MetaData Object", false);
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
            Program.WriteToConsole($"Got MetaData Object for {mdata.name} by {mdata.author}", false);
            return mdata;
        }


        public override Chapter[] GetChapterLinks(bool sort = false)
        {
            throw new NotImplementedException("Can not get chapter links for asian hobbyist yet");
        }
    }
}
