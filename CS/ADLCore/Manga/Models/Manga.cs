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

        public void ExportToEpub(string location, ref ZipArchive zapive)
        {
            
            Epub.Epub e = new Epub.Epub(metaData.name, metaData.author, new Epub.Image() { bytes = metaData.cover });
            int id = 0;

            foreach (MangaChapter chapter in Chapters)
            {
                using (StreamReader sr = new StreamReader(zapive.GetEntry("Chapters/" + chapter.ChapterName).Open()))
                {
                    string b;
                    List<Epub.Image> images = new List<Epub.Image>();
                    while ((b = sr.ReadLine()) != null)
                        images.Add(Epub.Image.GenerateImageFromByte(Convert.FromBase64String(b), id++.ToString()));

                    chapter.Images = images.ToArray();
                }
                e.AddPage(Epub.Page.AutoGenerate(null, chapter.ChapterName, chapter.Images));
                chapter.Images = null;
                GC.Collect();
            }

            e.CreateEpub(null);
            e.ExportToEpub(location);
        }

        public void ExportMetaData(ref ZipArchive zip)
        {
            using(StreamWriter sw = new StreamWriter(zip.CreateEntry("main.adl").Open()))
                sw.Write(metaData.ToString());
            using (BinaryWriter bw = new BinaryWriter(zip.CreateEntry("cover.jpeg").Open()))
                bw.Write(metaData.cover, 0, metaData.cover.Length);
        }

        public void LoadChaptersFromADL(ref ZipArchive zip)
        {
            List<MangaChapter> chapterlist = new List<MangaChapter>();
            string[] chapters = zip.GetEntriesUnderDirectoryToStandardString("Chapters/");
            int id = 0;
            foreach(string chp in chapters)
            {
                MangaChapter chap = new MangaChapter();
                chap.ChapterName = chp;
               /* using (StreamReader sr = new StreamReader(zip.GetEntry("Chapters/" + chp).Open()))
                {
                    string b;
                    List<Epub.Image> images = new List<Epub.Image>();
                    while((b = sr.ReadLine()) != null)
                        images.Add(Epub.Image.GenerateImageFromByte(Convert.FromBase64String(b), id++.ToString()));

                    chap.Images = images.ToArray();
                }*/
                chapterlist.Add(chap);
            }
            Chapters = chapterlist.ToArray();
        }

        public void LoadMangaFromADL(ref ZipArchive zip)
        {
            LoadChaptersFromADL(ref zip);
            string[] arr;
            Byte[] cover;
            using (StreamReader sr = new StreamReader(zip.GetEntry("main.adl").Open()))
                arr = sr.ReadToEnd().Split('\n');
            using (BinaryReader br = new BinaryReader(zip.GetEntry("cover.jpeg").Open()))
            using (MemoryStream ms = new MemoryStream())
            {
                br.BaseStream.CopyTo(ms);
                cover = ms.ToArray();
            }
            this.metaData = Novels.Models.MetaData.GetMeta(arr);
            this.metaData.cover = cover;
        }
    }
}
