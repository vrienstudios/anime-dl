using System;
using System.Collections.Generic;
using System.Text;

namespace ADLCore.Manga.Models
{
    public class MangaChapter
    {
        public string ChapterName;
        public string linkTo;
        public Epub.Image[] Images;
        public bool existing = false;
    }
}
