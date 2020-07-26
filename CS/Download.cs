using mshtml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace VidStreamIORipper
{
    public static class Dwnl
    {                       // sub.8.360.m3u8
        private static Regex reg = new Regex(@"(sub\..*?\..*?\.m3u8)");
        private static Regex dwnldLink = new Regex("(?<=\"file\":\")(.+?)(?=\")");
        private static MatchCollection match;
        private static String directUri = string.Empty;
        private static String content = string.Empty;
        private static String m3u8Manifest = string.Empty;

        public static String FileDest = string.Empty;

        private static Boolean setM3Man(string cont)
        {
            m3u8Manifest = cont;
            return true;
        }

        public static Boolean GetM3u8Link(string linktomanifest) => (match = (!linktomanifest.Contains("ajax")) ? reg.Matches(content = Program.wc.DownloadString(linktomanifest)) : reg.Matches(content = Program.wc.DownloadString(directUri = dwnldLink.Match(Program.wc.DownloadString(linktomanifest)).Groups[1].Value.Replace("\\", string.Empty)))) != null ? (match.Count > 0) ? setM3Man(Program.wc.DownloadString($"{((directUri != string.Empty) ? directUri.TrimToSlash() : (directUri = linktomanifest.TrimToSlash()) != null ? directUri : throw new Exception("Unknown Error"))}{GetHighestRes(match.GetEnumerator())}")) != false ? DownloadVideo() : throw new Exception("Error getting video information") : false : false;

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

        private const int BUFFER_SIZE = 128 * 1024;

        private static Boolean DownloadVideo()
        {
            String[] broken = m3u8Manifest.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
            String path = directUri.TrimToSlash();
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
                            Console.WriteLine($"Downloading: {broken[idx]}");
                            mergeToMain(FileDest, downloadPart($"{path}{broken[idx]}"));
                            break;
                        }
                }
            }
            return true;
        }

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

        private static String downloadPart(String uri)
        {
            Program.wc.DownloadFile(uri, $"{Directory.GetCurrentDirectory()}\\Vidstreaming.part");
            return $"{Directory.GetCurrentDirectory()}\\Vidstreaming.part";
        }

        private static void m3u8Test(string mfl)
        {
            Console.WriteLine(GetM3u8Link(mfl));
        }

        private static void regTest(string str)
        {
            MatchCollection ma = reg.Matches(str);
            Console.WriteLine($"{reg.ToString()} : {ma.Count}");
            foreach (Match grp in ma)
                Console.WriteLine(grp.Value);
        }
    }
}
