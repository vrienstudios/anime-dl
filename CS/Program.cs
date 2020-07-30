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
            Storage.wc = new WebClient();
            Storage.client = new HttpClient();
            //Console.ReadLine();
            for (int idx = 0; idx < args.Length; idx++)
            {
                switch (args[idx])
                {
                    case "-help":
                        {
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
            if (dwnld && Search)
            {
                fileDestDirectory = (Directory.GetCurrentDirectory() + $"\\vidstream\\{args[args.Length - 1]}");
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + $"\\vidstream\\{args[args.Length - 1]}");
            }
            else if(dwnld)
                throw new Exception("Can not have download option without Search option");

            if (Search)
                lnk = VidStreamingMain.Search(args[args.Length - 1]);
            else
                lnk = args[args.Length - 1];

            Storage.Aniname = lnk;
            VidStreamingMain.FindAllVideos(lnk, dwnld, fileDestDirectory);

            //Console.ReadLine();
        }
        ~Program()
        {
            Storage.client.Dispose();
            Storage.wc.Dispose();
        }
    }
}
