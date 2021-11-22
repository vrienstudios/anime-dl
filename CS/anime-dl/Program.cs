using ADLCore.Ext;
using ADLCore.Video.Constructs;
using System;
using System.Linq;
using System.Threading;

namespace anime_dl
{
    class Program
    {
        static Thread mainWorkerThread;

        private static ExList<string> buffer;
        private static bool[] tasksRunning;
        public static cTasks concurrentTasks;
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
            buffer.ModifySize(((bufferw - ((topBuffer - 1) * 2)) - concurrentTasks.Length) + 1);
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

        public static void WriteToConsoleC(string text, bool lineBreaks = false, bool refresh = false, bool bypassThreadLock = false)
            => Console.WriteLine(text);

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
            //ArgumentObject parsedArgs = ArgumentObject.Parse(args);  LEGACY
            ArgumentObject parsedArgs = new ArgumentObject(args);
            if (parsedArgs.arguments.help)
            {
                PrintHelp();
                return;
            }
            ADLCore.Interfaces.Main.Execute(parsedArgs, id, UpdateTask);
        }

        static int ctasks = 0;
        private static void CreateNewCommandInstance(string[] arguments)
        {
            if (ctasks >= 3)
            {
                WriteToConsole("E: Too many tasks running, try again later.");
                return;
            }
            int tid = tasksRunning[ctasks] == true ? tasksRunning.ToList().FindLastIndex(x => x == false) : ctasks;
            new Thread(() => {
                #if DEBUG
                concurrentTasks[tid] = "New Task Created!";
                tasksRunning[tid] = true;
                ctasks++;
                parg(arguments, tid);
                concurrentTasks[tid] += " Task Finished";
                #else
                try
                {
                    concurrentTasks[tid] = "New Task Created!";
                    tasksRunning[tid] = true;
                    ctasks++;
                    parg(arguments, tid);
                    concurrentTasks[tid] += " Task Finished";
                    Console.SetCursorPosition(1, 1);
                    tasksRunning[tid] = false;
                    ctasks--;
                    GC.Collect();
                }
                catch(Exception ex)
                {
                    concurrentTasks[tid] = $"Task failed! {ex.Message}";
                }
                finally
                {
                    Console.SetCursorPosition(1, 1);
                    tasksRunning[tid] = false;
                    ctasks--;
                    GC.Collect();
                }
                #endif
            }).Start();
        }

        private static void c(string b)
            => Console.WriteLine(b);
        static void Main(string[] args)
        {
            Quadratic.SolveQuadratic(1,200,-0.000015);
            #if DEBUG
            goto OFD;
            #endif
            if (args.Length > 0)
            {
                if (args[0] == "-msk")
                    goto OFD;
                ADLCore.Alert.ADLUpdates.onSystemUpdate += WriteToConsoleC;
                ADLCore.Alert.ADLUpdates.onSystemLog += c;
                parg(args, 0);
                return;
            }

            Console.WriteLine(help);
            return;

            OFD:;
            ADLCore.Alert.ADLUpdates.msk = true;
            ADLCore.Alert.ADLUpdates.onSystemUpdate += WriteToConsole;
            ADLCore.Alert.ADLUpdates.onThreadDeclaration += ThreadManage;
            concurrentTasks = new cTasks(3, WriteToConsole);
            tasksRunning = new bool[3];
            bufferw = Console.WindowHeight;            
            #if DEBUG
            bufferw = 10;
            #endif
            buffer = new ExList<string>(bufferw - ((topBuffer - 1) * 2), true, true);
            Console.CursorVisible = true;

            new Thread(() => {
                ReadText(new Action<string[]>(CreateNewCommandInstance));
            }).Start(); // Fire and forget.

            Thread.Sleep(100);
            WriteToConsole("Consider helping this project! https://github.com/vrienstudios/anime-dl");
        }

        public static string help = ("ani (use at the start of any search to specify anime-dl)\n" +
                " -d (Specifies download)\n" +
                " -mt (Enables multithreading; unavailable on hanime)\n" +
                " -cc (Enables continuos downloading for HAnime series, experimental)\n" +
                " -c  (Enables skipping already downloaded anime; excludes HAnime)\n" +
                " -h (Specifies HAnime search/download explicitly\n" +
                " -s (Specifies search explicitly\n" +
                " -hS (Specifically searches HAnime\n" +
                " -gS (Specifically searches Gogostream)\n" +
                " -tS (Specifically searches Twist.Moe\n" +
                " -range (allows you to select range of episodes to download, -range 1-13\n" +
                "nvl (use at the start of any search to specify novel-dl)\n" +
                " -d (Enables download)\n" +
                " -mt (Enables multithreading; does not work on odd-prime numbers\n" +
                " -e (Specifies to export the novel to epub)\n" +
                "misc:\n" +
                " -help (cancels everything else and prompts help text)\n" +
                "Example usages:\n" +
                " {alias} {parameters}\n" +
                " ani Godly -d -s             | downloads and searches for anime Godly\n" +
                " Godly -d -s -aS             | Does the same as above\n" +
                " nvl www.wuxiaworld.com/Godly -d | Downloads novel Godly\n" +
                "Note: To return to 3 multi-task version, start the command-line app with -msk with no other paramaters.\n" +
                "E.x anime-dl.exe -msk");
        static void PrintHelp()
        {
            WriteToConsole(help, true);
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

        private static void UpdateTask(int ti, string m)
        {
            if (concurrentTasks == null)
                return;
            concurrentTasks[ti] = m;
            WriteToConsole(null, false);
        }
    }
}
