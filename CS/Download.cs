using mshtml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VidStreamIORipper
{
    public static class Download
    {                       // sub.8.360.m3u8
        private static Regex reg = new Regex(@"(sub\..*?\..*?\.m3u8)");
        private static Regex dwnldLink = new Regex("(?<=\"file\":\")(.+?)(?=\")");
        private static MatchCollection match;
        private static String directUri = string.Empty;
        private static String content = string.Empty;
        private static String m3u8Manifest = string.Empty;
        private static Boolean setM3Man(string cont)
        {
            m3u8Manifest = cont;
            return true;
        }

        private static void MergeTS(string path)
        {
            // TODO
        }

        private static void DownloadTS(string tsListFile)
        {
            // TODO
        }

        private static Boolean GetM3u8Link(string linktomanifest) => (match = (!linktomanifest.Contains("ajax")) ? reg.Matches(content = Program.wc.DownloadString(linktomanifest)) : reg.Matches(content = Program.wc.DownloadString(directUri = dwnldLink.Match(Program.wc.DownloadString(linktomanifest)).Groups[1].Value.Replace("\\", string.Empty)))) != null ? (match.Count > 0) ? setM3Man(Program.wc.DownloadString($"{((directUri != string.Empty) ? directUri.TrimToSlash() : content)}{GetHighestRes(match.GetEnumerator())}")) != false ? DownloadVideo() : false : false : false;

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

        public static Boolean DownloadVideo()
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
                            mergeToMain(downloadPart($"{path}{broken[idx]}"));
                            break;
                        }
                }
            }
            return true;
        }

        private static Boolean mergeToMain(String partPath)
        {
            return false;
        }

        private static String downloadPart(String uri)
        {
            return null;
        }

        public static void m3u8Test(string mfl)
        {
            Console.WriteLine(GetM3u8Link(mfl));
        }

        public static void regTest(string str)
        {
            MatchCollection ma = reg.Matches(str);
            Console.WriteLine($"{reg.ToString()} : {ma.Count}");
            foreach (Match grp in ma)
                Console.WriteLine(grp.Value);
        }
    }
}
