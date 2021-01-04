using anime_dl.Ext;
using anime_dl.Novels.Models;
using anime_dl.Video.Extractors;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;

namespace anime_dl
{
    class Program
    {
        static Object[] ArgLoop(string[] args)
        {
            string mn = string.Empty;
            string term = string.Empty;
            bool d = false, mt = false, cc = false, h = false, s = false, e = false, aS = false, nS = false, help = false, c = false;
            for (int idx = 0; idx < args.Length; idx++)
            {
                string str = args[idx];
                switch (str)
                {
                    case "ani":
                        if (mn != string.Empty)
                            throw new Exception("ani/nvl selector has already been set in this parameter list.");
                        mn = "ani";
                        break;
                    case "nvl":
                        if (mn != string.Empty)
                            throw new Exception("ani/nvl selector has already been set in this parameter list.");
                        mn = "nvl";
                        break;
                    case "-aS":
                        if (mn != string.Empty)
                            throw new Exception("ani/nvl selector has already been set in this parameter list.");
                        mn = "ani";
                        aS = true;
                        break;
                    case "-nS":
                        if (mn != string.Empty)
                            throw new Exception("ani/nvl selector has already been set in this parameter list.");
                        mn = "nvl";
                        nS = true;
                        break;
                    case "-d":
                        d = true;
                        break;
                    case "-mt":
                        mt = true;
                        break;
                    case "-cc":
                        cc = true;
                        break;
                    case "-c":
                        c = true;
                        break;
                    case "-h":
                        h = true;
                        break;
                    case "-s":
                        s = true;
                        break;
                    case "-e":
                        e = true;
                        break;
                    case "-help":
                        help = true;
                        break;
                    default:
                        term += term.Length > 0 ? $" {str}" : str;
                        break;
                }
            }
            return new Object[] { mn, term, d, mt, cc, h, s, e, help, aS, nS, c };
        }

        static Thread mainWorkerThread;

        private static ExList<string> buffer;
        static int topBuffer = 3; // 3 lines reserved for user input, welcome message, and divider.
        public static void WriteToConsole(string text, bool lineBreaks)
        {
            if (lineBreaks)
                foreach (string str in text.Split('\n').Reverse())
                    buffer.push_back(str);
            else
                buffer.push_back(text);
            string x = buffer.ToString();
            Console.SetCursorPosition(0, topBuffer);
            Console.WriteLine(x);
            Console.SetCursorPosition(0, 0);
        }

        static void Main(string[] args)
        {
            Console.BufferHeight = Console.WindowHeight;
            buffer = new ExList<string>((int)(Console.BufferHeight - 6), true, true);
            Console.CursorVisible = false;
            Console.Write("anime-dl ~ Welcome to anime-dl! -help for help.\n");
            Console.Write(">\n");
            Console.WriteLine(new string('_', Console.WindowWidth));
            if (args.Length <= 0)
            {
                Console.SetCursorPosition(1, 1);
                args = Console.ReadLine().Split(' ');
            }
            object[] parsedArgs = ArgLoop(args);

            if ((bool)parsedArgs[8])
            {
                PrintHelp();
                Console.ReadLine();
                return;
            }

            Restart:
            string selector = ((string)parsedArgs[0]).ToLower();
            switch (selector)
            {
                case "ani":
                    animeDownload(parsedArgs);
                    break;
                case "nvl":
                    novelDownload(parsedArgs);
                    break;
                default:
                    {
                        switch(((string)parsedArgs[1]).SiteFromString())
                        {
                            case Site.HAnime: parsedArgs[0] = "ani"; goto Restart;
                            case Site.Vidstreaming: parsedArgs[0] = "ani"; goto Restart;
                            case Site.ScribbleHub: parsedArgs[0] = "nvl"; goto Restart;
                            case Site.wuxiaWorldA: parsedArgs[0] = "nvl"; goto Restart;
                            case Site.wuxiaWorldB: parsedArgs[0] = "nvl"; goto Restart;
                            case Site.NovelFull: parsedArgs[0] = "nvl"; goto Restart;
                            default:
                                throw new Exception("anime-dl/novel-dl not selected, and I could not auto-detect the downloader to use; please try by specifying nvl or ani.");
                        }
                    }
            }
        }

        static void PrintHelp()
        {
            WriteToConsole(
                new string("ani (use at the start of any search to specify anime-dl)\n" +
                " -d (Specifies download)\n" +
                " -mt (Enables multithreading; unavailable on hanime)\n" +
                " -cc (Enables continuos downloading for HAnime series, experimental)\n" +
                " -c  (Enables skipping already downloaded anime; excludes HAnime)\n" +
                " -h (Specifies HAnime search/download explicitly\n" +
                " -s (Specifies search explicitly\n" +
                "nvl (use at the start of any search to specify novel-dl)\n" +
                " -d (Enables download)\n" +
                " -mt (Enables multithreading; does not work on odd-prime numbers\n" +
                " -e (Specifies to export the novel to epub)\n" +
                "misc:\n" +
                " -aS (specifies anime-dl search without usage of ani at start of arguments)\n" +
                " -nS (specifies novel-dl search without usage of nvl at start of arguments)\n" +
                " -help (cancels everything else and prompts help text)\n" +
                "Example usages:\n" +
                " {alias} {parameters}\n" +
                " ani Godly -d -s             | downloads and searches for anime Godly\n" +
                " Godly -d -s -aS             | Does the same as above\n" +
                " nvl www.wuxiaworld.com/Godly -d | Downloads novel Godly\n"), true);
        }

        static void animeDownload(object[] args)
        {
            if ((bool)args[6])
            {
                if ((bool)args[5])
                {
                    HAnime hanime = new HAnime((string)args[1], false, null, (bool)args[4]);
                }
                else
                {
                    GoGoStream GoGo = new GoGoStream((string)args[1], (bool)args[3], null, (bool)args[11]);
                }
                return;
            }

            Site site = ((string)args[1]).SiteFromString();
            switch (site)
            {
                case Site.Vidstreaming:
                    GoGoStream ggstream = new GoGoStream((string)args[1], (bool)args[3], null);
                    break;
                case Site.HAnime:
                    HAnime hanime = new HAnime((string)args[1], false, null, (bool)args[4]);
                    break;
                default:
                    throw new Exception("Error, site is not supported.");
            }
        }

        static bool bkdwnldF = false;
        static void novelDownload(object[] args)
        {
            if ((bool)args[6] == true)
                throw new Exception("Novel Downloader does not support searching at this time.");
            if ((bool)args[4] == true)
                throw new Exception("Novel Downloader does not support continuos downloads at this time.");

            Book bk;
            if (((string)args[1]).IsValidUri())
            {
                bk = new Book((string)args[1], true);
                bk.ExportToADL();
            }
            else
            {
                bk = new Book((string)args[1], false);
                bkdwnldF = true;
            }

            if ((bool)args[2])
            {
                bk.DownloadChapters((bool)args[3]);
                bk.onDownloadFinish += Bk_onDownloadFinish;
                while (!bkdwnldF)
                    Thread.Sleep(200);
            }

            if ((bool)args[7])
            {
                bk.ExportToEPUB();
                ZipFile.CreateFromDirectory(Directory.GetCurrentDirectory() + "\\Epubs\\" + bk.metaData.name, Directory.GetCurrentDirectory() + "\\Epubs\\" + bk.metaData.name + ".epub");
                Directory.Delete(Directory.GetCurrentDirectory() + "\\Epubs\\" + bk.metaData.name, true);
            }

        }

        private static void Bk_onDownloadFinish()
            => bkdwnldF = true;
    }
}
