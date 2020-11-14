using mshtml;
using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using VidStreamIORipper.Sites;
using VidStreamIORipper.Sites.VidStreaming;

namespace VidStreamIORipper
{
    //https://www.youtube.com/watch?v=YkVtWhFOP-g
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
        public static String m3u8MIDA;

        static int cDownloads = 0;
        public static Char[][] downloadLinks = new char[0][];
        static Char[][] integrityChk = new char[0][];
        static Thread[] iThreads = new Thread[0];
        public static bool dwS = false;
        public static int amount = 0;

        public static void StartDownload()
        {
            dwS = true;
            amount = downloadLinks.Length;
            for(uint idx = 0; idx != 2; idx++)
            {
                string ix = new string(downloadLinks[idx]);
                //Thread ab = new Thread(() => MultiDownload(Extractors.extractDownloadUri(ix)));
                String las = Extractors.extractDownloadUri(ix); // returns null if not set as a variable.
                Thread ab = new Thread(() => MultiDownload(las, ix));
                ab.Name = (idx).ToString();
                iThreads = iThreads.push_back(ab);
                ab.Start();
            }
            Thread allocator = new Thread(TryAllocate);
            allocator.Start();
        }

        private static void TryAllocate()
        {
            while (cDownloads != downloadLinks.Length - 1)
            {
                for (uint id = 0; id < iThreads.Length; id++)
                {
                    if (cDownloads == downloadLinks.Length - 1)
                        break;
                    if (!iThreads[id].IsAlive)
                    {
                        string ix = new string(downloadLinks[cDownloads + 1]);
                        //iThreads[id] = new Thread(() => MultiDownload(Extractors.extractDownloadUri(ix)));
                        string els = Extractors.extractDownloadUri(ix);
                        iThreads[id] = new Thread(() => MultiDownload(els, ix));
                        iThreads[id].Start();
                    }
                }
                Thread.Sleep(500);
            }
        }

        public static void QueueDownload(string lnk)
        {
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
        //public static Boolean GetM3u8Link(string linktomanifest) => (Expressions.match = (!linktomanifest.Contains("ajax")) ? Expressions.reg.Matches(content = Storage.wc.DownloadString(linktomanifest)) : Expressions.reg.Matches(content = Storage.wc.DownloadString(directUri = Expressions.dwnldLink.Match(Storage.wc.DownloadString(linktomanifest)).Groups[1].Value.Replace("\\", string.Empty)))) != null ? (Expressions.match.Count > 0) ? setM3Man(Storage.wc.DownloadString($"{((directUri != string.Empty) ? directUri.TrimToSlash() : (directUri = linktomanifest.TrimToSlash()) != null ? directUri : throw new Exception("Unknown Error"))}{GetHighestRes(Expressions.match.GetEnumerator())}")) != false ? DownloadVideo() : throw new Exception("Error getting video information") : false : false;

        public static Boolean GetM3u8Link(string linktomanifest)
        {
            if (!linktomanifest.Contains("ajax"))
            {
                linktomanifest = Extractors.extractDownloadUri(linktomanifest);
                m3u8MIDA = $"https://vidstreaming.io/streaming.php?id={linktomanifest.Split(':')[2]}";
                linktomanifest = "https:" + linktomanifest.Split(':')[1];
                Storage.wc.Headers[HttpRequestHeader.Referer] = m3u8MIDA;
                m3u8Manifest = Storage.wc.DownloadString(linktomanifest);
                directUri = linktomanifest;
                DownloadVideo();
            }
            else
            {
                Console.WriteLine("Error processing.");
            }
            return true;
        }

        public static Boolean MultiDownload(string linktomanifest, string link)
        {
            WebClient wc = new WebClient();
            wc.Headers.Add("Origin", "https://vidstreaming.io");
            String ida = "https://vidstreaming.io/streaming.php?id=" + linktomanifest.Split(':')[2];
            wc.Headers[HttpRequestHeader.Referer] = ida;
            linktomanifest = "https:" + linktomanifest.Split(':')[1];
            bool ismp4 = Extensions.IsMp4(linktomanifest);
            if (ismp4)
            {
                string k = "null";
                Match mc = Regex.Match(linktomanifest, @"episode-(.*?)\.");
                if (mc.Success)
                    k = mc.Groups[1].Value;
                else
                    k = link.getNumStr();
                MDownloadVideo(linktomanifest, wc, k, true, null);

            }
            else
            {
                if (!linktomanifest.Contains("ajax"))
                {
                    MatchCollection mc = Regex.Matches(wc.DownloadString(linktomanifest), @"(sub\..*?\..*?\.m3u8)");
                    amount--;
                    //MDownloadVideo(linktomanifest, wc, (amount + 1).ToString(), Extensions.IsMp4(linktomanifest));
                    MDownloadVideo($"{linktomanifest.TrimToSlash()}{GetHighestRes(mc.GetEnumerator())}", wc, mc[0].Value.Split('.')[1], Extensions.IsMp4(linktomanifest), ida);

                }
            }
            cDownloads++;
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

        private static Boolean MDownloadVideo(string dirURI, WebClient wc, string id, bool mp4, string ida)
        {
            int top = Console.CursorTop;
            int dwnl = 0;
            if(Program.skip)
                if (File.Exists($"{Directory.GetCurrentDirectory()}\\vidstream\\{Storage.Aniname}\\{id}_{Storage.Aniname}.mp4"))
                return true;
            switch (mp4)
            {
                case false:
                    {
                        wc.Headers[HttpRequestHeader.Referer] = ida;
                        String a = wc.DownloadString(dirURI);
                        String[] broken = a.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
                        AmountTs = (broken.Length - 5) / 2;
                        String path = dirURI.TrimToSlash();
                        for (int idx = 0; idx < broken.Length - 1; idx++)
                        {
                            int part = 0;
                            switch (broken[idx][0])
                            {
                                case '#':
                                    {
                                        break; // Header, skip.
                                    }
                                default:
                                    {
                                        WriteAt($"Downloading Part: {AmountTs}~Estimated | {broken[idx]}", 0, top);
                                        mergeToMain($"{Directory.GetCurrentDirectory()}\\vidstream\\{Storage.Aniname}\\{id}_{Storage.Aniname}.mp4", mdownloadPart($"{path}{broken[idx]}", wc, Thread.CurrentThread.Name, ida, id));
                                        break;
                                    }
                            }
                        }
                        Console.WriteLine($"Finished Downloading {id}");
                        return true;
                    }
                case true:
                    {
                        wc.Headers[HttpRequestHeader.Referer] = ida;
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
            if (Program.skip)
                if (File.Exists($"{Directory.GetCurrentDirectory()}\\vidstream\\{Storage.Aniname}\\{amount}_{Storage.Aniname}.mp4"))
                    return true;
            String[] broken = m3u8Manifest.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
            AmountTs = (broken.Length / 2) - 5;
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

        private static String mdownloadPart(String uri, WebClient wc, string id, string ida, string part)
        {
            wc.Headers[HttpRequestHeader.Referer] = ida;
            Download.id++;
            wc.DownloadFile(uri, $"{Directory.GetCurrentDirectory()}\\{part}.part");
            return $"{Directory.GetCurrentDirectory()}\\{part}.part";
        }
        private static String downloadPart(String uri)
        {
            Storage.wc.Headers[HttpRequestHeader.Referer] = m3u8MIDA;
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
