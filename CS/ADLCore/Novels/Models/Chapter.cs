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
        public static Chapter[] BatchChapterGet(Chapter[] chapters, string dir, Site site = Site.wuxiaWorldA, int tid = 0, Action<int, string> statusUpdate = null)
        {
            Directory.CreateDirectory(dir);

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

                if (File.Exists($"{dir}{Path.DirectorySeparatorChar}{chp.name}.txt"))
                {
                    chp.text = File.ReadAllText(Path.Combine(dir, chp.name, ".txt"));
                    continue;
                }

                double prg = (double)f / (double)chapters.Length;
                if (statusUpdate != null)
                    statusUpdate(tid, $"[{new string('#', (int)(prg * 10))}{new string('-', (int)(10 - (prg * 10)))}] {(int)(prg * 100)}% | {f}/{chapters.Length} | Downloading: {tname}");

                chp.GetText(docu, wc);

                using (TextWriter tw = new StreamWriter(new FileStream(Path.GetFullPath($"{dir}{Path.DirectorySeparatorChar}{chp.name}.txt"), FileMode.OpenOrCreate)))
                    tw.WriteLine(chp.text);
                docu = new HtmlDocument();
                GC.Collect();
            }
            if (statusUpdate != null)
                statusUpdate(tid, $"Download finished, {chapters.Length}/{chapters.Length}");
            return chapters;
        }

        private static string GetTextWuxiaWorldB(Chapter chp, HtmlDocument use, WebClient wc)
        {
            use.LoadHtml(Regex.Replace(wc.DownloadString(chp.chapterLink), "(<br>|<br/>)", "\n", RegexOptions.Singleline));
            GC.Collect();
            HtmlNode a = use.DocumentNode.SelectSingleNode("//*[@id=\"chapter-content\"]");
            HtmlNodeCollection aaab = use.DocumentNode.SelectNodes("//*[@dir=\"ltr\"]");
            List<HtmlNode> aa = new List<HtmlNode>();

            if (aaab != null)
                aa = aaab.ToList();
            else
            {
                use.LoadHtml(a.OuterHtml);
                aa = use.DocumentNode.SelectNodes("//p").ToList();
            }

            StringBuilder b = new StringBuilder();
            foreach (HtmlNode n in aa)
                b.Append(HttpUtility.HtmlDecode(n.InnerText + "\n\n"));
            return b.ToString();
        }
    }
}
