using System;
using System.Collections.Generic;
using System.Text;

namespace ADLCore.Epub
{
    public class OPFMetaData
    {
        List<Meta> metadata;

        public OPFMetaData(string title, string author, string bookid, string cover, string moddate, string publisher = null)
        {
            Meta Title = new Meta($">{title}", "title", MetaType.dc);
            Meta Language = new Meta(">en_US", "language", MetaType.dc);
            Meta Author = new Meta($"opf:role=\"auth\" opf:file-as=\"{author}\">{author}", "creator", MetaType.dc);
            Meta Identifier = new Meta($"id=\"BookID\" opf:scheme=\"URI\">{bookid}", "identifier", MetaType.dc);
            Meta pub = new Meta(">" + publisher, "publisher", MetaType.dc);
            Meta _cover = new Meta("cover", "cover");
            Meta creator = new Meta("1.0f", author);
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
}
