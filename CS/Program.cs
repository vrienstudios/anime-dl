using System;
using System.IO;
using System.Net;
using System.Net.Http;
using VidStreamIORipper.Sites.VidStreaming;

namespace VidStreamIORipper
{
    class Program
    {
        public static bool Search;
        static bool dwnld;
        public static bool multTthread;
        public static bool skip;
        static bool cntD;
        public static String fileDestDirectory = null;
        static String lnk = null;

        private static String helpText = "~HELP~\n" +
                                    "Usage:\n" +
                                    "     -S anime_name -d -mt   | This will download the anime 2 episodes at a time." +
                                    "\nParameters:\n" +
                                    "     -S | Search for the anime with a given name.\n" +
                                    "     -d | Download the anime\n" +
                                    "     -mt | Enables experimental multi-threading\n" +
                                    "     -c | Skip any files already downloaded/continue download\n" +
                                    "     end | leaves the argument loop\n";

        private static void setArgs(string[] args)
        {
            for (uint idx = 0; idx < args.Length; idx++)
            {
                switch (args[idx])
                {
                    case "-help":
                        {
                            Console.WriteLine(helpText);
                            break;
                        }
                    case "-S":
                        {
                            Search = true;//TRUE;
                            lnk = args[idx + 1];
                            Storage.Aniname = lnk;
                        }
                        break;
                    case "-d": // progressive download.
                        {
                            dwnld = true;
                            Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\vidstream"); // || GET_LAST_ERROR == "ALREADY_EXISTS"
                            break;
                        }
                    case "-mt": // multi-thread flag
                        {
                            multTthread = true;
                            break;
                        }
                    case "-c":
                        {
                            cntD = true;
                            break;
                        }
                }
            }
        }

        static void Main(string[] args)
        {
            Download.ConRow = Console.CursorTop;
            Download.ConCol = Console.CursorLeft;
            Storage.wc = new WebClient();
            Storage.wc.Headers.Add("Origin", "https://vidstreaming.io");
            Storage.client = new HttpClient();
            Storage.client.DefaultRequestHeaders.TryAddWithoutValidation("Origin", "https://vidstreaming.io");
            //Console.ReadLine();
            if(args.Length > 0) // Iterate through arguments, but if there are none, skip.
            {
                setArgs(args);
            }
            else // Request arguments.
            {
                Console.WriteLine("Put your arguments here, or type help for help.");
                while (true)
                {
                    Console.Write("$:");
                    String t = Console.ReadLine();
                    if (t == "help")
                        Console.WriteLine(helpText + "\n\n");
                    else
                    {
                        Console.Write("\nIs this correct? y/n:");
                        if (Console.ReadLine().ToUpper() == "Y")
                        {
                            setArgs(t.Split(' '));
                            break;
                        }
                        else
                            Console.Clear();
                    }
                }
            }


            if (dwnld && Search)
            {
                fileDestDirectory = (Directory.GetCurrentDirectory() + $"\\vidstream\\{lnk}");
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + $"\\vidstream\\{lnk}");
                lnk = VidStreamingMain.Search(lnk);
            }
            else if (Search)
            {
                fileDestDirectory = Directory.GetCurrentDirectory() + $"\\vidstream\\{lnk}.txt";
                lnk = VidStreamingMain.Search(lnk);
                if(lnk == "E")
                {
                    Console.WriteLine("We couldn't find any videos associated with this search term!");
                    Console.ReadLine();
                    return;
                }
            }
            else
            {
                fileDestDirectory = "T.txt";
            }

            if(lnk == null)
            {
                Console.Write("Put your link here: ");
                lnk = Console.ReadLine();
                Console.Write("\nPut the anime name here: ");
                Storage.Aniname = Console.ReadLine();
                //Console.Write("put the folder name here: ");
            }

            string a = VidStreamingMain.FindAllVideos(lnk, dwnld, fileDestDirectory);
            if(a != null)
            {
                Console.WriteLine("Gathering and Exporting direct download Links");
                foreach(String ln in File.ReadAllLines(a))
                {
                    if(ln.Length > 5)
                    {
                        String text = VidStreamingMain.extractDownloadUri(ln);
                        File.AppendAllText($"{fileDestDirectory}.txt", $"\n{text}");
                    }
                }
            }
            Console.ReadLine();
        }

        ~Program()
        {
            Storage.client.Dispose();
            Storage.wc.Dispose();
        }
    }
}
