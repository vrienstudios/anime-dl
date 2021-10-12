using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using System.Text;

namespace ADLCore.Epub
{
    public class NavMap
    {
        public List<NavPoint> Points;
        public NavMap() => this.Points = new List<NavPoint>();

        //https://www.w3.org/publishing/epub3/epub-packages.html#sec-manifest-elem
        //https://www.w3.org/publishing/epub3/epub-packages.html#sec-package-nav-def-types-intro

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<navMap>");
            foreach (NavPoint point in Points)
            {
                if (point.isGrp)
                {
                    sb.AppendLine(
                        $"<navPoint id=\"navPoint-{point.id}\" playOrder=\"{point.playOrder}\"><navLabel><text>{point.text}</text></navLabel>");
                    for (int idx = 0; idx < point.sources.Length; idx++)
                        sb.AppendLine(
                            $"<navPoint id=\"navPoint-{point.id}-{idx}\" playOrder=\"{idx}\"><navLabel><text>{point.titles[idx]}</text></navLabel><content src=\"{point.sources[idx]}\"/></navPoint>");
                    sb.AppendLine("</navPoint>");
                    continue;
                }
                sb.AppendLine(
                    $"<navPoint id=\"navPoint-{point.id}\" playOrder=\"{point.playOrder}\"><navLabel><text>{point.text}</text></navLabel><content src=\"{point.source}\"/></navPoint>");
            }

            sb.AppendLine("</navMap>");
            return sb.ToString();
        }
    }

    public class NavPoint
    {
        public string id, playOrder;
        public string text, source;
        public bool isGrp;
        public string[] titles;
        public string[] sources;
    }
}
