using ADLCore.Alert;
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
        argumentList args;

        public MangaBase(argumentList args, int taskIndex, Action<int, string> act)
        {
            if (taskIndex > -1 && act != null || taskIndex == -1 && act == null)
            {
                this.taskIndex = taskIndex;
                this.updateStatus = act;
            }
            else
                throw new Exception("Invalid statusUpdate args");

            ADLUpdates.CallUpdate("Creating Manga Download Instance", false);
            this.url = new Uri(args.term);
            webClient = new WebClient();
            GenerateHeaders();
            string html = webClient.DownloadString(url);
            LoadPage(html);
            html = null;
            this.args = args;
            ADLUpdates.CallUpdate("Generating ADL Archive", false);
            archive = new ArchiveManager() { args = args };
        }

        public void BeginExecution()
        {
            Manga.Models.Manga manga = new Manga.Models.Manga();
            manga.metaData = GetMetaData();

            if (args.d)
            {
                archive.InitializeZipper(args.l ? args.export + Path.DirectorySeparatorChar + manga.metaData.name + ".adl" : Directory.GetCurrentDirectory() + $"{Path.DirectorySeparatorChar}Epubs{Path.DirectorySeparatorChar}" + manga.metaData.name + ".adl");
                manga.Chapters = GetMangaLinks();
            }
            else
            {
                archive.InitializeZipper(args.l ? args.export + Path.DirectorySeparatorChar + manga.metaData.name + ".adl" : Directory.GetCurrentDirectory() + $"{Path.DirectorySeparatorChar}Epubs{Path.DirectorySeparatorChar}" + manga.metaData.name + ".adl", true);
                manga.LoadChaptersFromADL(Directory.GetCurrentDirectory() + $"{Path.DirectorySeparatorChar}Epubs{Path.DirectorySeparatorChar}" + manga.metaData.name + ".adl", ref archive.zapive);
            }


            for (int idx = manga.Chapters.Length; idx < manga.Chapters.Length; idx++)
            {
                manga.Chapters[idx].Images = GetImages(ref manga.Chapters[idx], ref manga, ref archive);
                List<Byte[]> bytes = new List<byte[]>();

                foreach (Epub.Image img in manga.Chapters[idx].Images)
                    bytes.Add(img.bytes);

                archive.AddContentToArchive(manga.Chapters[idx].ChapterName, bytes);
                manga.Chapters[idx].Images = null; // free up memory.
                GC.Collect();
            }

            manga.ExportToEpub(Directory.GetCurrentDirectory() + $"{Path.DirectorySeparatorChar}Epubs{Path.DirectorySeparatorChar}" + manga.metaData.name);
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
