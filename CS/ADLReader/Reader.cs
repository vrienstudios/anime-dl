using System;
using System.Collections.Generic;
using System.Text;
using ADLCore.Ext;
using ADLCore.Manga.Models;
using ADLCore.Novels.Models;

namespace ADLReader
{
    public class Reader<T>
    {
        private ArchiveManager writer;
        private ArchiveManager reader;

        private int position;
        private bool isManga;

        private Manga mHost;
        private Book cHost;

        private MangaChapter mChapter;
        private Chapter cChapter;

        public Reader(string adlPath)
        {
            OpenADL(adlPath);
            if (typeof(T) == typeof(Manga))
                isManga = true;
            if (isManga)
            {
                mHost = new Manga();
                mHost.LoadChaptersFromADL(ref reader.zapive);
            }
            else
            {
                cHost = new Book();
                cHost.LoadFromADL(ref reader.zapive);
            }
        }

        public void OpenADL(string path)
        {
            writer = new ArchiveManager();
            reader = new ArchiveManager();

            writer.InitWriteOnlyStream(path);
            reader.InitReadOnlyStream(path);
        }

        public void Next()
        {
            if (isManga)
                mChapter = mHost.Chapters[position++];
            else
                cChapter = cHost.chapters[position++];
        }

        public string Title
        {
            get { return isManga ? mChapter.ChapterName : cChapter.parsedName; }
        }

        public TiNodeList CurrentNodeContent
        {
            get { return isManga ? mChapter.content : cChapter.content; }
        }
    }
}
