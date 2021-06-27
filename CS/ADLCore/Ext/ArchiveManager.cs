using ADLCore.Video.Constructs;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;

namespace ADLCore.Ext
{
    public class ArchiveManager
    {
        System.IO.Stream insideStream;
        ZipArchiveEntry[][] entries; // MT
        public ZipArchive zapive;
        Random rng = new Random();
        public argumentList args;

        public void InitializeZipper(string loc, bool dc = false)
        {
            insideStream = new FileStream(loc, dc ? FileMode.Open : FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
            zapive = new ZipArchive(insideStream, ZipArchiveMode.Update, true);
        }

        public void InitializeZipper(System.IO.Stream stream)
        {
            zapive = new ZipArchive(stream, ZipArchiveMode.Update, true);
        }

        public void InitReadOnlyStream(string loc)
        {
            insideStream = new FileStream(loc, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
            zapive = new ZipArchive(insideStream, ZipArchiveMode.Read, false);
        }

        // Necessary so that memory doesn't explode to over 5gb due to "Update" stream issues.
        //
        public void InitWriteOnlyStream(string loc)
        {
            insideStream = new FileStream(loc, FileMode.Create, FileAccess.Write, FileShare.Read);
            zapive = new ZipArchive(insideStream, ZipArchiveMode.Create, false);
        }

        bool exo = false;
        public void UpdateStream()
        {
            while (exo)
                Thread.Sleep(rng.Next(100, 700));
            exo = true;
            insideStream.Flush();
            zapive.Dispose();
            zapive = new ZipArchive(insideStream, ZipArchiveMode.Update, false);
            exo = false;
        }

        public void updateStreamN()
        {
            insideStream.Flush();
        }

        public void CloseStream()
        {
            insideStream.Flush();
            zapive.Dispose();
            insideStream.Dispose();
        }
        public void bluntClose()
        {
            insideStream.Dispose();
            zapive.Dispose();
        }

        private void ZipArchiveFinish(int i) // MT 
        {
            ZipArchiveEntry[] archive = entries[i];
            while (exo)
                Thread.Sleep(rng.Next(100, 700));
            foreach (ZipArchiveEntry entry in archive)
            {
                exo = true;
                using (StreamWriter sw = new StreamWriter(zapive.CreateEntry(entry.FullName).Open()))
                using (StreamReader sr = new StreamReader(entry.Open()))
                    sw.Write(sr.ReadToEnd());
                exo = false;
            }
            UpdateStream();
        }

        //IMAGES ONLY
        public void AddContentToArchive(string name, List<Byte[]> bytes)
        {
            using (StreamWriter tw = new StreamWriter(zapive.CreateEntry($"Chapters/{name}").Open()))
            {
                foreach(Byte[] barr in bytes)
                    tw.WriteLine(Convert.ToBase64String(barr));
            }

            ////UpdateStream();
        }
    }
}
