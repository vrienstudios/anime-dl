using System.Collections.Generic;
using System.Text;

namespace ADLCore.Epub
{
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
}
