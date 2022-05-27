﻿using ADLCore;
using ADLCore.Alert;
using ADLCore.Ext;
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
using System.Threading;
using System.Web;
using ADLCore.Constructs;
using ADLCore.Ext.ExtendedClasses;

namespace ADLCore.Novels.Downloaders
{
    /// <summary>
    /// WuxiaWorld.co
    /// </summary>
    public class dWuxiaWorld : DownloaderBase
    {
        public dWuxiaWorld(argumentList args, int taskIndex, Action<int, dynamic> act) : base(args, taskIndex, act)
        {
        }

        public override dynamic GetMetaData()
        {
            if (mdata != null)
                return mdata;

            pageEnumerator.Reset();
            Dictionary<string, LinkedList<HtmlNode>> baseInfo = pageEnumerator.GetElementsByClassNames(new string[]
                {"book-name", "author", "book-state", "book-catalog", "score"});

            mdata = new MetaData();
            this.mdata.url = this.url.ToString();
            mdata.LangType = MetaData.LangTypes.Mixed;
            try
            {
                mdata.name = baseInfo["book-name"].First().InnerText.DeleteFollowingWhiteSpaceA();
                mdata.author = baseInfo["author"].First().InnerText.SkipPreceedingAndChar(':').Sanitize();
                mdata.type = "nvl";
                mdata.genre = baseInfo["book-catalog"].First().InnerText.DeleteFollowingWhiteSpaceA().Sanitize();
                mdata.rating = baseInfo["score"].First().InnerText.Sanitize();
            }
            catch
            {
                updateStatus(taskIndex, "Failed to load some values, failed");
            }

            mdata.cover =
                webClient.DownloadData(
                    $"https://img.wuxiaworld.co/BookFiles/BookImages/{mdata.name.Replace(' ', '-').Replace('\'', '-')}.jpg");

            mdata.Downloader = this;
            mdata.Parent = this.thisBook;
            return EndMDataRoutine();
        }

        //TODO: implement this, when the site comes back online; site is currently dead as of writing.
        public override void GrabHome(int amount)
        {
            throw new Exception("Dead Site");
        }

        public override void GrabLinks(int[] range)
        {
            throw new Exception("Dead Site");
        }

        public override dynamic Search(bool promptUser = false, bool grabAll = false)
        {
            throw new NotImplementedException();
        }

        public override Chapter[] GetChapterLinks(bool sort = false, int x = 0, int y = 0)
        {
            Dictionary<string, LinkedList<HtmlNode>> chapterInfo =
                pageEnumerator.GetElementsByClassNames(new string[] {"chapter-item"});
            IEnumerator<HtmlNode> a = chapterInfo["chapter-item"].GetEnumerator();
            Regex reg = new Regex("href=\"(.*?)\"");

            Chapter[] c = new Chapter[chapterInfo["chapter-item"].Count()];

            for (int idx = 0; idx < chapterInfo["chapter-item"].Count(); idx++)
            {
                a.MoveNext();
                c[idx] = new Chapter(this)
                {
                    name = (a.Current).InnerText.Replace("\r\n", string.Empty).SkipCharSequence(new char[] {' '}),
                    chapterLink = new Uri("https://www.wuxiaworld.co" + reg.Match(a.Current.OuterHtml).Groups[1].Value)
                };
            }

            reg = null;
            a = null;
            chapterInfo.Clear();

            return c;
        }

        public override TiNodeList GetText(Chapter chp, HtmlDocument use, AWebClient wc)
        {
            a: ;

            try
            {
                use.LoadHtml(Regex.Replace(wc.DownloadString(chp.chapterLink), "(<br>|<br/>)", "\n",
                    RegexOptions.Singleline));
            }
            catch
            {
                ADLCore.Alert.ADLUpdates.CallLogUpdate("Retrying download, retrying in 30 seconds.");
                Thread.Sleep(30000);
                goto a;
            }

            GC.Collect();
            string[] cnt = HttpUtility
                .HtmlDecode(Regex.Unescape(use.DocumentNode.FindAllNodes().GetFirstElementByClassNameA("chapter-entity")
                    .InnerText)).Split("\n");
            TiNodeList tnl = new TiNodeList();
            foreach (string str in cnt)
                tnl.push_back(new Epub.TiNode() {text = str});
            return tnl;
        }
    }
}