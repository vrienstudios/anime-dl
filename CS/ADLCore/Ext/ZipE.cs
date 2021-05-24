using ADLCore.Novels.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace ADLCore.Ext
{
    public static class ZipE
    {
        public static ZipArchiveEntry[] GetEntriesUnderDirectory(this ZipArchive zap, string search)
            => zap.Entries.ToList().Where(x => x.FullName.Contains(search)).ToArray();

        public static string[] GetEntriesUnderDirectoryToString(this ZipArchive zap, string search)
        {
            ZipArchiveEntry[] zappo = zap.GetEntriesUnderDirectory(search);
            string[] arr = new string[zappo.Length];
            for(int idx = 0; idx < zappo.Length; idx++)
                arr[idx] = zappo[idx].FullName; //Fullname fails.
            return arr;
        }

        //Searches for the path given and retrieves an array of directories under the path.
        public static string[] GetEntriesUnderDirectoryToStandardString(this ZipArchive zap, string search)
        {
            ZipArchiveEntry[] zappo = zap.GetEntriesUnderDirectory(search);
            string[] arr = new string[zappo.Length];
            for (int idx = 0; idx < zappo.Length; idx++)
                arr[idx] = zappo[idx].Name;
            return arr;
        }

        public static Byte[] GetAllBytes(this ZipArchiveEntry entry)
        {
            Stream s = entry.Open();
            MemoryStream ms = new MemoryStream();
            s.CopyTo(ms);
            return ms.ToArray();
        }

        public static string GetString(this ZipArchiveEntry entry)
        {
            StreamReader s = new StreamReader(entry.Open());
            return s.ReadToEnd();
        }
    }
}
