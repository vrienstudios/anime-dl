using mshtml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using VidStreamIORipper.Sites;
using VidStreamIORipper.Sites.HAnime;

namespace VidStreamIORipper.Sites
{
    public static class Download
    {
        static int cDownloads = 0;
        public static Char[][] downloadLinks = new char[0][];
        private static List<HentaiVideo> Videos = new List<HentaiVideo>();
        static Char[][] integrityChk = new char[0][];
        static Thread[] iThreads = new Thread[0];

        private static Object[] headers = new object[2] { null, null };

        public static void QueueDownload(string lnk, HentaiVideo hv)
        {
            downloadLinks = downloadLinks.push_back(lnk.ToCharArray());
            Videos.Add(hv);
        }

        public static void StartMTDownload()
        {
            for (int idx = 0; idx < 2; idx++)
            {
                string ix = new string(downloadLinks[idx]);
                //Thread ab = new Thread(() => MultiDownload(Extractors.extractDownloadUri(ix)));
                HentaiVideo vid = Videos[idx];
                Thread ab = new Thread(() => StartDownload(ix, Directory.GetCurrentDirectory() + "\\" + Storage.hostSiteStr + "\\" + vid.brand, cSites.Vidstreaming, Encryption.None, vid));
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
                        cDownloads++;
                        string ix = new string(downloadLinks[cDownloads + 1]);
                        //iThreads[id] = new Thread(() => MultiDownload(Extractors.extractDownloadUri(ix)));
                        HentaiVideo hv = Videos[cDownloads + 1];
                        iThreads[id] = new Thread(() => StartDownload(ix, Directory.GetCurrentDirectory() + "\\" + Storage.hostSiteStr + "\\" + hv.brand, cSites.Vidstreaming, Encryption.None, hv));
                        iThreads[id].Start();
                    }
                }
                Thread.Sleep(500);
            }
        }

        public static void StartDownload(String linktomanifest, String destination, cSites site, Encryption enc, HentaiVideo anime = null, string alt = null, bool highestres = true, string key = null, string iv = null)
        {
            HentaiVideo hv = anime == null ? new HentaiVideo() : anime;
            hv.name = hv.name.RemoveSpecialCharacters();
            switch (site)
            {
                case cSites.Vidstreaming:
                    {
                        Directory.CreateDirectory(destination);
                        if (Storage.skip)
                            if (File.Exists(destination + "\\" + hv.name + ".mpg") || File.Exists(destination + "\\" + hv.name + ".mp4"))
                                return;
                        Object[] oa = GetVidstreamingManifestToStream(Extractors.extractDownloadUri(linktomanifest), alt);
                        hv.slug = (string)oa[0];
                        hv.ismp4 = (bool)oa[1];
                        try
                        {
                            VidstreamingDownload(hv, destination);
                        }
                        catch
                        {
                            Console.WriteLine("Failed to getvideo from vidstreaming server -> moving to fallback cloud9");
                            hv.slug = Extractors.extractCloudDUri(linktomanifest);
                            VidstreamingDownload(hv, destination, true);
                        }
                        
                        break;
                    }
                case cSites.HAnime:
                    {
                        Directory.CreateDirectory(destination + "\\" + Storage.hostSiteStr + "\\");
                        WebClient wc = new WebClient();
                        String[] oa = wc.DownloadString(linktomanifest).Split(new string[] { "\r", "\r\n", "\n"}, StringSplitOptions.None);
                        int sequence = 0;
                        int length = (oa.Length / 2) - 4;
                        for(int idx = 0; idx < oa.Length; idx++)
                        {
                            if (oa[idx][0] != '#')
                            {
                                Console.WriteLine("Downloading Part: {0} of {1} for {2}", sequence, length, hv.name);
                                mergeToMain(decodePartAES128(wc.DownloadData(oa[idx]), "0123456701234567", sequence++), destination + "\\" + Storage.hostSiteStr + "\\" + hv.name + ".mpg");
                            }

                        }
                        Console.Write("\nEnjoy the hentai, finished!\n     Next Hentai: {0}\nWould you like to download the next hentai?\n(y/n) $: ", Storage.videoObj.next_hentai_video.name);
                        if(Console.ReadLine().ToUpper() == "Y")
                        {
                            Object[] ar = Extractors.extractHAnimeLink($"https://hanime.tv/videos/hentai/{Storage.videoObj.next_hentai_video.slug}");
                            StartDownload((string)ar[0], Directory.GetCurrentDirectory(), cSites.HAnime, Encryption.AES128, (HentaiVideo)ar[1]);
                        }
                        break;
                    }
            }
        }

        private static Byte[] decodePartAES128(Byte[] data, string key, int sequence)
        {
            byte[] iv = sequence.ToBigEndianBytes();
            iv = new byte[8].Concat(iv).ToArray();

            // HLS uses AES-128 w/ CBC & PKCS7
            RijndaelManaged algorithm = new RijndaelManaged()
            {
                Padding = PaddingMode.PKCS7,
                Mode = CipherMode.CBC,
                KeySize = 128,
                BlockSize = 128
            };

            algorithm.Key = Encoding.ASCII.GetBytes(key);
            algorithm.IV = iv;

            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, algorithm.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(data, 0, data.Length);

            GC.Collect();

            return ms.ToArray();
        }

        private static void VidstreamingDownload(HentaiVideo vid, String destination, bool r = false)
        {
            if (vid.ismp4 == true)
            {
                WebClient wc = createNewWebClient();
                Console.WriteLine("Downloading: {0}", vid.slug);
                wc.DownloadFile(vid.slug, $"{destination}\\{vid.name}.mp4");
                Console.WriteLine($"Finished Downloading: {vid.name}");
                return;
            }
            else
            {
                String[] manifestData;
                String basePath = r == false ? vid.slug.TrimToSlash() : string.Empty;

                using (WebClient wc = createNewWebClient())
                    manifestData = wc.DownloadString(vid.slug).Split(new string[] { "\n", "\r\n", "\r" }, StringSplitOptions.None);

                int id = 1;
                for(int idx = 0; idx < manifestData.Length; idx++)
                {
                    if(manifestData[idx][0] != '#')
                    {
                        using(WebClient wc = createNewWebClient())
                            mergeToMain(wc.DownloadData(basePath + manifestData[idx]), $"{destination}\\{vid.name}.mpg");
                        Console.WriteLine($"Downloaded {id++}/{(manifestData.Length / 2) - 5} for {vid.name}");
                    }
                }
            }
        }

        private static bool mergeToMain(Byte[] data, String file)
        {
            if (!File.Exists(file))
                File.Create(file).Close();
            using (FileStream fs = new FileStream(file, FileMode.Append))
                fs.Write(data, 0, data.Length);
            return true;
        }

        private static WebClient createNewWebClient()
        {
            WebClient wc = new WebClient();
            wc.Headers.Add((string)((object[])headers[0])[0], (string)((object[])headers[0])[1]);
            wc.Headers[HttpRequestHeader.Referer] = (string)headers[1];
            return wc;
        }

        private static Object[] GetVidstreamingManifestToStream(string link, string alt, bool highestres = true, string id = null)
        {
            if (headers[0] == null)
            {
                String ida = "https://vidstreaming.io/streaming.php?id=" + link.Split(':')[2];
                headers[0] = new object[] { "Origin", "https://vidstreaming.io" };
                headers[1] = ida;
            }
            link = "https:" + link.Split(':')[1];

            if (Extensions.IsMp4(link))
                return new object[] { link, true };

            WebClient wc = createNewWebClient();

            if (Extensions.IsMp4(link))
            {
                string k = "null";
                Match mc = Regex.Match(wc.DownloadString(link), @"episode-(.*?)\.");
                if (mc.Success)
                    k = mc.Groups[1].Value;
                else
                    k = alt.getNumStr();
                wc.Dispose();
                return new object[2] { link, true };

            }
            else
            {
                MatchCollection mc = Regex.Matches(wc.DownloadString(link), @"(sub\..*?\..*?\.m3u8)");
                wc.Dispose();
                return new object[2] { $"{link.TrimToSlash()}{GetHighestRes(mc.GetEnumerator())}", false};
            }

        }

        //Get the highest resolution out of all the possible options.
        private static String GetHighestRes(IEnumerator enumerator)
        {
            int current = 0;
            string bi = string.Empty;
            string bf = string.Empty;
            //enumerator.MoveNext(); // First step should be nil, at least it is in CLI
            while (enumerator.MoveNext())
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
    }
}
