using System;
using System.Collections.Generic;
using System.Text;

namespace ADLCore.Epub
{
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
}
