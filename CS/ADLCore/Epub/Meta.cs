using System;
using System.Collections.Generic;
using System.Text;

namespace ADLCore.Epub
{
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
