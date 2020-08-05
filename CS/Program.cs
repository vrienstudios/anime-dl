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
        static String fileDestDirectory;
        static String lnk;
        static void Main(string[] args)
        {
            Download.ConRow = Console.CursorTop;
            Download.ConCol = Console.CursorLeft;
            Storage.wc = new WebClient();
            Storage.wc.DownloadProgressChanged += Wc_DownloadProgressChanged;
            Storage.client = new HttpClient();
            //Console.ReadLine();
            for (int idx = 0; idx < args.Length; idx++)
            {
                switch (args[idx])
                {
                    case "-help":
                        {
                            Console.WriteLine("~HELP~\nUsage:\nVidStreamIO.exe -S \"anime_name\"   | This will report back all downloaded links for the series found; use with youtube-dl\nParameters:\n-S | Search for the anime with a given name.\n-pD | Download from highest episode to lowest e.g 100 to 0");
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
                }
            }

            lnk = args[args.Length - 1];
            Storage.Aniname = lnk;

            if (dwnld && Search)
            {
                fileDestDirectory = (Directory.GetCurrentDirectory() + $"\\vidstream\\{lnk}");
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + $"\\vidstream\\{lnk}");
            }
            else if(dwnld)
                throw new Exception("Can not have download option without Search option");

            if (Search)
                lnk = VidStreamingMain.Search(args[args.Length - 1]);

            VidStreamingMain.FindAllVideos(lnk, dwnld, fileDestDirectory);

            //Console.ReadLine();
        }

        private static void Wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Console.WriteLine("AA");
            Download.WriteAt($"{e.BytesReceived}/{e.TotalBytesToReceive}", 0, Download.ConCol);
        }

        ~Program()
        {
            Storage.client.Dispose();
            Storage.wc.Dispose();
        }
    }
}
