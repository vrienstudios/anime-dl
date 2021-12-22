using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using ADLCore.Novels.Models;
using ADLCore.SiteFolder;
using ADLCore.Video.Constructs;
using Stream = System.IO.Stream;

namespace ADLCore.Ext
{
    public class ADLArchiveManager
    {
        public static Tuple<SiteBase, MetaData, Book, HentaiVideo> GetADLInformation(string adl)
        {
            ArchiveManager am = new ArchiveManager();
            am.InitReadOnlyStream(adl);
            string[] mainADL;
            using (StreamReader sr = new StreamReader(am.zapive.GetEntry("main.adl").Open()))
                mainADL = sr.ReadToEnd().Split('\r', '\n', StringSplitOptions.None);
            MetaData metaData = MetaData.GetMeta(mainADL);
            am.CloseStream();
            SiteBase siteBase = metaData.url.SiteFromString();

            if (metaData.type == "nvl")
            {
                Book bk = new Book();
                bk.LoadFromADL(adl);
                return new Tuple<SiteBase, MetaData, Book, HentaiVideo>(siteBase, metaData, bk, null);
            }

            if (metaData.type == "ani")
            {
                throw new NotImplementedException("CAN NOT LOAD ANI ADLS YET");
            }

            return new Tuple<SiteBase, MetaData, Book, HentaiVideo>(siteBase, metaData, null, null);
        }
    }

    public class ArchiveManager
    {
        Stream insideStream;
        ZipArchiveEntry[][] entries; // MT
        public ZipArchive zapive;
        readonly Random rng = new Random();
        public argumentList args;
        private string l;

        public void InitializeZipper(string loc, bool dc = false)
        {
            insideStream = new FileStream(loc, dc ? FileMode.Open : FileMode.Create, FileAccess.ReadWrite,
                FileShare.Read);
            l = loc;
            zapive = new ZipArchive(insideStream, ZipArchiveMode.Update, true);
        }

        public void InitializeZipper(Stream stream)
        {
            zapive = new ZipArchive(stream, ZipArchiveMode.Update, true);
        }

        public void InitReadOnlyStream(string loc)
        {
            insideStream = new FileStream(loc, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
            insideStream.Seek(0, SeekOrigin.Begin);
            zapive = new ZipArchive(insideStream, ZipArchiveMode.Read, true);
            l = loc;
        }

        // Necessary so that memory doesn't explode to over 5gb due to "Update" stream issues.
        //
        public void InitWriteOnlyStream(string loc)
        {
            insideStream = new FileStream(loc, FileMode.Create, FileAccess.Write, FileShare.Read);
            zapive = new ZipArchive(insideStream, ZipArchiveMode.Create, true);
            l = loc;
        }

        bool exo;

        public void UpdateStream(ZipArchiveMode mode = ZipArchiveMode.Update, bool leaveOpen = false)
        {
            while (exo)
                Thread.Sleep(rng.Next(100, 700));
            exo = true;
            zapive.Dispose();
            insideStream.Dispose();
            insideStream = new FileStream(l, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
            zapive = new ZipArchive(insideStream, mode, leaveOpen);
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
        public void AddContentToArchive(string name, List<Byte[]> bytes, Action callBack = null)
        {
            using (StreamWriter tw = new StreamWriter(zapive.CreateEntry($"Chapters/{name}").Open()))
            {
                foreach (Byte[] barr in bytes)
                    tw.WriteLine(Convert.ToBase64String(barr));
            }

            if (callBack == null) updateStreamN();
            else callBack();
        }
    }
}