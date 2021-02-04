using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using ADLCore.Ext;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using ADLCore.Interfaces;
using HtmlAgilityPack;
using System.Linq;
using System.Web;
using System.Runtime.CompilerServices;
using System.IO.Compression;

namespace ADLCore.Novels.Models
{
    public class Chapter
    {
        public string name;
        public string parsedName;
        public Uri chapterLink;
        DownloaderBase parent;
        public DateTime uploaded;
        public string text = null;
        public Byte[] image;
        public string desc = null;

        public Chapter(DownloaderBase _base = null)
        {
            parent = _base;
        }

        public string GetText(HtmlDocument docu, WebClient wc)
        {
            if (text != null)
                return text;
            text = parent.GetText(this, docu, wc);
            return text;
        }

        /// <summary>
        /// Gets content for every chapter.
        /// </summary>
        /// <param name="chapters"></param>
        /// <returns></returns>
        public static Chapter[] BatchChapterGet(Chapter[] chapters, string dir, ref ZipArchive zappo, Site site = Site.wuxiaWorldA, int tid = 0, Action<int, string> statusUpdate = null, Action updateArchive = null, bool mt = false)
        {
            WebClient wc = new WebClient();
            HtmlDocument docu = new HtmlDocument();
            int f = 0;
            string[] a = null;
            foreach (Chapter chp in chapters)
            {
                f++;
                a = zappo.GetEntriesUnderDirectoryToStandardString("Chapters/");
                chp.name = chp.name.RemoveSpecialCharacters();
                string tname = chp.name;
                chp.name = chp.name.Replace(' ', '_');
                if (!chp.name.Any(char.IsDigit))
                    chp.name += $" {(f - 1).ToString()}";

                double prg = (double)f / (double)chapters.Length;
                if (statusUpdate != null)
                    statusUpdate(tid, $"[{new string('#', (int)(prg * 10))}{new string('-', (int)(10 - (prg * 10)))}] {(int)(prg * 100)}% | {f}/{chapters.Length} | Downloading: {tname}");

                if (a.Contains($"{chp.name}.txt"))
                {
                    using (StreamReader sr = new StreamReader(zappo.GetEntry($"Chapters/{chp.name}.txt").Open()))
                        chp.text = sr.ReadToEnd();
                    continue;
                }

                chp.GetText(docu, wc);
                using (TextWriter tw = new StreamWriter(zappo.CreateEntry($"Chapters/{chp.name}.txt").Open()))
                    tw.WriteLine(chp.text);
                updateArchive?.Invoke();
                docu = new HtmlDocument();
                GC.Collect();
            }
            if (statusUpdate != null)
                statusUpdate(tid, $"Download finished, {chapters.Length}/{chapters.Length}");
            return chapters;
        }
        public static ZipArchiveEntry[] BatchChapterGetMT(Chapter[] chapters, string dir, Site site = Site.wuxiaWorldA, int tid = 0, Action<int, string> statusUpdate = null, Action updateArchive = null, bool mt = false)
        {
            Stream fs = new MemoryStream();
            ZipArchive zappo = new ZipArchive(fs, ZipArchiveMode.Update);

            WebClient wc = new WebClient();
            HtmlDocument docu = new HtmlDocument();
            int f = 0;
            foreach (Chapter chp in chapters)
            {
                f++;
                chp.name = chp.name.RemoveSpecialCharacters();
                string tname = chp.name;
                chp.name = chp.name.Replace(' ', '_');
                if (!chp.name.Any(char.IsDigit))
                    chp.name += $" {(f - 1).ToString()}";

                double prg = (double)f / (double)chapters.Length;
                if (statusUpdate != null)
                    statusUpdate(tid, $"[{new string('#', (int)(prg * 10))}{new string('-', (int)(10 - (prg * 10)))}] {(int)(prg * 100)}% | {f}/{chapters.Length} | Downloading: {tname}");

                if (chp.text != null)
                    continue;

                chp.GetText(docu, wc);
                using (TextWriter tw = new StreamWriter(zappo.CreateEntry($"Chapters/{chp.name}.txt").Open()))
                    tw.WriteLine(chp.text);
                docu = new HtmlDocument();
                GC.Collect();
            }
            if (statusUpdate != null)
                statusUpdate(tid, $"Download finished, {chapters.Length}/{chapters.Length}");
            return zappo.Entries.ToArray();
        }
    }
}
