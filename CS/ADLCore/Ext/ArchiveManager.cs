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
            insideStream = new FileStream(loc, dc ? FileMode.Open : FileMode.Create);
            zapive = new ZipArchive(insideStream, ZipArchiveMode.Update, true);
        }

        public void InitializeZipper(System.IO.Stream stream)
        {
            zapive = new ZipArchive(stream, ZipArchiveMode.Update, true);
        }

        bool exo = false;
        public void UpdateStream()
        {
            while (exo)
                Thread.Sleep(rng.Next(100, 700));
            exo = true;
            insideStream.Flush();
            zapive.Dispose();
            zapive = new ZipArchive(insideStream, ZipArchiveMode.Update, true);
            exo = false;
        }

        public void CloseStream()
        {
            insideStream.Flush();
            zapive.Dispose();
            insideStream.Dispose();
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
            using (StreamWriter tw = new StreamWriter(zapive.CreateEntry($"Chapters/{name}.imc").Open()))
            {
                foreach(Byte[] barr in bytes)
                    tw.WriteLine(Convert.ToBase64String(barr));
            }

            UpdateStream();
        }
    }
}
