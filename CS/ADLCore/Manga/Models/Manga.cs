using System;
using System.Collections.Generic;
using System.Text;

namespace ADLCore.Manga.Models
{
    public class Manga
    {
        public MangaChapter[] Chapters;
        public Novels.Models.MetaData metaData;

        public void ExportToEpub(string location)
        {
            Epub.Epub e = new Epub.Epub(metaData.name, metaData.author, new Epub.Image() { bytes = metaData.cover });

            foreach (MangaChapter chapter in Chapters)
                e.AddPage(Epub.Page.AutoGenerate(null, chapter.ChapterName, chapter.Images));

            e.CreateEpub(null);
            e.ExportToEpub(location);
        }

        public void LoadChaptersFromADL(string adlLoc)
        {

        }
    }
}
