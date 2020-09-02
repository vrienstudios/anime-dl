using mshtml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using VidStreamIORipper.Sites.VidStreaming;

namespace VidStreamIORipper
{
    public static class Download
    {
        public static int ConRow = 0;
        public static int ConCol = 0;
        static int id = 0;
        private static String directUri = string.Empty;
        private static String content = string.Empty;
        private static String m3u8Manifest = string.Empty;

        public static String FileDest = string.Empty;
        public static int AmountTs = 0;


        static int cDownloads = 0;
        public static Char[][] downloadLinks = new char[0][];
        static Char[][] integrityChk = new char[0][];
        static Thread[] iThreads = new Thread[0];
        public static bool dwS = false;
        public static int amount = 0;
        public static void StartDownload()
        {
            dwS = true;
            amount = downloadLinks.Length - 1;
            for(uint idx = 0; idx != downloadLinks.Length; idx++)
            {
                string ix = new string(downloadLinks[idx]);
                //Thread ab = new Thread(() => MultiDownload(VidStreamingMain.extractDownloadUri(ix)));
                String las = VidStreamingMain.extractCloudDUri(ix); // returns null if not set as a variable.
                Thread ab = new Thread(() => MultiDownload(las));
                ab.Name = (idx).ToString();
                iThreads = iThreads.push_back(ab);
                ab.Start();
                cDownloads++;
            }
            Thread allocator = new Thread(TryAllocate);
            allocator.Start();
        }

        private static void TryAllocate()
        {
            while (cDownloads != downloadLinks.Length)
            {
                for (uint id = 0; id < iThreads.Length; id++)
                {
                    if (cDownloads == downloadLinks.Length)
                        break;
                    if (!iThreads[id].IsAlive)
                    {
                        string ix = downloadLinks[cDownloads].ToString();
                        cDownloads++;
                        //iThreads[id] = new Thread(() => MultiDownload(VidStreamingMain.extractDownloadUri(ix)));
                        string els = VidStreamingMain.extractCloudDUri(ix);
                        iThreads[id] = new Thread(() => MultiDownload(els));
                        iThreads[id].Start();
                    }
                }
                Thread.Sleep(500);
            }
        }

        public static void QueueDownload(string lnk)
        {
            cDownloads++;
            downloadLinks = downloadLinks.push_back(lnk.ToCharArray());
        }

        private static Boolean setM3Man(string cont)
        {
            m3u8Manifest = cont;
            return true;
        }

        /// <summary>
        /// I don't remember what this does, so I'll just tell you what I think it does.
        /// This here will get the m3u8 link from the ajax request or from the manifest directly.
        /// </summary>
        /// <param name="linktomanifest"></param>
        /// <returns></returns>
        public static Boolean GetM3u8Link(string linktomanifest) => (Expressions.match = (!linktomanifest.Contains("ajax")) ? Expressions.reg.Matches(content = Storage.wc.DownloadString(linktomanifest)) : Expressions.reg.Matches(content = Storage.wc.DownloadString(directUri = Expressions.dwnldLink.Match(Storage.wc.DownloadString(linktomanifest)).Groups[1].Value.Replace("\\", string.Empty)))) != null ? (Expressions.match.Count > 0) ? setM3Man(Storage.wc.DownloadString($"{((directUri != string.Empty) ? directUri.TrimToSlash() : (directUri = linktomanifest.TrimToSlash()) != null ? directUri : throw new Exception("Unknown Error"))}{GetHighestRes(Expressions.match.GetEnumerator())}")) != false ? DownloadVideo() : throw new Exception("Error getting video information") : false : false;

        public static Boolean MultiDownload(string linktomanifest)
        {
            WebClient wc = new WebClient();
            bool ismp4 = Extensions.IsMp4(linktomanifest);
            if (ismp4)
            {
                Match mc = Regex.Match(linktomanifest, @"episode-(.*?)\.");
                MDownloadVideo(linktomanifest, wc, mc.Groups[1].Value, true);

            }
            else
            {
                if (!linktomanifest.Contains("ajax"))
                {
                    MatchCollection mc = Regex.Matches(wc.DownloadString(linktomanifest), @"(sub\..*?\..*?\.m3u8)");
                    amount--;
                    MDownloadVideo(linktomanifest, wc, (amount + 1).ToString(), Extensions.IsMp4(linktomanifest));
                    //MDownloadVideo($"{linktomanifest.TrimToSlash()}{GetHighestRes(mc.GetEnumerator())}", wc, mc[0].Value.Split('.')[1], Extensions.IsMp4(linktomanifest));

                }
            }

            return true;
        }

        //Get the highest resolution out of all the possible options.
        private static String GetHighestRes(IEnumerator enumerator)
        {
            int current = 0;
            string bi = string.Empty;
            string bf = string.Empty;
            //enumerator.MoveNext(); // First step should be nil, at least it is in CLI
            while(enumerator.MoveNext())
            {
                bf = enumerator.Current.ToString();
                int ia = (int.Parse(bf.Split('.')[2]) > current) ? current = int.Parse(bf.Split('.')[2]) : -1;
                switch (ia)
                {
                    case -1: // not higher break;
                        continue;
                    default:
                        {
                            current = ia;
                            ia = 0;
                            bi = bf;
                            continue;
                        }
                }
            }
            return bf;
        }

        private static String GetUrl(string fileuri)
        {
            // Todo: 
            return fileuri;
        }

        private const int BUFFER_SIZE = 128 * 1024; // Amount of data that we will write at a time.

        private static Boolean MDownloadVideo(string dirURI, WebClient wc, string id, bool mp4)
        {
            int top = Console.CursorTop;
            int dwnl = 0;
            switch (mp4)
            {
                case false:
                    {
                        String a = wc.DownloadString(dirURI);
                        String[] broken = a.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
                        AmountTs = broken.Length / 2;
                        String path = dirURI.TrimToSlash();
                        for (int idx = 0; idx < broken.Length - 1; idx++)
                        {
                            switch (broken[idx][0])
                            {
                                case '#':
                                    {
                                        break; // Header, skip.
                                    }
                                default:
                                    {
                                        WriteAt($"Downloading Part: {AmountTs}~Estimated | {broken[idx]}", 0, top);
                                        mergeToMain($"{Directory.GetCurrentDirectory()}\\vidstream\\{Storage.Aniname}\\{id}_{Storage.Aniname}.mp4", mdownloadPart($"{path}{broken[idx]}", wc, Thread.CurrentThread.Name));
                                        break;
                                    }
                            }
                        }
                        Console.WriteLine($"Finished Downloading {id}");
                        return true;
                    }
                case true:
                    {
                        WriteAt("Downlaoding MP4, this may take a while.,", 0, top);
                        wc.DownloadFile(dirURI, $"{Directory.GetCurrentDirectory()}\\vidstream\\{Storage.Aniname}\\{id}_{Storage.Aniname}.mp4");
                        Console.WriteLine($"Finished Downloading {id}");
                        return true;
                    }
                default:
                    return false;
            }
        }

        private static Boolean DownloadVideo()
        {
            String[] broken = m3u8Manifest.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
            AmountTs = broken.Length / 2;
            String path = directUri.TrimToSlash();
            int top = Console.CursorTop;
            for(int idx = 0; idx < broken.Length; idx++)
            {
                switch (broken[idx][0])
                {
                    case '#':
                        {
                            break; // Header, skip.
                        }
                    default:
                        {
                            WriteAt($"Downloading Part: {id}/{AmountTs}~Estimated | {broken[idx]}", 0, top);
                            mergeToMain(FileDest, downloadPart($"{path}{broken[idx]}"));
                            break;
                        }
                }
            }
            Console.WriteLine($"Finished Downloading {id}_{FileDest}");
            return true;
        }

        //Merge the previously downloaded .ts file into the main .mp4 file.
        private static Boolean mergeToMain(String destinationFile, String partPath)
        {
            Stream stream = new FileStream(partPath, FileMode.Open);
            Stream write = new FileStream(destinationFile, FileMode.Append);
            int c;
            byte[] buffer = new byte[BUFFER_SIZE];

            while ((c = stream.Read(buffer, 0, buffer.Length)) > 0)
                write.Write(buffer, 0, c);

            stream.Close();
            write.Close();

            File.Delete(partPath);
            return false;
        }

        public static void WriteAt(string str, int left, int top)
        {
            Console.SetCursorPosition(ConCol + left, ConRow + top);
            Console.WriteLine(str);
        }

        private static String mdownloadPart(String uri, WebClient wc, string id)
        {
            wc.DownloadFile(uri, $"{Directory.GetCurrentDirectory()}\\{id}.part");
            Download.id++;
            return $"{Directory.GetCurrentDirectory()}\\{id}.part";
        }
        private static String downloadPart(String uri)
        {
            Storage.wc.DownloadFile(uri, $"{Directory.GetCurrentDirectory()}\\a.part");
            id++;
            return $"{Directory.GetCurrentDirectory()}\\a.part";
        }

        private static void m3u8Test(string mfl)
        {
            Console.WriteLine(GetM3u8Link(mfl));
        }

        private static void regTest(string str)
        {
            MatchCollection ma = Expressions.reg.Matches(str);
            Console.WriteLine($"{Expressions.reg.ToString()} : {ma.Count}");
            foreach (Match grp in ma)
                Console.WriteLine(grp.Value);
        }
    }
}
