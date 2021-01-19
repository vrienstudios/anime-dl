using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using anime_dl.Ext;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using anime_dl.Interfaces;
using HtmlAgilityPack;
using System.Linq;
using System.Web;
using System.Runtime.CompilerServices;

namespace anime_dl.Novels.Models
{
    class Chapter
    {
        public string name;
        public Uri chapterLink;
        public DateTime uploaded;
        public string text = null;
        public Byte[] image;
        public string desc = null;

        [Obsolete]
        public string GetText()
        {
            if (text != null)
                return text;
            //chapter-entity//
            WebClient wc = new WebClient();
            /*IHTMLDocument2 htmlDoc = mshtml.GetDefaultDocument();
            string dwb = wc.DownloadString(chapterLink);
            htmlDoc.write(dwb);
            dwb = null;
            text = htmlDoc.all.GetEnumerator().GetFirstElementByClassNameA("chapter-entity").innerText;
            htmlDoc.clear();
            htmlDoc = null;*/
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
                string tname = chp.name;
                chp.name = chp.name.Replace(' ', '_').RemoveSpecialCharacters();
                if (!chp.name.Any(char.IsDigit))
                    chp.name += $" {(f - 1).ToString()}";

                if (File.Exists($"{dir}\\{chp.name}.txt"))
                {
                    chp.text = File.ReadAllText($"{dir}\\{chp.name}.txt");
                    continue;
                }

                double prg = (double)f / (double)chapters.Length;
                if (statusUpdate != null)
                    statusUpdate(tid, $"[{new string('#', (int)(prg * 10))}{new string('-', (int)(10 - (prg * 10)))}] {(int)(prg * 100)}% | {f}/{chapters.Length} | Downloading: {tname}");

                switch (site)
                {
                    case Site.AsianHobbyist:
                        break;
                    case Site.NovelFull:
                        chp.text = GetTextNovelFull(chp, docu, wc);
                        break;
                    case Site.NovelHall:
                        chp.text = GetTextNovelHall(chp, docu, wc);
                        break;
                    case Site.ScribbleHub:
                        chp.text = GetTextScribbleHub(chp, docu, wc);
                        break;
                    case Site.wuxiaWorldA:
                        chp.text = GetTextWuxiaWorldA(chp, docu, wc);
                        break;
                    case Site.wuxiaWorldB:
                        chp.text = GetTextWuxiaWorldB(chp, docu, wc);
                        break;
                }

                using (TextWriter tw = new StreamWriter(new FileStream($"{dir}\\{chp.name}.txt", FileMode.OpenOrCreate)))
                    tw.WriteLine(chp.text);
                docu = new HtmlDocument();
                GC.Collect();
            }
            if (statusUpdate != null)
                statusUpdate(tid, $"Download finished, {chapters.Length}/{chapters.Length}");
            return chapters;
        }

        private static string GetTextNovelHall(Chapter chp, HtmlDocument use, WebClient wc)
        {
            use.LoadHtml(Regex.Replace(wc.DownloadString(chp.chapterLink), "(<br>|<br/>|<br />)", "\n", RegexOptions.None));
            GC.Collect();
            return use.DocumentNode.FindAllNodes().GetFirstElementByClassNameA("entry-content").InnerText;
        }

        private static string GetTextWuxiaWorldA(Chapter chp, HtmlDocument use, WebClient wc)
        {
            use.LoadHtml(Regex.Replace(wc.DownloadString(chp.chapterLink), "(<br>|<br/>)", "\n", RegexOptions.Singleline));
            GC.Collect();
            return use.DocumentNode.FindAllNodes().GetFirstElementByClassNameA("chapter-entity").InnerText;
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

        private static string GetTextScribbleHub(Chapter chp, HtmlDocument use, WebClient wc)
        {
            wc.Headers = IAppBase.GenerateHeaders(chp.chapterLink.Host);
            string dwnld = wc.DownloadString(chp.chapterLink);
            use.LoadHtml(dwnld);
            GC.Collect();
            return use.DocumentNode.FindAllNodes().GetFirstElementByClassNameA("chp_raw").InnerText;
        }

        private static string GetTextNovelFull(Chapter chp, HtmlDocument use, WebClient wc)
        {
            wc.Headers = IAppBase.GenerateHeaders(chp.chapterLink.Host);
            string dwnld = wc.DownloadString(chp.chapterLink);
            use.LoadHtml(dwnld);
            HtmlNode a = use.DocumentNode.FindAllNodes().GetFirstElementByClassNameA("chapter-c");
            a.InnerHtml = Regex.Replace(a.InnerHtml, "<script.*?</script>", string.Empty, RegexOptions.Singleline);
            a.InnerHtml = Regex.Replace(a.InnerHtml, "<div.*?</div>", string.Empty, RegexOptions.Singleline);
            a.InnerHtml = a.InnerHtml.Replace("<p>", "\n").Replace("</p>", "\n");
            GC.Collect();
            return a.InnerHtml;
        }
    }
}
