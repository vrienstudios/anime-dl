using System.Collections.Generic;
using System.Text;

namespace ADLCore.Epub
{
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
}
