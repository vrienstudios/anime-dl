using System;
using System.Collections.Generic;
using System.Text;
using ADLCore.Manga.Models;
using ADLCore.Novels.Models;

namespace ADLCore.Ext
{
    public class ADLReader<T>
    {
        private ArchiveManager writer;
        private ArchiveManager reader;

        public void OpenADL(string path)
        {
            writer = new ArchiveManager();
            reader = new ArchiveManager();
            
            writer.InitWriteOnlyStream(path);
            reader.InitReadOnlyStream(path);
        }

        public T GetNextChapterAsObject()
        {
            if (typeof(T) == typeof(Chapter))
            {
                Chapter chp = new Chapter();
                
                return (T)Convert.ChangeType(chp, typeof(T));
            }
            else if (typeof(T) == typeof(MangaChapter))
            {
                MangaChapter chp = new MangaChapter();
                return (T) Convert.ChangeType(chp, typeof(T));
            }
            else
                throw new Exception("Bad Type");
        }
    }
}
