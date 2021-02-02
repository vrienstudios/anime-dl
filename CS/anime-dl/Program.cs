using ADLCore;
using ADLCore.Ext;
using ADLCore.Novels.Models;
using ADLCore.Video.Constructs;
using ADLCore.Video.Extractors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace anime_dl
{
    class Program
    {
        static ArgumentObject ArgLoop(string[] args)
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
            return new ArgumentObject(new Object[] { mn, term, d, mt, cc, h, s, e, help, aS, nS, c });
        }

        static Thread mainWorkerThread;

        private static ExList<string> buffer;
        private static bool[] tasksRunning;
        public static string[] concurrentTasks;
        static int topBuffer = 3; // 3 lines reserved for user input, welcome message, and divider.
        static int bufferw = 100;
        private static bool _pause = false;
        private static object locker = new object();

        private static void awaitThreadUnlock()
        {
            lock (locker)
                Monitor.Wait(locker);
        }
        public static void WriteToConsole(string text, bool lineBreaks = false, bool refresh = false, bool bypassThreadLock = false)
        {
            if(!bypassThreadLock)
                if(_pause)
                    lock (locker)
                    {
                        Monitor.Wait(locker);
                    }

            int running = 0;
            for(int idx = 0; idx < concurrentTasks.Length; idx++)
            {
                if (tasksRunning[idx] == true)
                {
                    Console.SetCursorPosition(0, topBuffer + idx);
                    running++;
                    int d = Console.BufferWidth - concurrentTasks[idx].Length;
                    Console.Write($"{concurrentTasks[idx]}{new string(' ', d < 0 ? 0 : d)}");
                }
            }
            if (text == null && refresh == false)
                return;
            buffer.ModifySize((bufferw - ((topBuffer - 1) * 2)) - concurrentTasks.Length);
            if (lineBreaks)
                foreach (string str in text.Split('\n').Reverse())
                    buffer.push_back(str);
            else
                buffer.push_back(text);

            string x = buffer.ToString();

            Console.SetCursorPosition(0, running > 0 ? topBuffer + running : topBuffer);
            Console.Write(x);
            WriteTop();
            Console.SetCursorPosition(0, 0);
        }

        private static void WriteTop()
        {
            Console.SetCursorPosition(0, 0);
            Console.Write("anime-dl ~ Welcome to anime-dl! -help for help.\r\n");
            Console.Write(">\r\n");
            Console.Write(new string('_', Console.WindowWidth) + "\r\n");
            Console.SetCursorPosition(1, 1);
        }

        public static void ReadText(Action<string[]> action)
        {
            string ab = string.Empty;
            while (true)
            {
                WriteTop();
                ConsoleKeyInfo a = Console.ReadKey();
                switch (a.Key)
                {
                    case ConsoleKey.Enter:
                        action.Invoke(ab.Split(' '));
                        ab = string.Empty;
                        UpdateUserInput(ab);
                        break;
                    case ConsoleKey.Backspace:
                        ab = (ab.Length > 0) ? ab.Remove(ab.Length - 1) : string.Empty;
                        break;
                    default:
                        ab += a.KeyChar;
                        break;
                }
                UpdateUserInput(ab);
            }
        }

        private static void UpdateUserInput(string input)
        {
            Console.SetCursorPosition(1, 1);
            Console.Write(new string(' ', Console.BufferWidth) + "\r");
            Console.SetCursorPosition(1, 1);
            Console.Write(input + "\r");
        }

        static void parg(string[] args, int id)
        {
            ArgumentObject parsedArgs = ArgLoop(args);

            if (parsedArgs.help)
            {
                PrintHelp();
                return;
            }

        Restart:
            string selector = (parsedArgs.mn.ToLower());
            switch (selector)
            {
                case "ani":
                    animeDownload(ref parsedArgs, id);
                    break;
                case "nvl":
                    novelDownload(ref parsedArgs, id);
                    break;
                default:
                    {
                        switch (parsedArgs.term.SiteFromString())
                        {
                            case Site.HAnime: parsedArgs[0] = "ani"; goto Restart;
                            case Site.Vidstreaming: parsedArgs[0] = "ani"; goto Restart;
                            case Site.ScribbleHub: parsedArgs[0] = "nvl"; goto Restart;
                            case Site.wuxiaWorldA: parsedArgs[0] = "nvl"; goto Restart;
                            case Site.wuxiaWorldB: parsedArgs[0] = "nvl"; goto Restart;
                            case Site.NovelFull: parsedArgs[0] = "nvl"; goto Restart;
                            default:
                                WriteToConsole("Error: could not parse command (Failure to parse website to ani/nvl flag.. you can retry with ani/nvl flag)");
                                return;
                        }
                    }
            }
        }

        static int ctasks = 0;
        private static void CreateNewCommandInstance(string[] arguments)
        {
            if (ctasks >= 3)
            {
                WriteToConsole("E: Too many tasks running, try again later.");
                return;
            }
            Thread a = new Thread(() => {
                int tid = tasksRunning[ctasks] == true ? tasksRunning.ToList().FindLastIndex(x => x == false) : ctasks;
                concurrentTasks[tid] = "New Task Created!";
                tasksRunning[tid] = true;
                ctasks++;
                parg(arguments, tid);
                concurrentTasks[tid] += " Task Finished";
                WriteToConsole(null, false);
                Console.SetCursorPosition(1, 1);
                tasksRunning[tid] = false;
                ctasks--;
                GC.Collect();
            });
            a.Start();
        }

        static void Main(string[] args)
        {
            ADLCore.Alert.ADLUpdates.onSystemUpdate += WriteToConsole;
            ADLCore.Alert.ADLUpdates.onThreadDeclaration += ThreadManage;

            concurrentTasks = new string[3];
            tasksRunning = new bool[3];
            bufferw = Console.WindowHeight;
            buffer = new ExList<string>(bufferw - ((topBuffer - 1) * 2), true, true);
            Console.CursorVisible = true;

            mainWorkerThread = new Thread(() => {
                ReadText(new Action<string[]>(CreateNewCommandInstance));
            });
            mainWorkerThread.Start();
        }

        static void PrintHelp()
        {
            WriteToConsole(("ani (use at the start of any search to specify anime-dl)\n" +
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
                " nvl www.wuxiaworld.com/Godly -d | Downloads novel Godly"), true);
        }
        //   0    1   2  3   4   5  6  7   8    9   10  11
        //{ mn, term, d, mt, cc, h, s, e, help, aS, nS, c };
        static void animeDownload(ref ArgumentObject args, int taski)
        {
            ////return new ArgumentObject(new Object[] { mn, term, d, mt, cc, h, s, e, help, aS, nS, c });
            if (args.s)
            {
                if (args.h)
                {
                    HAnime hanime = new HAnime(args.term, args.mt, null, args.cc, taski, new Action<int, string>(UpdateTask));
                    hanime.Begin();
                }
                else
                {
                    GoGoStream GoGo = new GoGoStream(args.term, args.mt, null, args.c, taski, new Action<int, string>(UpdateTask));
                }
                return;
            }

            Site site = args.term.SiteFromString();
            switch (site)
            {
                case Site.Vidstreaming:
                    GoGoStream ggstream = new GoGoStream(args.term, args.mt, null, args.c, taski, new Action<int, string>(UpdateTask));
                    break;
                case Site.HAnime:
                    HAnime hanime = new HAnime(args.term, args.mt, null, args.cc, taski, new Action<int, string>(UpdateTask));
                    if (!args.d)
                    {
                        UpdateTask(taski, $"{args.term.SkipCharSequence("https://hanime.tv/videos/hentai/".ToCharArray())} {hanime.GetDownloadUri(args.term)}");
                    }
                    else
                        hanime.Begin();
                    break;
                default:
                    throw new Exception("Error, site is not supported, site not detected.|" + args.term);
            }

            return;
        }

        public static void ThreadManage(bool lockresume)
        {
            if (lockresume)
                _pause = true;
            else
            {
                _pause = false;
                lock (locker)
                    Monitor.PulseAll(locker);
            }
        }

        static void novelDownload(ref ArgumentObject args, int taski)
        {
            bool dwnldFinishedFlag = false;
            if (args.s)
                throw new Exception("Novel Downloader does not support searching at this time.");
            if (args.cc)
                throw new Exception("Novel Downloader does not support continuos downloads at this time.");

            Book bk;
            if (args.term.IsValidUri())
            {
                bk = new Book(args.term, true, taski, new Action<int, string>(UpdateTask));
                bk.ExportToADL();
            }
            else
            {
                bk = new Book(args.term, false, taski, new Action<int, string>(UpdateTask));
                bk.dwnldFinished = true;
            }

            if (args.d)
            {
                bk.DownloadChapters(args.mt);
                while (!bk.dwnldFinished)
                    Thread.Sleep(200);
            }

            if (args.e)
            {
                bk.ExportToEPUB(Path.Join(Directory.GetCurrentDirectory(), "Epubs", bk.metaData.name));
                concurrentTasks[taski] = $"{bk.metaData.name} exported to epub successfully!";
            }

        }

        private static void UpdateTask(int ti, string m)
        {
            concurrentTasks[ti] = m;
            WriteToConsole(null, false);
        }
    }
}
