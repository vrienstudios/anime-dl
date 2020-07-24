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
        private static MatchCollection match;
        private static String content = string.Empty;
        private static void MergeTS(string path)
        {
            // TODO
        }

        private static void DownloadTS(string tsListFile)
        {
            // TODO
        }

        private static String GetM3u8Link(string linktomanifest) => (match = reg.Matches(content = Program.wc.DownloadString(linktomanifest))) != null ? (match.Count > 0) ? $"{GetUrl(content)}{GetHighestRes(match.GetEnumerator())}" : null : null;

        private static String GetHighestRes(IEnumerator enumerator)
        {
            int current = 0;
            string bf = string.Empty;
            enumerator.MoveNext(); // First step should be nil, at least it is in CLI
            while(enumerator.MoveNext())
            {
                int ia = (int.Parse(bf.Split('.')[2]) > current) ? int.Parse(bf.Split('.')[2]) : -1;
                switch (ia)
                {
                    case -1: // not higher break;
                            break;
                    default:
                        {
                            current = ia;
                            ia = 0;
                            break;
                        }
                }
            }

            return null;
        }

        private static String GetUrl(string fileuri)
        {
            // Todo: 
            return null;
        }

        public static void DownloadVideo(string Uri)
        {
            // TODO
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
