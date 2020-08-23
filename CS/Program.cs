using mshtml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VidStreamIORipper.Sites.VidStreaming;

namespace VidStreamIORipper
{
    class Program
    {
        public static bool Search;
        static bool dwnld;
        public static bool multTthread;
        public static String fileDestDirectory = null;
        static String lnk = null;
        static void Main(string[] args)
        {
            Download.ConRow = Console.CursorTop;
            Download.ConCol = Console.CursorLeft;
            Storage.wc = new WebClient();
            Storage.client = new HttpClient();
            //Console.ReadLine();
            if(args.Length > 0) // Iterate through arguments, but if there are none, skip.
            {
                for (uint idx = 0; idx < args.Length; idx++)
                {
                    switch (args[idx])
                    {
                        case "-help":
                            {
                                Console.WriteLine("~HELP~\nUsage:\nVidStreamIO.exe -S \"anime_name\" -d -mt   | This will report back all downloaded links for the series found; use with youtube-dl\nParameters:\n-S | Search for the anime with a given name.\n-pD | Download from highest episode to lowest e.g 100 to 0\n-mt | Enables eperimental multi threading.");
                                break;
                            }
                        case "-S":
                            {
                                Search = true;//TRUE;
                                lnk = args[idx + 1].Remove('\"');
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
                    }
                }
            }
            else // Request arguments.
            {
                bool loop = true;
                Char[][] MESSAGES = new char[0][]; // don't ask, don't tell.
                Char[][] ARGS = new char[0][];
                Console.WriteLine("Remember: Type \"-help\" for help on command usage.");
                while (loop)
                {
                    if(MESSAGES.Length > 0)
                    {
                        for(uint i = 0; i < MESSAGES.Length; i++)
                        {
                            Console.WriteLine(new string(MESSAGES[i]));
                        }
                    }
                    Console.Write("$:");
                    switch (Console.ReadLine())
                    {
                        case "-help":
                            {
                                Console.WriteLine("~HELP~\nUsage:\nVidStreamIO.exe -S \"anime_name\" -d -mt   | This will report back all downloaded links for the series found; use with youtube-dl\nParameters:\n-S | Search for the anime with a given name.\n-pD | Download from highest episode to lowest e.g 100 to 0\n-mt | Enables experimental multi-threading\nend | leaves the argument loop");
                                break;
                            }
                        case "-S":
                            {
                                switch(Search)
                                {
                                    case true:
                                        Search = !Search;
                                        MESSAGES = MESSAGES.push_back(new char[] { 'S', 'e', 'a', 'r', 'c', 'h', ' ', 'o', 'f', 'f' });
                                        break;
                                    case false:
                                        Search = true;
                                        MESSAGES = MESSAGES.push_back(new char[] { 'S', 'e', 'a', 'r', 'c', 'h', ' ', 'a', 'c', 't', 'i', 'v', 'e' });
                                        Console.Write("Search query: ");
                                        lnk = Console.ReadLine();
                                        Storage.Aniname = lnk;
                                        break;
                                }
                                break;
                            }
                        case "-d":
                            {
                                switch (dwnld)
                                {
                                    case true:
                                        dwnld = false;
                                        Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\vidstream");
                                        MESSAGES = MESSAGES.push_back(new char[] { 'd', 'o', 'w', 'n', 'l', 'o', 'a', 'd', ' ', 'o', 'f', 'f' });
                                        break;
                                    case false:
                                        dwnld = true;
                                        Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\vidstream");
                                        MESSAGES = MESSAGES.push_back(new char[] { 'd', 'o', 'w', 'n', 'l', 'o', 'a', 'd', ' ', 'a', 'c', 't', 'i', 'v', 'e' });
                                        break;
                                }
                                break;
                            }
                        case "-mt": // multi-thread flag
                            {
                                switch (multTthread)
                                {
                                    case true:
                                        multTthread = false;
                                        MESSAGES = MESSAGES.push_back(new char[] { 'm', 'u', 'l', 't', 'i', '-', 't', 'h', 'r', 'e', 'a', 'd', ' ', 'o', 'f', 'f' });
                                        break;
                                    case false:
                                        multTthread = true;
                                        MESSAGES = MESSAGES.push_back(new char[] { 'm', 'u', 'l', 't', 'i', '-', 't', 'h', 'r', 'e', 'a', 'd', ' ', 'o', 'n' });
                                        break;
                                }
                                break;
                            }
                        case "end":
                            {
                                loop = false;
                                break;
                            }
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
