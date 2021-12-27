using System.Collections.Generic;
using System.Text;

namespace ADLCore.Epub
{
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
}