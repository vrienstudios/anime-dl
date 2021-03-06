﻿using ADLCore.Alert;
using ADLCore.Ext;
using ADLCore.Interfaces;
using ADLCore.Manga.Models;
using ADLCore.Novels.Models;
using ADLCore.Video.Constructs;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;

namespace ADLCore.Manga
{
    public abstract class MangaBase : IAppBase
    {
        public WebClient webClient;
        public HtmlDocument page;

        public IEnumerator<HtmlNode> pageEnumerator;

        public MetaData mdata;
        public Uri url;

        public int taskIndex;

        public Action<int, string> updateStatus;

        ArchiveManager archive;
        public argumentList args;

        public MangaBase(argumentList args, int taskIndex, Action<int, string> act)
        {
            if (taskIndex > -1 && act != null || taskIndex == -1 && act == null)
            {
                this.taskIndex = taskIndex;
                this.updateStatus = act;
            }
            else
                throw new Exception("Invalid statusUpdate args");

            ADLUpdates.CallLogUpdate("Creating Manga Download Instance");
            this.url = new Uri(args.term);
            webClient = new WebClient();
            GenerateHeaders();

            if (args.d && args.term.IsValidUri())
            {
                string html = webClient.DownloadString(url);
                LoadPage(html);
                html = null;
            }
            this.args = args;
            ADLUpdates.CallLogUpdate("Generating ADL Archive");
            archive = new ArchiveManager() { args = args };
        }

        public void BeginExecution()
        {
            Manga.Models.Manga manga = new Manga.Models.Manga();
            string ex;
            if (args.d)
            {
                if (args.term.IsValidUri()) {
                    manga.metaData = GetMetaData();
                    ex = args.l ? args.export + Path.DirectorySeparatorChar + manga.metaData.name + ".adl" : Directory.GetCurrentDirectory() + $"{Path.DirectorySeparatorChar}Epubs{Path.DirectorySeparatorChar}" + manga.metaData.name + ".adl";
                    archive.InitializeZipper(ex);
                }
                else {
                    archive.InitializeZipper(args.term, true);
                    manga.LoadMangaFromADL(ref archive.zapive);
                    ex = args.l ? args.export + Path.DirectorySeparatorChar + manga.metaData.name + ".adl" : Directory.GetCurrentDirectory() + $"{Path.DirectorySeparatorChar}Epubs{Path.DirectorySeparatorChar}" + manga.metaData.name + ".adl";
                    args.term = manga.metaData.url;
                }

                this.mdata = manga.metaData;
                manga.ExportMetaData(ref archive.zapive);
                MovePage(args.term);
                MangaChapter[] b = GetMangaLinks();
                if (manga.Chapters != null)
                {
                    ArraySegment<MangaChapter> mg = new ArraySegment<MangaChapter>(b, manga.Chapters.Length, b.Length - manga.Chapters.Length);
                    MangaChapter[] c = new MangaChapter[b.Length];
                    manga.Chapters.CopyTo(c, 0);
                    mg.CopyTo(c, manga.Chapters.Length);
                    manga.Chapters = c;
                }
                else
                    manga.Chapters = b;
            }
            else
            {
                //manga.Chapters = GetMangaLinks(); unable for now.
                ex = args.l ? args.export : args.term;
                archive.InitializeZipper(ex, true);
                manga.LoadMangaFromADL(ref archive.zapive);
                manga.LoadChaptersFromADL(ref archive.zapive);
            }

            for (int idx = (args.vRange ? args.VideoRange[0] : args.d ? 0 : manga.Chapters.Length); idx < (args.vRange ? args.VideoRange[1] : manga.Chapters.Length); idx++)
            {
                if (manga.Chapters[idx].existing == true)
                    continue;
                manga.Chapters[idx].Images = GetImages(ref manga.Chapters[idx], ref manga, ref archive);
                List<Byte[]> bytes = new List<byte[]>();

                foreach (Epub.Image img in manga.Chapters[idx].Images)
                    bytes.Add(img.bytes);

                archive.AddContentToArchive(manga.Chapters[idx].ChapterName, bytes);
                manga.Chapters[idx].Images = null; // free up memory.
                GC.Collect();
            }

            manga.ExportToEpub(Directory.GetCurrentDirectory() + $"{Path.DirectorySeparatorChar}Epubs{Path.DirectorySeparatorChar}" + manga.metaData.name, ref archive.zapive);
        }

        public void CancelDownload(string mdataLock)
        {
            throw new NotImplementedException();
        }

        public void GenerateHeaders()
        {
            webClient.Headers.Clear();
            webClient.Headers.Add("Referer", "https://mangakakalot.com/");
        }

        public abstract Epub.Image[] GetImages(ref MangaChapter aski, ref Models.Manga manga, ref ArchiveManager arc);

        public dynamic Get(HentaiVideo obj, bool dwnld)
        {
            throw new NotImplementedException();
        }

        public abstract MetaData GetMetaData();

        public abstract Models.MangaChapter[] GetMangaLinks();

        public void LoadPage(string html)
        {
            page = new HtmlDocument();
            page.LoadHtml(html);
            pageEnumerator = page.DocumentNode.FindAllNodes();
            GC.Collect();
        }

        public void MovePage(string uri)
        {
            webClient.Headers.Clear();
            GenerateHeaders();
            LoadPage(webClient.DownloadString(uri));
        }

        public void ResumeDownload(string mdataLock)
        {
            throw new NotImplementedException();
        }
    }
}
