using System;
using System.Net;
using ADLCore.Ext;
using System.IO;
using HtmlAgilityPack;
using System.Linq;
using System.IO.Compression;
using ADLCore.Epub;
using System.Text.Json.Serialization;
using System.Threading;
using ADLCore.Ext.ExtendedClasses;

namespace ADLCore.Novels.Models
{
    public class Chapter
    {
        public string name { get; set; }
        public string parsedName;
        public int chapterNum { get; set; }
        public Uri chapterLink { get; set; }

        [JsonIgnore] DownloaderBase parent;

        public DateTime uploaded;
        public TiNodeList content;
        public string desc = null;

        public Chapter()
        {
        }

        public Chapter(DownloaderBase _base = null)
        {
            parent = _base;
        }

        public string GetText(HtmlDocument docu, AWebClient wc)
        {
            if (content != null)
                return content.ToString();
            content = parent.GetText(this, docu, wc);
            return content.ToString();
        }

        public string GetText()
        {

            if (content == null)
                content = parent.GetText(this, parent.page, parent.webClient);
            return content.ToString();
        }

        public void push_back(string name, byte[] bytes)
            => content.push_back(new TiNode() {img = new Image[] {Image.GenerateImageFromByte(bytes, name)}});

        public void push_back(TiNode ti)
            => content.push_back(ti);

        public void push_back(string text)
            => content.push_back(new TiNode() {text = text});

        /// <summary>
        /// Gets content for every chapter.
        /// </summary>
        /// <param name="chapters"></param>
        /// <returns></returns>
        public static Chapter[] BatchChapterGet(Chapter[] chapters, string dir, Book host, ZipArchive zappo,
            int tid = 0, Action<int, string> statusUpdate = null, Action updateArchive = null)
        {
            AWebClient wc = new AWebClient();
            HtmlDocument docu = new HtmlDocument();
            int f = 0;
            string[] a = null;

            Chapter lastChp = null;
            foreach (Chapter chp in chapters)
            {
                f++;
                a = zappo.GetEntriesUnderDirectoryToStandardString("Chapters/");
                chp.name = chp.name.RemoveSpecialCharacters();
                chp.parsedName = chp.name;
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

                double prg = (double) f / (double) chapters.Length;
                if (statusUpdate != null)
                    statusUpdate(tid,
                        $"[{new string('#', (int) (prg * 10))}{new string('-', (int) (10 - (prg * 10)))}] {(int) (prg * 100)}% | {f}/{chapters.Length} | Downloading: {chp.parsedName}");
                ADLCore.Alert.ADLUpdates.CallLogUpdate(
                    $"[{new string('#', (int) (prg * 10))}{new string('-', (int) (10 - (prg * 10)))}] {(int) (prg * 100)}% | {f}/{chapters.Length} | Downloading: {chp.parsedName}");

                if (a.Contains($"{chp.name}.txt"))
                {
                    using (StreamReader sr = new StreamReader(zappo.GetEntry($"Chapters/{chp.name}.txt").Open()))
                        chp.push_back(sr.ReadToEnd());
                    continue;
                }

                chp.GetText(docu, wc);
                using (TextWriter tw = new StreamWriter(zappo.CreateEntry($"Chapters/{chp.name}.txt").Open()))
                    tw.WriteLine(chp.content.ToString());
                updateArchive?.Invoke();
                docu = new HtmlDocument();
                GC.Collect();
            }

            if (statusUpdate != null)
                statusUpdate(tid, $"Download finished, {chapters.Length}/{chapters.Length}");
            return chapters;
        }

        public static ZipArchiveEntry[] BatchChapterGetMT(Chapter[] chapters, Book host, string dir, int tid = 0,
            Action<int, string> statusUpdate = null)
        {
            Stream fs = new MemoryStream();
            ZipArchive zappo = new ZipArchive(fs, ZipArchiveMode.Update);

            AWebClient wc = new AWebClient();
            HtmlDocument docu = new HtmlDocument();
            int f = 0;

            Chapter lastChp = null;

            foreach (Chapter chp in chapters)
            {
                f++;
                chp.name = chp.name.RemoveSpecialCharacters();
                if (!chp.name.Any(char.IsDigit))
                    throw new Exception("Chapter lacks chapter number (retry without -mt): " + chp.name);

                //chp.name = chp.name;  ???? Don't program while half asleep reason #481
                if (chp.name.ToLower().Contains("volume"))
                {
                    chp.name = new string(chp.name.Skip(7).ToArray());
                    int integrals = chp.name.ToArray().LeadingIntegralCount();
                    chp.name = new string(chp.name.Skip(integrals + 1).ToArray());
                    chp.chapterNum = chp.name.ToArray().FirstLIntegralCount();
                }
                else
                    chp.chapterNum = chp.name.ToArray().FirstLIntegralCount();

                if (chp.name[0] == '-')
                    chp.chapterNum = chp.chapterNum * -1;

                if (lastChp != null)
                    if (chp.chapterNum - 1 != lastChp.chapterNum)
                        host.sortedTrustFactor = true; // Consistency lost, chapter list can not be trusted.
                chp.parsedName = chp.name;
                chp.name = chp.name.Replace(' ', '_');
                lastChp = chp;

                double prg = (double) f / (double) chapters.Length;
                if (statusUpdate != null)
                    statusUpdate(tid,
                        $"[{new string('#', (int) (prg * 10))}{new string('-', (int) (10 - (prg * 10)))}] {(int) (prg * 100)}% | {f}/{chapters.Length} | Downloading: {chp.name}");
                ADLCore.Alert.ADLUpdates.CallLogUpdate(
                    $"[{new string('#', (int) (prg * 10))}{new string('-', (int) (10 - (prg * 10)))}] {(int) (prg * 100)}% | {f}/{chapters.Length} | Downloading: {chp.name}");

                if (chp.content != null)
                    continue;

                chp.GetText(docu, wc);
                using (TextWriter tw = new StreamWriter(zappo.CreateEntry($"Chapters/{chp.name}.txt").Open()))
                    tw.WriteLine(chp.content.ToString());
                docu = new HtmlDocument();
                GC.Collect();
            }

            if (statusUpdate != null)
                statusUpdate(tid,
                    $"Thread {Thread.CurrentThread.ManagedThreadId} finished, {chapters.Length}/{chapters.Length}");
            return zappo.Entries.ToArray();
        }

        public static Chapter testChapter(string url, DownloaderBase _base)
        {
            Chapter c = new Chapter {chapterLink = new Uri(url)};
            HtmlDocument docu = new HtmlDocument();
            AWebClient wc = new AWebClient();
            c.content = _base.GetText(c, docu, wc);
            return c;
        }
    }
}