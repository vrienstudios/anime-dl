using System;
using System.IO;
using System.Net;
using System.Net.Http;
using VidStreamIORipper.Sites;
using VidStreamIORipper.Sites.HAnime;

namespace VidStreamIORipper
{
    class Program
    {

        private static String helpText = "~HELP~\n" +
                                    "Usage:\n" +
                                    "     -S anime name -d -mt   | This will download the anime 2 episodes at a time." +
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
                            Storage.Search = true;//TRUE;
                            Storage.Aniname = Storage.lnk;

                            do
                            {
                                idx++;
                                if (Storage.lnk != null)
                                    Storage.lnk += " ";
                                Storage.lnk += args[idx];
                                Storage.Aniname = Storage.lnk;
                            }
                            while (idx < args.Length - 2 && args[idx + 1][0] != '-');

                            break;
                        }
                    case "-d": // progressive download.
                        {
                            Storage.dwnld = true;
                             // || GET_LAST_ERROR == "ALREADY_EXISTS"
                            break;
                        }
                    case "-mt": // multi-thread flag
                        {
                            Storage.multTthread = true;
                            break;
                        }
                    case "-c":
                        {
                            Storage.skip = true;
                            break;
                        }
                    case "-h":
                            Storage.selectedSite = cSites.HAnime;
                        break;
                    default:
                        break;
                }
            }
        }

        static void Main(string[] args)
        {
            Storage.wc = new WebClient();
            Storage.wc.Headers.Add("Origin", "https://vidstreaming.io");
            Storage.client = new HttpClient();
            Storage.client.DefaultRequestHeaders.TryAddWithoutValidation("Origin", "https://vidstreaming.io");
            Storage.selectedSite = cSites.Vidstreaming; //default;
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

            Storage.hostSiteStr = Storage.selectedSite == cSites.HAnime ? "hanime" : "vidstream";
            if (Storage.dwnld && Storage.Search)
            {
                switch (Storage.selectedSite)
                {
                    case cSites.Vidstreaming:
                        {
                            Storage.fileDestDirectory = (Directory.GetCurrentDirectory() + $"\\{Storage.hostSiteStr}\\{Storage.lnk}");
                            //Directory.CreateDirectory(Directory.GetCurrentDirectory() + $"\\{Storage.hostSiteStr}\\{Storage.lnk}");
                            Storage.lnk = Extractors.Search(Storage.lnk);
                            break;
                        }
                    case cSites.HAnime:
                        {
                            Storage.lnk = Extractors.HSearch(Storage.Aniname);
                            Object[] oarr = Extractors.extractHAnimeLink(Storage.lnk);
                            Download.StartDownload((string)oarr[0], Directory.GetCurrentDirectory(), cSites.HAnime, Encryption.AES128, (HentaiVideo)oarr[1]);
                            break;
                        }
                }
            }
            else if (Storage.Search)
            {
                Storage.fileDestDirectory = Directory.GetCurrentDirectory() + $"\\{Storage.hostSiteStr}\\{Storage.lnk}.txt";
                Storage.lnk = Extractors.Search(Storage.lnk);
                if(Storage.lnk == "E")
                {
                    Console.WriteLine("We couldn't find any videos associated with this search term!");
                    Console.ReadLine();
                    return;
                }
            }
            else
            {
                Storage.fileDestDirectory = "T.txt";
            }

            if(Storage.lnk == null)
            {
                Console.Write("Put your link here:");
                Storage.lnk = Console.ReadLine();
                if(Storage.selectedSite == cSites.HAnime)
                {
                    Object[] oarr = Extractors.extractHAnimeLink(Storage.lnk);
                    Download.StartDownload((string)oarr[0], Directory.GetCurrentDirectory(), cSites.HAnime, Encryption.AES128, (HentaiVideo)oarr[1]);
                    //Sites.HAnime.Download.DownloadHAnime((string)oarr[0], null, (Video)oarr[1]);
                }
                else
                {
                    Console.Write("\nPut the anime name here: ");
                    Storage.Aniname = Console.ReadLine();
                }
            }

            if (Storage.selectedSite == cSites.Vidstreaming)
            {
                string a = Extractors.FindAllVideos(Storage.lnk, Storage.dwnld, Storage.fileDestDirectory);
                if (a != null)
                {
                    Console.WriteLine("Gathering and Exporting direct download Links");
                    foreach (String ln in File.ReadAllLines(a))
                    {
                        if (ln.Length > 5)
                        {
                            String text = Extractors.extractDownloadUri(ln);
                            File.AppendAllText($"{Storage.fileDestDirectory}.txt", $"\n{text}");
                        }
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
