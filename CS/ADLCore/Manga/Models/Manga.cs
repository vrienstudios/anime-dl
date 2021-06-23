using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using ADLCore.Ext;
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

        public void LoadChaptersFromADL(string adlLoc, ref ZipArchive zip)
        {
            List<MangaChapter> chapterlist = new List<MangaChapter>();
            string[] chapters = zip.GetEntriesUnderDirectoryToStandardString("Chapters/");
            int id = 0;
            foreach(string chp in chapters)
            {
                MangaChapter chap = new MangaChapter();
                chap.ChapterName = chp;
                using (StreamReader sr = new StreamReader(zip.GetEntry("Chapters/" + chp).Open()))
                {
                    string b;
                    List<Epub.Image> images = new List<Epub.Image>();
                    while((b = sr.ReadLine()) != null)
                        images.Add(Epub.Image.GenerateImageFromByte(Convert.FromBase64String(b), id++.ToString()));

                    chap.Images = images.ToArray();
                }
                chapterlist.Add(chap);
            }
            Chapters = chapterlist.ToArray();
        }
    }
}
