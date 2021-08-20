using ADLCore.Epub;
using ADLCore.Ext;
using System;
using System.Collections.Generic;
using System.Text;

namespace ADLCore.Manga.Models
{
    public class MangaChapter
    {
        public MangaChapter()
        {
            content = new TiNodeList();
        }

        public string ChapterName;
        public string linkTo;
        public TiNodeList content;
        public bool existing = false;
    }
}
