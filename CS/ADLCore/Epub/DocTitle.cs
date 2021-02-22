using System;
using System.Collections.Generic;
using System.Text;

namespace ADLCore.Epub
{
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
}
