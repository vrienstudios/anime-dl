using ADLCore.Ext;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace ADLCore.Epub
{
    /// <summary>
    /// Epub class used for generating and exporting epubs to memory and disk.
    /// </summary>
    public class Epub
    {
        public string Title, author;
        public string workingDirectory, OEBPSDIR;
        public string mimeType = "application/epub+zip";
        public string METAINF = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><container version = \"1.0\" xmlns=\"urn:oasis:names:tc:opendocument:xmlns:container\"><rootfiles><rootfile full-path=\"OEBPS/content.opf\" media-type=\"application/oebps-package+xml\"/></rootfiles></container>";
        public string creditFactory = "<?xml version='1.0' encoding='utf-8'?><html xmlns=\"http://www.w3.org/1999/xhtml\" xml:lang=\"en\"><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=UTF-8\"/><meta name=\"calibre:cover\" content=\"false\"/><title>Tribute</title><style type=\"text/css\" title=\"override_css\">@page {padding: 0pt; margin:0pt}\nbody { text-align: center; padding:0pt; margin: 0pt; }</style></head><body><div><svg xmlns = \"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" version=\"1.1\" width=\"100%\" height=\"100%\" viewBox=\"0 0 741 1186\" preserveAspectRatio=\"none\"><image width = \"741\" height=\"1186\" xlink:href=\"../cover.jpeg\"/></svg></div>";

        public string xhtmlCover = "<?xml version='1.0' encoding='utf-8'?><html xmlns=\"http://www.w3.org/1999/xhtml\" xml:lang=\"en\"><head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=UTF-8\"/><meta name=\"calibre:cover\" content=\"true\"/><title>Cover</title><style type=\"text/css\" title=\"override_css\">@page {padding: 0pt; margin:0pt}\nbody { text-align: center; padding:0pt; margin: 0pt; }</style></head><body><div><svg xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" version=\"1.1\" width=\"100%\" height=\"100%\" viewBox=\"0 0 741 1186\" preserveAspectRatio=\"none\"><image width=\"741\" height=\"1186\" xlink:href=\"cover.jpeg\"/></svg></div></body></html>";

        public NCX ToC;
        public OPFPackage OPF;

        List<Page> pages;
        List<Image> images;

        ZipArchive zf;
        public Stream fStream;

        /// <summary>
        /// Epub class for the generation and exportation of epubs in memory and to disk.
        /// </summary>
        /// <param name="title">Title of the novel, what epub readers display it as</param>
        /// <param name="author">Author of this work, what epub readers can sort by</param>
        /// <param name="image">This is the cover of the novel; please only use ADLCore.Epub.Image</param>
        /// <param name="toWork">URL displayed on the first page of the novel summary view, link to page.</param>
        public Epub(string title, string author = null, Image image = null, Uri toWork = null)
        {
            // This Epub creator creates an EPUB in memory rather than disk.
            fStream = new MemoryStream();
            zf = new ZipArchive(fStream, ZipArchiveMode.Create, true);

            Title = title; this.author = author;

            workingDirectory = $"{Directory.GetCurrentDirectory()}\\Epubs\\{title}";

            zf.CreateEntry("OEBPS/");
            zf.CreateEntry("OEBPS/Text/");
            zf.CreateEntry("OEBPS/Styles/");

            zf.CreateEntry("META-INF/");

            ZipArchiveEntry echo = zf.CreateEntry("META-INF/container.xml");

            Stream memS = echo.Open();

            StreamWriter sw = new StreamWriter(memS);
            sw.Write(METAINF);

            sw.Close();
            echo = zf.CreateEntry("mimetype");
            memS = echo.Open();
            sw = new StreamWriter(memS);
            sw.Write(mimeType);
            sw.Close();



            if (image != null)
            {
                echo = zf.CreateEntry("OEBPS/cover.jpeg");
                using (BinaryWriter bw = new BinaryWriter(echo.Open()))
                    bw.Write(image.bytes, 0, image.bytes.Length);
            }

            pages = new List<Page>();
            images = new List<Image>();
        }

        public void AddPage(Page page)
        {
            page.id.Replace(" ", "_");
            page.FileName = $"{pages.Count}_{page.id}.xhtml";
            page.hrefTo = $"Text/{pages.Count}_{page.id}.xhtml";

            using (Stream echo = zf.CreateEntry($"OEBPS/Text/{page.FileName}").Open())
            using (StreamWriter sw = new StreamWriter(echo))
                sw.Write(page.Text);


            if (page.images != null)
                foreach (Image img in page.images)
                    if (!images.Contains(img))
                        images.Add(img);

            pages.Add(page);
        }

        public void CreateEpub(OPFMetaData opf)
        {
            if (zf == null)
                throw new Exception("Can not run Create EPUB twice, access the fStream object instead.");

            OPF = new OPFPackage();
            OPF.metaData = opf;
            OPF.manifest = new Manifest();
            OPF.manifest.items = pages.ToItems();
            OPF.manifest.items.AddRange(images.ToItems());

            zf.CreateEntry("OEBPS/Pictures/");
            foreach (Image img in images)
            {
                using (BinaryWriter bw = new BinaryWriter(zf.CreateEntry($"OEBPS/Pictures/{img.Name}.jpeg").Open()))
                    bw.Write(img.bytes, 0, img.bytes.Length);
            }

            OPF.manifest.items.Add(new Item("cover", "cover.jpeg", MediaType.image));
            OPF.manifest.items.Add(new Item("ncx", "toc.ncx", MediaType.ncx));
            OPF.spine = new Spine(OPF.manifest.items);

            //TOC | Table of Contents
            ToC = new NCX();
            ToC.header = new TOCHeader();
            ToC.header.AddMeta("VrienCo", "dtb:uid");
            ToC.header.AddMeta("1", "dtb:depth");
            ToC.header.AddMeta("0", "dtb:totalPageCount");
            ToC.header.AddMeta("0", "dtb:maxPageNumber");

            ToC.title = new DocTitle(Title);
            ToC.map = new NavMap();

            for (int idx = 0; idx < pages.Count; idx++)
                ToC.map.Points.Add(new NavPoint() { text = pages[idx].id, id = $"navPoint-{idx}", playOrder = idx.ToString(), source = pages[idx].hrefTo });

            Stream echo = zf.CreateEntry("OEBPS/content.opf").Open();
            StreamWriter sw = new StreamWriter(echo);
            sw.Write(OPF.ToString());
            sw.Close();
            echo = zf.CreateEntry("OEBPS/toc.ncx").Open();
            sw = new StreamWriter(echo);
            sw.Write(ToC.GenerateTOCNCXFile());
            sw.Close();
            echo = zf.CreateEntry("OEBPS/cover.xhtml").Open();
            sw = new StreamWriter(echo);
            sw.Write(xhtmlCover);
            sw.Close();
            zf.Dispose();
            zf = null;
        }

        /// <summary>
        /// Adds .epub extension to every one exported.
        /// </summary>
        /// <param name="location"></param>
        public void ExportToEpub(string location)
        {
            Directory.CreateDirectory(location.TrimToSlash('\\')); // Ensure directory exists and trim off filename.
            using (FileStream fs = new FileStream(location + ".epub", FileMode.Create))
            {
                fStream.Seek(0, SeekOrigin.Begin);
                fStream.CopyTo(fs);
            }
        }
    }
}
