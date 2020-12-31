using System;

namespace anime_dl
{
    class Program
    {
        static Object[] ArgLoop(string[] args)
        {
            string mn = string.Empty;
            string term = string.Empty;
            bool d = false, mt = false, cc = false, h = false, s = false, aS = false, nS = false;
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
                    default:
                        term = str;
                        break;
                }
            }
            return new Object[] { mn, term, d, mt, cc, h, s, aS, nS };
        }

        static void Main(string[] args)
        {
            object[] parsedArgs = ArgLoop(args);
            string selector = ((string)parsedArgs[0]).ToLower();
            PrintHelp();
            switch (selector)
            {
                case "ani":
                    break;
                case "nvl":
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
                "          -h (Specifies HAnime search/download explicitly\n" +
                "          -s (Specifies search explicitly\n" +
                "     nvl (use at the start of any search to specify novel-dl)\n" +
                "          -d (Enables download)\n" +
                "          -mt (Enables multithreading; does not work on odd-prime numbers\n" +
                "     misc:\n" +
                "          -aS (specifies anime-dl search without usage of ani at start of arguments)\n" +
                "          -nS (specifies novel-dl search without usage of nvl at start of arguments)\n" +
                "          Example usages:\n" +
                "               {alias} {parameters}\n" +
                "               ani Godly -d -s             | selects anime-dl and passes the search term Godly and tells the anime-dl to download the anime with the -d flag.\n" +
                "               Godly -d -s -aS             | Does the same as above\n" +
                "               www.wuxiaworld.com/Godly -d | Automatically detects downloader and downloads the novel Godly\n");
        }

        static void novelDownload(string[] args)
        {

        }
    }
}
