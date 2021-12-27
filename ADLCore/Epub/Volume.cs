using System;
using System.Collections.Generic;
using System.Text;

namespace ADLCore.Epub
{
    //https://www.w3.org/publishing/epub3/epub-packages.html#sec-package-nav-def-types-intro
    public class Volume
    {
        public string name;
        public List<Page> pages;
    }
}