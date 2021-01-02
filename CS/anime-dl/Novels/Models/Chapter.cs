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

namespace anime_dl.Novels.Models
{
    class Chapter
    {
        public string name;
        public Uri chapterLink;
        public DateTime uploaded;
        public string text = null;
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
        public static Chapter[] BatchChapterGet(Chapter[] chapters, string dir, Site site = Site.wuxiaWorldA)
        {
            Directory.CreateDirectory(dir);
            WebClient wc = new WebClient();
            HtmlDocument docu = new HtmlDocument();
            foreach (Chapter chp in chapters)
            {
                chp.name = chp.name.Replace(' ', '_').RemoveSpecialCharacters();
                if (File.Exists($"{dir}\\{chp.name}.txt"))
                    continue;
                switch (site)
                {
                    case Site.NovelFull:
                        chp.text = GetTextNovelFull(chp, docu, wc);
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
            return chapters;
        }

        private static string GetTextWuxiaWorldA(Chapter chp, HtmlDocument use, WebClient wc)
        {
            Console.WriteLine($"Getting Chapter {chp.name} from {chp.chapterLink}");
        B:
            try
            {
                use.LoadHtml(Regex.Replace(wc.DownloadString(chp.chapterLink), "<script.*?</script>", string.Empty, RegexOptions.Singleline));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occurred Retry: {0}", ex.Message);
                goto B;
            }
            GC.Collect();
            return use.DocumentNode.FindAllNodes().GetFirstElementByClassNameA("chapter-entity").InnerText;
        }

        private static string GetTextWuxiaWorldB(Chapter chp, HtmlDocument use, WebClient wc)
        {
            Console.WriteLine($"Getting Chapter {chp.name} from {chp.chapterLink}");
            use.LoadHtml(Regex.Replace(wc.DownloadString(chp.chapterLink), "<script.*?</script>", string.Empty, RegexOptions.Singleline));
            GC.Collect();
            return use.DocumentNode.FindAllNodes().GetFirstElementByClassNameA("fr-view").InnerText;
        }

        private static string GetTextScribbleHub(Chapter chp, HtmlDocument use, WebClient wc)
        {
            Console.WriteLine($"Getting Chapter {chp.name} from {chp.chapterLink}");
            wc.Headers = IAppBase.GenerateHeaders(chp.chapterLink.Host);
            string dwnld = wc.DownloadString(chp.chapterLink);
            use.LoadHtml(dwnld);
            GC.Collect();
            return use.DocumentNode.FindAllNodes().GetFirstElementByClassNameA("chp_raw").InnerText;
        }

        private static string GetTextNovelFull(Chapter chp, HtmlDocument use, WebClient wc)
        {
            Console.WriteLine($"{Thread.CurrentThread.Name} || Getting Chapter {chp.name} from {chp.chapterLink}");
            wc.Headers = IAppBase.GenerateHeaders(chp.chapterLink.Host);
            string dwnld = wc.DownloadString(chp.chapterLink);
            use.LoadHtml(dwnld);
            HtmlNode a = use.DocumentNode.FindAllNodes().GetFirstElementByClassNameA("chapter-c");
            GC.Collect();
            return a.InnerText;
        }
    }
}
