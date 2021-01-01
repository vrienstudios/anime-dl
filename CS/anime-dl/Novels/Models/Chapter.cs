using MSHTML;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using anime_dl.Ext;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using anime_dl.Interfaces;

namespace anime_dl.Novels.Models
{
    class Chapter
    {
        public string name;
        public Uri chapterLink;
        public DateTime uploaded;
        public string text = null;
        public string desc = null;

        public string GetText()
        {
            if (text != null)
                return text;
            //chapter-entity//
            WebClient wc = new WebClient();
            IHTMLDocument2 htmlDoc = mshtml.GetDefaultDocument();
            string dwb = wc.DownloadString(chapterLink);
            htmlDoc.write(dwb);
            dwb = null;
            text = htmlDoc.all.GetEnumerator().GetFirstElementByClassNameA("chapter-entity").innerText;
            htmlDoc.clear();
            htmlDoc = null;
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
            IHTMLDocument2 docu = (IHTMLDocument2)new HTMLDocument();
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
                docu = (IHTMLDocument2)new HTMLDocument();
                docu.designMode = "On";
                GC.Collect();
            }
            return chapters;
        }

        private static string GetTextWuxiaWorldA(Chapter chp, IHTMLDocument2 use, WebClient wc)
        {
            use.clear(); // just in case
            Console.WriteLine($"Getting Chapter {chp.name} from {chp.chapterLink}");
        B:
            try
            {
                use.write(Regex.Replace(wc.DownloadString(chp.chapterLink), "<script.*?</script>", string.Empty, RegexOptions.Singleline));
                use.close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occurred Retry: {0}", ex.Message);
                goto B;
            }
            GC.Collect();
            return use.all.GetEnumerator().GetFirstElementByClassNameA("chapter-entity").innerText;
        }

        private static string GetTextWuxiaWorldB(Chapter chp, IHTMLDocument2 use, WebClient wc)
        {
            use.clear(); // just in case
            Console.WriteLine($"Getting Chapter {chp.name} from {chp.chapterLink}");
            use.write(Regex.Replace(wc.DownloadString(chp.chapterLink), "<script.*?</script>", string.Empty, RegexOptions.Singleline));
            use.close();
            GC.Collect();
            return use.all.GetEnumerator().GetFirstElementByClassNameA("fr-view").innerText;
        }

        private static string GetTextScribbleHub(Chapter chp, IHTMLDocument2 use, WebClient wc)
        {
            use.clear(); // just in case
            Console.WriteLine($"Getting Chapter {chp.name} from {chp.chapterLink}");
            wc.Headers = IAppBase.GenerateHeaders(chp.chapterLink.Host);
            string dwnld = wc.DownloadString(chp.chapterLink);
            use.write(dwnld);
            use.close();
            GC.Collect();
            return use.all.GetEnumerator().GetFirstElementByClassNameA("chp_raw").innerText;
        }

        private static string GetTextNovelFull(Chapter chp, IHTMLDocument2 use, WebClient wc)
        {
            use.clear(); // just in case
            Console.WriteLine($"{Thread.CurrentThread.Name} || Getting Chapter {chp.name} from {chp.chapterLink}");
            wc.Headers = IAppBase.GenerateHeaders(chp.chapterLink.Host);
            string dwnld = wc.DownloadString(chp.chapterLink);
            use.write(dwnld);
            use.close();
            MSHTML.IHTMLElement a = use.all.GetEnumerator().GetFirstElementByClassName("chapter-c");
            GC.Collect();
            return a.innerText;
        }
    }
}
