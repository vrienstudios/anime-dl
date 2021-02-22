using System;
using System.Collections.Generic;
using System.Text;

namespace ADLCore.Epub
{
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
}
