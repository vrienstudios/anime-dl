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

        private static String downloadPart(String uri)
        {
            Storage.wc.DownloadFile(uri, $"{Directory.GetCurrentDirectory()}\\Vidstreaming.part");
            id++;
            return $"{Directory.GetCurrentDirectory()}\\Vidstreaming.part";
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
