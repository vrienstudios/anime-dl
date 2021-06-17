using System;
using System.Net;
using ADLCore.Ext;
using System.IO;
using HtmlAgilityPack;
using System.Linq;
using System.IO.Compression;

namespace ADLCore.Novels.Models
{
    public class Chapter
    {
        public string name;
        public string parsedName;
        public int chapterNum;
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
        public static Chapter[] BatchChapterGet(Chapter[] chapters, string dir, Book host, ref ZipArchive zappo, int tid = 0, Action<int, string> statusUpdate = null, Action updateArchive = null)
        {
            WebClient wc = new WebClient();
            HtmlDocument docu = new HtmlDocument();
            int f = 0;
            string[] a = null;

            Chapter lastChp = null;
            foreach (Chapter chp in chapters)
            {
                f++;
                a = zappo.GetEntriesUnderDirectoryToStandardString("Chapters/");
                chp.name = chp.name.RemoveSpecialCharacters();
                string tname = chp.name;
                chp.name = chp.name.Replace(' ', '_');

                if (!chp.name.Any(char.IsDigit))
                {
                    chp.name += $" {(f - 1)}";
                    chp.chapterNum = f - 1;
                }

                if (lastChp != null)
                    if (chp.chapterNum - 1 != lastChp.chapterNum)
                        host.sortedTrustFactor = true; // Consistency lost, chapter list can not be trusted.

                lastChp = chp;

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

        public static ZipArchiveEntry[] BatchChapterGetMT(Chapter[] chapters, Book host, string dir, int tid = 0, Action<int, string> statusUpdate = null)
        {
            Stream fs = new MemoryStream();
            ZipArchive zappo = new ZipArchive(fs, ZipArchiveMode.Update);

            WebClient wc = new WebClient();
            HtmlDocument docu = new HtmlDocument();
            int f = 0;

            Chapter lastChp = null;

            foreach (Chapter chp in chapters)
            {
                f++;
                chp.name = chp.name.RemoveSpecialCharacters();
                if (!chp.name.Any(char.IsDigit))
                    throw new Exception("Chapter lacks chapter number (retry without -mt): " + chp.name);

                chp.name = chp.name.Replace(' ', '_');

                if(chp.name.ToLower().Contains("volume"))
                {
                    chp.name = new string(chp.name.Skip(7).ToArray());
                    int integrals = chp.name.ToArray().LeadingIntegralCount();
                    chp.name = new string(chp.name.Skip(integrals + 1).ToArray());
                    chp.chapterNum = chp.name.ToArray().FirstLIntegralCount();
                }

                if (chp.name[0] == '-')
                    chp.chapterNum = chp.chapterNum * -1;

                if (lastChp != null)
                    if (chp.chapterNum - 1 != lastChp.chapterNum)
                        host.sortedTrustFactor = true; // Consistency lost, chapter list can not be trusted.

                lastChp = chp;

                double prg = (double)f / (double)chapters.Length;
                if (statusUpdate != null)
                    statusUpdate(tid, $"[{new string('#', (int)(prg * 10))}{new string('-', (int)(10 - (prg * 10)))}] {(int)(prg * 100)}% | {f}/{chapters.Length} | Downloading: {chp.name}");

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

        public static Chapter testChapter(string url, DownloaderBase _base)
        {
            Chapter c = new Chapter { chapterLink = new Uri(url) };
            HtmlDocument docu = new HtmlDocument();
            WebClient wc = new WebClient();
            c.text = _base.GetText(c, docu, wc);
            return c;
        }
    }
}
