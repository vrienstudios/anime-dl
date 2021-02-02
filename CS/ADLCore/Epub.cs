using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.IO.Compression;
using System.Threading;

namespace ADLCore
{
    public enum MediaType
    {
        xhtml,
        image,
        ncx,
        css
    }

    public enum MetaType
    {
        dc,
        meta,
    }

    public static class shorts
    {
        public static Dictionary<string, string> RemoveList = new Dictionary<string, string>();

        public static string mediaTypes(MediaType mt)
        {
            switch (mt)
            {
                case MediaType.image:
                    return "image/jpeg";
                case MediaType.xhtml:
                    return "application/xhtml+xml";
                case MediaType.ncx:
                    return "application/x-dtbncx+xml";
                case MediaType.css:
                    return "text/css";
            }
            return null;
        }

        public static List<Item> ToItems(this List<Page> pages)
        {
            List<Item> items = new List<Item>();
            foreach (Page pg in pages)
                items.Add(new Item(pg.id, pg.hrefTo, MediaType.xhtml));
            return items;
        }

        public static List<Item> ToItems(this List<Image> img)
        {
            List<Item> items = new List<Item>();
            foreach (Image imag in img)
                items.Add(new Item(imag.Name, imag.location, MediaType.image));
            return items;
        }

        /// <summary>
        /// Enumerates over all characters in the given string and replaces special chars, <, >, and & with escaped chars.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string MakeTextXHTMLReady(string text)
        {
            char[] chars = text.ToCharArray();
            StringBuilder sb = new StringBuilder();
            for (int idx = 0; idx < text.Length; idx++)
                switch (text[idx])
                {
                    case '<':
                        sb.Append("&lt;");
                        break;
                    case '>':
                        sb.Append("&gt;");
                        break;
                    case '&':
                        sb.Append("&amp;");
                        break;
                    default:
                        sb.Append(text[idx]);
                        continue;
                }
            return sb.ToString();
        }
    }

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
        public Epub(string title, string author = null, Image image = null, Uri toWork = null)
        {
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
            sw.Write(mimeType);

            sw.Close();
            echo = zf.CreateEntry("mimetype");
            memS = echo.Open();
            sw = new StreamWriter(memS);
            sw.Write(METAINF);
            sw.Close();



            if (image != null)
            {
                echo = zf.CreateEntry("OEBPS/cover.jpeg");
                using (BinaryWriter bw = new BinaryWriter(echo.Open()))
                    bw.Write(image.bytes, 0, image.bytes.Length);
            }

            creditFactory += $"<p>Link to source: <a href=\"{(toWork != null ? toWork.ToString() : "null")}\">{(toWork != null ? toWork.ToString() : "null")}</a></p><p>Work is by: {author}, go support them!</p><p>Converted to Epub by Chay#3670</p></body></html>";
            pages = new List<Page>();
            images = new List<Image>();
            AddPage(new Page() { id = "titlepage", Text = creditFactory });
        }

        public void AddPage(Page page)
        {
            page.id.Replace(" ", "_");
            page.FileName = $"{pages.Count}_{page.id}.xhtml";
            page.hrefTo = $"Text/{pages.Count}_{page.id}.xhtml";
            
            using(Stream echo = zf.CreateEntry($"OEBPS/Text/{page.FileName}").Open())
                using (StreamWriter sw = new StreamWriter(echo))
                    sw.Write(page.Text);


            if(page.images != null)
                foreach (Image img in page.images)
                    if (!images.Contains(img))
                        images.Add(img);

            pages.Add(page);
        }

        public void CreateEpub()
        {
            if (zf == null)
                throw new Exception("Can not run Create EPUB twice, access the fStream object instead.");
            OPF = new OPFPackage();
            OPF.metaData = new OPFMetaData(Title, author, "Chay#3670", "null", "2020-01-01");
            OPF.manifest = new Manifest();
            OPF.manifest.items = pages.ToItems();
            OPF.manifest.items.AddRange(images.ToItems());
            zf.CreateEntry("OEBPS/Pictures/");
            foreach(Image img in images)
            {
                using (BinaryWriter bw = new BinaryWriter(zf.CreateEntry($"OEBPS/Pictures/{img.Name}.jpeg").Open()))
                    bw.Write(img.bytes, 0, img.bytes.Length);
            }
            OPF.manifest.items.Add(new Item("cover", "cover.jpeg", MediaType.image));
            OPF.manifest.items.Add(new Item("css", "Styles/stylesheet.css", MediaType.css));
            OPF.manifest.items.Add(new Item("ncx", "toc.ncx", MediaType.ncx));
            OPF.spine = new Spine(OPF.manifest.items);

            //TOC
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
            using(FileStream fs = new FileStream(location + ".epub", FileMode.Create))
            {
                fStream.Seek(0, SeekOrigin.Begin);
                fStream.CopyTo(fs);
            }
        }
    }

    public class OPFPackage
    {
        public OPFMetaData metaData;
        public Manifest manifest;
        public Spine spine;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<package xmlns=\"http://www.idpf.org/2007/opf\" version=\"2.0\">");
            sb.AppendLine(metaData.ToString());
            sb.AppendLine(manifest.ToString());
            sb.AppendLine(spine.ToString());
            sb.AppendLine("<guide><reference type=\"cover\" title=\"cover\" href=\"cover.xhtml\"/></guide>");
            sb.AppendLine("</package>");
            return sb.ToString();
        }
    }

    public class Spine
    {
        List<Item> items;
        public Spine(List<Item> items)
        {
            this.items = items;
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<spine toc=\"ncx\">");
            foreach (Item item in this.items)
                sb.AppendLine($"<itemref idref=\"{item.id}\"/>");
            sb.AppendLine("</spine>");
            return sb.ToString();
        }
    }

    public class OPFMetaData
    {
        List<Meta> metadata;

        public OPFMetaData(string title, string author, string bookid, string cover, string moddate)
        {
            Meta Title = new Meta($">{title}", "title", MetaType.dc);
            Meta Language = new Meta(">en_US", "language", MetaType.dc);
            Meta Author = new Meta($"opf:role=\"auth\" opf:file-as=\"{author}\">{author}", "creator", MetaType.dc);
            Meta Identifier = new Meta($"id=\"BookID\" opf:scheme=\"URI\">{bookid}", "identifier", MetaType.dc);
            Meta pub = new Meta(">Chay#3670", "publisher", MetaType.dc);
            Meta _cover = new Meta("cover", "cover");
            Meta creator = new Meta("1.0f", "VrienV");
            Meta date = new Meta($"xmlns:opf=\"http://www.idpf.org/2007/opf\" opf:event=\"modification\">{ DateTime.Now }", "date", MetaType.dc);

            metadata = new List<Meta>();

            metadata.AddRange(new Meta[] { Title, Language, Author, Identifier, _cover, creator });
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<metadata xmlns:opf=\"http://www.idpf.org/2007/opf\" xmlns:dc=\"http://purl.org/dc/elements/1.1/\">");
            foreach (Meta meta in metadata)
                sb.AppendLine(meta.ToString());
            sb.AppendLine("</metadata>");
            return sb.ToString();
        }
    }
    public class Manifest
    {
        public List<Item> items;

        public Manifest() =>
            items = new List<Item>();

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<manifest>");
            foreach (Item it in items)
                sb.Append(it.ToString());
            sb.AppendLine("</manifest>");
            return sb.ToString();
        }
    }

    public class Item
    {
        public string id, href;
        MediaType mediaType;

        public Item(string id, string href, MediaType mediaType)
        {
            this.id = id; this.href = href; this.mediaType = mediaType;
        }

        public override string ToString()
            => $"<item id=\"{id}\" href=\"{href}\" media-type=\"{shorts.mediaTypes(mediaType)}\"/>";
    }

    /// <summary>
    /// JPG only please.
    /// </summary>
    public class Image
    {
        public string Name;
        // Location is set when exporting to epub
        public string location;
        public Byte[] bytes;

        public static Image LoadImageFromFile(string name, string location)
            => new Image { Name = name, bytes = File.ReadAllBytes(location) };

        public static Image GenerateImageFromByte(Byte[] bytes, string name)
            => new Image { Name = name, location = $"../Pictures/{name}", bytes = bytes};
        //<div class="svg_outer svg_inner"><svg xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" height="99%" width="100%" version="1.1" preserveAspectRatio="xMidYMid meet" viewBox="0 0 1135 1600"><image xlink:href="../Pictures/1483348780 329510 original" width="1135" height="1600"/></svg></div>
        public override string ToString()
            => $"<div class=\"svg_outer svg_inner\"><svg xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" height=\"99%\" width=\"100%\" version=\"1.1\" preserveAspectRatio=\"xMidYMid meet\" viewBox=\"0 0 1135 1600\"><image xlink:href=\"{location}\" width=\"1135\" height=\"1600\"/></svg></div>";

    }

    public class Page
    {
        public string id;
        public string Text;
        public string FileName;
        public string hrefTo;
        public Image[] images;

        public static Page AutoGenerate(string pageText, string title, Image[] images = null)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<!DOCTYPE html PUBLIC \" -//W3C//DTD XHTML 1.1//EN\"\n\"http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd\">\n");
            sb.AppendLine("<html xmlns=\"http://www.w3.org/1999/xhtml\">\n<head><title></title><link href=\"../Styles/stylesheet.css\" type=\"text/css\" rel=\"stylesheet\"/></head>");
            if (pageText != string.Empty)
            {
                pageText = shorts.MakeTextXHTMLReady(pageText);
                foreach (KeyValuePair<string, string> str in shorts.RemoveList)
                    pageText = pageText.Replace(str.Key, str.Value);
                sb.AppendLine($"<body>\n<h1 class=\"entry-title\">{title}</h1><p></p>");
                string[] st = pageText.Split(new string[] { "\r", "\n", "\r\n" }, StringSplitOptions.None);
                foreach (string str in st)
                    sb.AppendLine($"<p>{str}</p>");
            }
            if (images != null)
                foreach (Image img in images)
                    sb.AppendLine(img.ToString());
            sb.AppendLine("</body></html>");
            return new Page() { id = title, Text = sb.ToString(), FileName = title, images = images };
        }
    }

    public class NCX
    {
        StringBuilder sb = new StringBuilder();
        public TOCHeader header;
        public DocTitle title;
        public NavMap map;

        public NCX()
        {
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sb.AppendLine("<!DOCTYPE ncx PUBLIC \" -//NISO//DTD ncx 2005-1//EN\"\n\"http://www.daisy.org/z3986/2005/ncx-2005-1.dtd\"><ncx version = \"2005-1\" xmlns = \"http://www.daisy.org/z3986/2005/ncx/\" >");
        }

        public string GenerateTOCNCXFile()
        {
            sb.AppendLine(header.ToString());
            sb.AppendLine(title.ToString());
            sb.AppendLine(map.ToString());
            sb.AppendLine("</ncx>");
            return sb.ToString();
        }
    }

    public class TOCHeader
    {
        List<Meta> metaContent;

        public TOCHeader()
        {
            metaContent = new List<Meta>();
        }

        public void AddMeta(Meta metacontent)
            => metaContent.Add(metacontent);

        public void AddMeta(string a, string b)
            => metaContent.Add(new Meta(a, b));

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<head>");
            foreach (Meta meta in metaContent)
                sb.AppendLine(meta.ToString());
            sb.AppendLine("</head>");
            return sb.ToString();
        }
    }

    public class DocTitle
    {
        string docName;
        public DocTitle(string name)
            => docName = name;
        public override string ToString()
            => $"<docTitle><text>{docName}</text></docTitle>";

        public override bool Equals(object obj)
            => docName == obj;
    }

    public class NavMap
    {
        public List<NavPoint> Points;
        public NavMap() => this.Points = new List<NavPoint>();
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<navMap>");
            foreach (NavPoint point in Points)
                sb.AppendLine($"<navPoint id=\"navPoint-{point.id}\" playOrder=\"{point.playOrder}\"><navLabel><text>{point.text}</text></navLabel><content src=\"{point.source}\"/></navPoint>");
            sb.AppendLine("</navMap>");
            return sb.ToString();
        }
    }

    public class NavPoint
    {
        public string id, playOrder;
        public string text, source;
    }

    /// <summary>
    /// If metaType DC content is the other variables of the data, e.x content = "name=\"coolio\""
    /// </summary>
    public class Meta
    {
        MetaType metaType;
        string metaHeader;
        public Meta(string content, string name, MetaType mt = MetaType.meta)
        {
            metaType = mt;
            if (mt == MetaType.meta)
                metaHeader = $"<meta content=\"{content}\" name=\"{name}\"/>";
            else
                metaHeader = $"<dc:{name} {content}</dc:{name}>";
        }

        public override string ToString() => metaHeader;
    }
}
