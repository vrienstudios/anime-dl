using System.Text;

namespace ADLCore.Epub
{
    public class NCX
    {
        StringBuilder sb = new StringBuilder();
        public TOCHeader header;
        public DocTitle title;
        public NavMap map;

        public NCX()
        {
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sb.AppendLine(
                "<!DOCTYPE ncx PUBLIC \" -//NISO//DTD ncx 2005-1//EN\"\n\"http://www.daisy.org/z3986/2005/ncx-2005-1.dtd\"><ncx version = \"2005-1\" xmlns = \"http://www.daisy.org/z3986/2005/ncx/\" >");
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
}