using anime_dl.Novels.Models;
using anime_dl.Video.Extractors;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace anime_dl
{
    class Program
    {
        static Object[] ArgLoop(string[] args)
        {
            string mn = string.Empty;
            string term = string.Empty;
            bool d = false, mt = false, cc = false, h = false, s = false, e = false, aS = false, nS = false, help = false;
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
            return new Object[] { mn, term, d, mt, cc, h, s, e, help, aS, nS };
        }

        static void Main(string[] args)
        {
            if (args.Length <= 0)
            { 
                Console.WriteLine("-help for help.\n");
                Console.Write(">");
                args = Console.ReadLine().Split(' ');
            }
            object[] parsedArgs = ArgLoop(args);

            if ((bool)parsedArgs[8])
            {
                PrintHelp();
                Console.ReadLine();
                return;
            }

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
                    throw new Exception("anime-dl/novel-dl not selected, and you specified search. Retry with link to anime/novel or specify downloader or site.");
            }
        }

        static void PrintHelp()
        {
            Console.WriteLine("\n~Help~\n\n" +
                "     ani (use at the start of any search to specify anime-dl)\n" +
                "          -d (Specifies download)\n" +
                "          -mt (Enables multithreading; unavailable on hanime)\n" +
                "          -cc (Enables continuos downloading for HAnime series, experimental)\n" +
                "          -c  (Enables skipping already downloaded anime; excludes HAnime)\n" +
                "          -h (Specifies HAnime search/download explicitly\n" +
                "          -s (Specifies search explicitly\n" +
                "     nvl (use at the start of any search to specify novel-dl)\n" +
                "          -d (Enables download)\n" +
                "          -mt (Enables multithreading; does not work on odd-prime numbers\n" +
                "          -e (Specifies to export the novel to epub)\n" +
                "     misc:\n" +
                "          -aS (specifies anime-dl search without usage of ani at start of arguments)\n" +
                "          -nS (specifies novel-dl search without usage of nvl at start of arguments)\n" +
                "          -help (cancels everything else and prompts help text)\n" +
                "          Example usages:\n" +
                "               {alias} {parameters}\n" +
                "               ani Godly -d -s             | selects anime-dl and passes the search term Godly and tells the anime-dl to download the anime with the -d flag.\n" +
                "               Godly -d -s -aS             | Does the same as above\n" +
                "               www.wuxiaworld.com/Godly -d | Automatically detects downloader and downloads the novel Godly\n");
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
                    GoGoStream GoGo = new GoGoStream((string)args[1], (bool)args[3], null);
                }
                return;
            }

            Site site = ((string)args[1]).SiteFromString();
            switch (site)
            {
                case Site.Vidstreaming:
                    GoGoStream ggstream = new GoGoStream((string)args[1], (bool)args[3]);
                    break;
                case Site.HAnime:
                    HAnime hanime = new HAnime((string)args[1], false, null, (bool)args[4]);
                    break;
            }
        }

        static bool bkdwnldF = false;
        static void novelDownload(object[] args)
        {
            if ((bool)args[6] == true)
                throw new Exception("Novel Downloader does not support searching at this time.");
            if ((bool)args[4] == true)
                throw new Exception("Novel Downloader does not support continuos downloads at this time.");

            Book bk = new Book((string)args[1], true);
            bk.ExportToADL();

            if ((bool)args[2])
                bk.DownloadChapters((bool)args[3]);

            bk.onDownloadFinish += Bk_onDownloadFinish;
            while (!bkdwnldF)
                Thread.Sleep(200);

            if ((bool)args[7])
            {
                bk.ExportToEPUB();
                ZipFile.CreateFromDirectory(Directory.GetCurrentDirectory() + "\\Epubs\\" + bk.metaData.name, Directory.GetCurrentDirectory() + "\\Epubs\\" + bk.metaData.name + ".epub");
                Directory.Delete(Directory.GetCurrentDirectory() + "\\Epubs\\" + bk.metaData.name);
            }

        }

        private static void Bk_onDownloadFinish()
            => bkdwnldF = true;
    }
}
