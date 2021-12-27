using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace ADLCore.Ext
{
    public static class ZipE
    {
        public static ZipArchiveEntry[] GetEntriesUnderDirectory(this ZipArchive zap, string search)
            => zap.Entries.ToList().Where(x => x.FullName.Contains(search) && x.FullName.Length > search.Length)
                .ToArray();

        public static string[] GetEntriesUnderDirectoryToString(this ZipArchive zap, string search)
        {
            ZipArchiveEntry[] zappo = zap.GetEntriesUnderDirectory(search);
            string[] arr = new string[zappo.Length];
            for (int idx = 0; idx < zappo.Length; idx++)
                arr[idx] = zappo[idx].FullName; //Fullname fails.
            return arr;
        }

        //Searches for the path given and retrieves an array of directories under the path.
        public static string[] GetEntriesUnderDirectoryToStandardString(this ZipArchive zap, string search)
        {
            ZipArchiveEntry[] zappo = zap.GetEntriesUnderDirectory(search);
            string[] arr = new string[zappo.Length];
            for (int idx = 0; idx < zappo.Length; idx++)
                arr[idx] = new string(zappo[idx].FullName.Skip(search.Length).ToArray());
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