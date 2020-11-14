using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VidStreamIORipper.Sites.VidStreaming
{
    static class ThreadAllocationTest
    {   
        public static Char[] downloadLinks = new char[0];
        static Thread[] iThreads = new Thread[0];
        public static bool dwS = false;
        static int cDownloads = 0;

        public static void TestD(string ui)
        {
            Thread.Sleep(1000);
            Console.WriteLine("TESTE: " + ui);
            return;
        }
        public static void StartDownload()
        {
            dwS = true;
            int i = 0;
            for (uint idx = 0; idx != downloadLinks.Length; idx++)
            {
                string ix = downloadLinks[idx].ToString();
                Thread ab = new Thread(() => TestD(ix));
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
                        iThreads[id] = new Thread(() => TestD(ix));
                        iThreads[id].Start();
                    }
                }
                Thread.Sleep(500);
            }
        }
    }
}
