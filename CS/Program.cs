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
        static bool Search;
        static bool dwnld;
        public static bool multTthread;
        static String fileDestDirectory;
        static String lnk;
        static void Main(string[] args)
        {
            Download.ConRow = Console.CursorTop;
            Download.ConCol = Console.CursorLeft;
            Storage.wc = new WebClient();
            Storage.client = new HttpClient();
            //Console.ReadLine();
            if(args.Length > 0)
            {
                for (uint idx = 0; idx < args.Length; idx++)
                {
                    switch (args[idx])
                    {
                        case "-help":
                            {
                                Console.WriteLine("~HELP~\nUsage:\nVidStreamIO.exe -S \"anime_name\"   | This will report back all downloaded links for the series found; use with youtube-dl\nParameters:\n-S | Search for the anime with a given name.\n-pD | Download from highest episode to lowest e.g 100 to 0\n-mt | Enables eperimental multi threading.");
                                break;
                            }
                        case "-S":
                            {
                                Search = true;//TRUE;
                            }
                            break;
                        case "-pD": // progressive download.
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
                lnk = args[args.Length - 1];
                Storage.Aniname = lnk;
            }
            else
            {
                bool loop = true;
                Char[][] MESSAGES = new char[0][];
                Char[][] ARGS = new char[0][];
                while (loop)
                {
                    if(MESSAGES.Length > 0)
                    {
                        for(uint i = 0; i < MESSAGES.Length; i++)
                        {
                            Console.WriteLine(new string(MESSAGES[i]));
                        }
                    }
                    Console.WriteLine("Remember: Type \"-help\" for help on command usage.");
                    Console.Write("$:");
                    switch (Console.ReadLine())
                    {
                        case "-help":
                            {
                                Console.WriteLine("~HELP~\nUsage:\nVidStreamIO.exe -S \"anime_name\"   | This will report back all downloaded links for the series found; use with youtube-dl\nParameters:\n-S | Search for the anime with a given name.\n-pD | Download from highest episode to lowest e.g 100 to 0\n-mt | Enables experimental multi-threading\nend | leaves the argument loop");
                                break;
                            }
                        case "-S":
                            {
                                Search = true;
                                MESSAGES = MESSAGES.push_back(new char[] { 'S', 'e', 'a', 'r', 'c', 'h', ' ', 'a', 'c', 't', 'i', 'v', 'e' });
                                break;
                            }
                        case "-pD":
                            {
                                dwnld = true;
                                Directory.CreateDirectory(Directory.GetCurrentDirectory() + "\\vidstream");
                                MESSAGES = MESSAGES.push_back(new char[] { 'd', 'o', 'w', 'n', 'l', 'o', 'a', 'd', ' ', 'a', 'c', 't', 'i', 'v', 'e' });
                                break;
                            }
                        case "-mt": // multi-thread flag
                            {
                                multTthread = true;
                                MESSAGES = MESSAGES.push_back(new char[] { 'm', 'u', 'l', 't', 'i', '-', 't', 'h', 'r', 'e', 'a', 'd', ' ', 'o', 'n' });
                                break;
                            }
                        case "end":
                            {
                                loop = false;
                                break;
                            }
                    }
                }
                Console.Write("\nLink/Name to/of anime: ");

                lnk = Console.ReadLine();
                Storage.Aniname = lnk;
            }

            if (dwnld && Search)
            {
                fileDestDirectory = (Directory.GetCurrentDirectory() + $"\\vidstream\\{lnk}");
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + $"\\vidstream\\{lnk}");
                lnk = VidStreamingMain.Search(lnk);
            }
            else if (dwnld && !Search)
                throw new Exception("Can not have download option without Search option");
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

            string a = VidStreamingMain.FindAllVideos(lnk, dwnld, fileDestDirectory);
            if(a != null)
            {
                Console.WriteLine("Gathering and Exporting direct download Links");
                foreach(String ln in File.ReadAllLines(a))
                {
                    if(ln.Length > 5)
                    {
                        String text = VidStreamingMain.extractDownloadUri(ln);
                        File.AppendAllText($"{fileDestDirectory}", $"\n{text}");
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
